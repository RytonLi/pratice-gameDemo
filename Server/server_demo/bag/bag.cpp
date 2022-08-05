#pragma once
#include "bag.h"
#include <stdio.h>
#include <stdlib.h>
#include "filedb/filedb.h"
#include "../codec/codec.h"
#include "../utils/utils.h"

static char s_send_buff[1024 * 64];

BagMgr::BagMgr() {

}
BagMgr::~BagMgr() {

}

bool BagMgr::init() {
	m_bagMap.clear();
	return true;
}


bool BagMgr::un_init() {
	for (auto it = m_bagMap.begin(); it != m_bagMap.end(); ++it) {
		delete it->second;
	}
	m_bagMap.clear();
	return true;
}

bool BagMgr::on_client_close(uv_tcp_t* client) {
	auto it = m_bagMap.find(client);
	if (it != m_bagMap.end()) {
		free(it->second);
		m_bagMap.erase(it);
	}
	return true;
}

bool BagMgr::add_bag(uv_tcp_t* client, const PlayerSaveData* playerData) {
	Bag* bag = new Bag();
	bag->PlayerID = playerData->syncdata().playerid();
	bag->Money = playerData->syncdata().money();
	bag->ItemMap.clear();
	for (int i = 0; i < playerData->syncdata().items_size(); i++) {
		BagItem cur = playerData->syncdata().items(i);
		bag->ItemMap.insert(make_pair(cur.id(), cur));
	}
	m_bagMap.insert(make_pair(client, bag));
	return true;
}

Bag* BagMgr::find_bag(uv_tcp_t* client) {
	Bag* bag = nullptr;
	auto it = m_bagMap.find(client);
	if (it != m_bagMap.end()) {
		bag = it->second;
	}
	return bag;
}

int BagMgr::get_money(uv_tcp_t* client) {
	int money = -1;
	if (m_bagMap.count(client) == 1) {
		money = m_bagMap[client]->Money;
	}
	return money;
}

//����������Ʒ�ĸ��£���Ʒ�ļ۸���ƷID����Ʒ��������
bool BagMgr::update_bag_byBuy(uv_tcp_t* client, int price, int itemID, int itemNum) {
	Bag* bag = nullptr;
	if (m_bagMap.count(client) == 0) {
		goto Exit0;
	}
	//��ȥ��Ӧ����
	bag = m_bagMap[client];
	bag->Money -= (price * itemNum);
	if (bag->Money < 0) {
		bag->Money = 0;
	}

	//�û�ԭ��û�и���Ʒ
	if (bag->ItemMap.count(itemID) == 0) {
		BagItem bitem;
		bitem.set_id(itemID);
		bitem.set_num(itemNum);
		bag->ItemMap.insert(make_pair(itemID, bitem));
		
		//����
		BagUpdate update;
		update.set_cmd(ITEM_ADD);
		update.mutable_item()->MergeFrom(bag->ItemMap[itemID]);
		bag->updateQueue.push(update);
	}
	//�û�ԭ���и���Ʒ
	else {
		int oldNum = bag->ItemMap[itemID].num();
		bag->ItemMap[itemID].set_num(oldNum + itemNum);

		//�޸�
		BagUpdate update;
		update.set_cmd(ITEM_MODIFY);
		update.mutable_item()->MergeFrom(bag->ItemMap[itemID]);
		bag->updateQueue.push(update);
	}

Exit0:
	return true;
}

bool BagMgr::on_updateBag_req(uv_tcp_t* client, const PlayerBagUpdateReq* req) {
	PlayerBagUpdateRsp rsp;
	Bag* bag = find_bag(client);
	int len;

	if (bag == nullptr || bag->PlayerID != req->playerid()) {
		rsp.set_result(-1);
		rsp.set_reason(convertToUTF8("��������쳣..."));
		goto Exit0;
	}

	if (bag->updateQueue.empty()) {
		rsp.set_result(-2);
		rsp.set_reason(convertToUTF8("�����������..."));
		goto Exit0;
	}

	rsp.set_result(0);
	rsp.set_reason(convertToUTF8("�������³ɹ�..."));
	while (!bag->updateQueue.empty()) {
		BagUpdate update = bag->updateQueue.front();
		bag->updateQueue.pop();
		rsp.add_updatelist()->MergeFrom(update);
	}

Exit0:
	fprintf(stdout, "--------------finish handle update bag: %d\n", rsp.result());
	//Ӧ����
	len = encode(s_send_buff, SERVER_BAGUPDATE_RSP, rsp.SerializeAsString().c_str(), rsp.ByteSize());
	sendData((uv_stream_t*)client, s_send_buff, len);
	return true;
}

bool BagMgr::on_money_req(uv_tcp_t* client, const PlayerMoneyReq* req) {
	PlayerMoneyRsp rsp;
	Bag* bag = find_bag(client);
	int len;

	if (bag == nullptr || bag->PlayerID != req->playerid()) {
		rsp.set_result(-1);
		rsp.set_reason(convertToUTF8("��������쳣..."));
		goto Exit0;
	}

	rsp.set_result(0);
	rsp.set_reason(convertToUTF8("��Ǯ���³ɹ�..."));
	rsp.set_money(bag->Money);

Exit0:
	fprintf(stdout, "--------------finish handle money: %d\n", rsp.result());
	//Ӧ����
	len = encode(s_send_buff, SERVER_MONEY_RSP, rsp.SerializeAsString().c_str(), rsp.ByteSize());
	sendData((uv_stream_t*)client, s_send_buff, len);
	return true;
}
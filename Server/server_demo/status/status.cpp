#pragma once
#include "status.h"
#include <stdio.h>
#include <stdlib.h>
#include "filedb/filedb.h"
#include "../codec/codec.h"
#include "../utils/utils.h"

static char s_send_buff[1024 * 64];

StatusMgr::StatusMgr() {

}
StatusMgr::~StatusMgr() {

}

bool StatusMgr::init() {
	m_statusMap.clear();
	return true;
}

bool StatusMgr::un_init() {
	for (auto it = m_statusMap.begin(); it != m_statusMap.end(); ++it) {
		delete it->second;
	}
	m_statusMap.clear();
	return true;
}

bool StatusMgr::on_client_close(uv_tcp_t* client) {
	auto it = m_statusMap.find(client);
	if (it != m_statusMap.end()) {
		free(it->second);
		m_statusMap.erase(it);
	}
	return true;
}

bool StatusMgr::add_status(uv_tcp_t* client, const PlayerSaveData* playerData) {
	Status* status = new Status();
	status->PlayerID = playerData->syncdata().playerid();
	status->PositionX = playerData->syncdata().positionx();
	status->PositionY = playerData->syncdata().positiony();
	status->Rotation = playerData->syncdata().rotation();
	status->IsWalking = playerData->syncdata().iswalking();
	m_statusMap.insert(make_pair(client, status));
	return true;
}

bool StatusMgr::on_status_req(uv_tcp_t* client, PlayerStatusReq* req) {
	PlayerStatusRsp rsp;
	Status* status = find_status(client);
	int len;

	if (status == nullptr || status->PlayerID != req->playerid()) {
		rsp.set_result(-1);
		rsp.set_reason(convertToUTF8("玩家数据异常..."));
		goto Exit0;
	}

	status->PositionX = req->positionx();
	status->PositionY = req->positiony();
	status->Rotation = req->rotation();
	status->IsWalking = req->iswalking();

	rsp.set_result(0);
	rsp.set_reason(convertToUTF8("玩家状态同步成功...");

Exit0:
	fprintf(stdout, "--------------finish handle create: %d\n", rsp.result());
	//应答结果
	len = encode(s_send_buff, SERVER_CREATE_RSP, rsp.SerializeAsString().c_str(), rsp.ByteSize());
	sendData((uv_stream_t*)client, s_send_buff, len);
	return true;
}

Status* StatusMgr::find_status(uv_tcp_t* client) {
	Status* status = nullptr;
	auto it = m_statusMap.begin();
	if (it != m_statusMap.end()) {
		status = it->second;
	}
	return status;
}
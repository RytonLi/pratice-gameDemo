#pragma once
#include "save.h"
#include <stdio.h>
#include <stdlib.h>
#include "filedb/filedb.h"
#include "../codec/codec.h"
#include "../utils/utils.h"
#include "../player/player.h"
#include "../bag/bag.h"
#include "../status/status.h"

extern PlayerMgr g_playerMgr;
extern StatusMgr g_statusMgr;
extern BagMgr g_bagMgr;

SaveMgr::SaveMgr() {

}

SaveMgr::~SaveMgr() {

}

bool SaveMgr::init() {
	m_saveMap.clear();
	return true;
}

bool SaveMgr::un_init() {
    for (auto it = m_saveMap.begin(); it != m_saveMap.end(); ++it) {
        delete it->second;
    }
    m_saveMap.clear();
    return true;
}

bool SaveMgr::on_client_close(uv_tcp_t* client) {
    save_player(client);
    auto it = m_saveMap.find(client);
    if (it != m_saveMap.end()) {
        free(it->second);
        m_saveMap.erase(it);
    }
    return true;
}

PlayerSaveData* SaveMgr::find_saveData(uv_tcp_t* client) {
    PlayerSaveData* saveData = nullptr;
    auto it = m_saveMap.find(client);
    if (it != m_saveMap.end()) {
        saveData = it->second;
    }
    return saveData;
}

bool SaveMgr::add_saveData(uv_tcp_t* client, const PlayerSaveData* playerData) {
    PlayerSaveData* saveData = new PlayerSaveData();
    saveData->MergeFrom(*playerData);
    m_saveMap.insert(make_pair(client, saveData));
    return true;
}

bool SaveMgr::save_allPlayer() {
    for (auto it = m_saveMap.begin(); it != m_saveMap.end(); it++) {
        save_player(it->first);
    }
    return true;
}

bool SaveMgr::save_player(uv_tcp_t* client) {
    PlayerSaveData* saveData = nullptr;
    int len;

    _update_player(client);
    saveData = find_saveData(client);
    if (saveData == nullptr) {
        goto Exit0;
    }

    len = save(saveData->syncdata().playerid().c_str(), saveData->SerializeAsString().c_str(), saveData->ByteSize());
    if (len < 0) {
        fprintf(stdout, "save player error with: %d\n", len);
        goto Exit0;
    }
    fprintf(stdout, "-----------save player %d success\n", client);

Exit0:
    return true;
}

bool SaveMgr::_update_player(uv_tcp_t* client) {
    PlayerSaveData* saveData = nullptr;
    Player* player = nullptr;
    Status* status = nullptr;
    Bag* bag = nullptr;
    
    saveData = find_saveData(client);
    if (saveData == nullptr) {
        goto Exit0;
    }
    
    player = g_playerMgr.find_player(client);
    if (player != nullptr) {
        saveData->set_password(player->Password);
        saveData->mutable_syncdata()->set_playerid(player->PlayerID);
        saveData->mutable_syncdata()->set_name(player->Name);
    }

    status = g_statusMgr.find_status(client);
    if (status != nullptr) {
        saveData->mutable_syncdata()->set_positionx(status->PositionX);
        saveData->mutable_syncdata()->set_positiony(status->PositionY);
        saveData->mutable_syncdata()->set_rotation(status->Rotation);
        saveData->mutable_syncdata()->set_iswalking(status->IsWalking);
    }

    bag = g_bagMgr.find_bag(client);
    if (status != nullptr) {
        saveData->mutable_syncdata()->set_money(bag->Money);
        saveData->mutable_syncdata()->clear_items();
        for (auto it = bag->ItemMap.begin(); it != bag->ItemMap.end(); it++) {
            saveData->mutable_syncdata()->add_items()->MergeFrom(it->second);
        }
    }

Exit0:
    return true;
}


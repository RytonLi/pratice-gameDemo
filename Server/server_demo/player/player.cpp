#pragma once
#include "player.h"
#include <stdio.h>
#include <stdlib.h>
#include "filedb/filedb.h"
#include "../codec/codec.h"
#include "../utils/utils.h"
#include "../status/status.h"
#include "../bag/bag.h"
#include "../shop/shop.h"
#include "../save/save.h"

static char s_send_buff[1024*64];
extern ServerCfg g_config;
extern StatusMgr g_statusMgr;
extern BagMgr g_bagMgr;
extern ShopMgr g_shopMgr;
extern SaveMgr g_saveMgr;

PlayerMgr::PlayerMgr() {

}

PlayerMgr::~PlayerMgr() {

}

bool PlayerMgr::init() {
    m_playerMap.clear();
    m_announce = g_config.default_announce;
    return true;
}

bool PlayerMgr::un_init() {
    for (auto it = m_playerMap.begin(); it != m_playerMap.end(); ++it) {
        delete it->second;
    }
    m_playerMap.clear();
    return true;
}

// 客户端拉取公告内容
bool PlayerMgr::announce_request(uv_tcp_t* client) {
    fprintf(stdout, "announce request\n");
    SyncAnnounce sync;
    int len = 0;
    sync.set_announce(m_announce);

    len = encode(s_send_buff, SERVER_ANNOUNCE_RSP, sync.SerializeAsString().c_str(), sync.ByteSize());

    return sendData((uv_stream_t*)client, s_send_buff, len);
}

// 根据playerID查找玩家
Player* PlayerMgr::find_player(string playerID) {
    for (auto it = m_playerMap.begin(); it != m_playerMap.end(); ++it) {
        if (it->second->PlayerID == playerID)
            return it->second;
    }
    return nullptr;
}

Player* PlayerMgr::find_player(uv_tcp_t* client) {
    Player* player = nullptr;
    auto it = m_playerMap.find(client);
    if (it != m_playerMap.end()) {
        player = it->second;
    }
    return player;
}

// 客户端断链，清除对应的玩家对象
bool PlayerMgr::on_client_close(uv_tcp_t* client) {
    auto it = m_playerMap.find(client);
    if (it != m_playerMap.end()) {
        free(it->second);
        m_playerMap.erase(it);
    }
    return true;
}

// 广播公告
bool PlayerMgr::broadcast_announce(string announce) {
    SyncAnnounce sync;
    int len = 0;

    m_announce = announce;
    sync.set_announce(announce);

    if (m_playerMap.empty())
        return true;

    len = encode(s_send_buff, SERVER_ANNOUNCE_RSP, sync.SerializeAsString().c_str(), sync.ByteSize());

    for (auto it = m_playerMap.begin(); it != m_playerMap.end(); ++it) {
        sendData((uv_stream_t*)it->first, s_send_buff, len);
    }
    return true;
}

// 玩家登录
bool PlayerMgr::player_login(uv_tcp_t* client, const PlayerLoginReq* req) {
    PlayerLoginRsp rsp;
    PlayerSaveData playerData;
    Player* player = nullptr;
    int len;

    //检查m_playerMap
    player = find_player(req->playerid());
    if (player != nullptr) {
        rsp.set_result(-1);
        rsp.set_reason(convertToUTF8("用户已登录..."));
        goto Exit0;
    }

    //根据id读取玩家数据
    len = _load_player(req->playerid(), &playerData);
    //无法读取数据，响应错误码：-2
    if (len < 0) {
        if (len == -1) {
            rsp.set_result(-2);
            rsp.set_reason(convertToUTF8("用户不存在..."));
        }
        else {
            rsp.set_result(-3);
            rsp.set_reason(convertToUTF8("服务器异常..."));
        }
        goto Exit0;
    }

    //验证密码是否正确
    if (playerData.password() != req->password()) {
        rsp.set_result(-4);
        rsp.set_reason(convertToUTF8("密码错误，请重试..."));
        goto Exit0;
    }

    //创建玩家对象
    _add_player(client, &playerData);
    g_statusMgr.add_status(client, &playerData);
    g_bagMgr.add_bag(client, &playerData);
    g_shopMgr.add_state(client, &playerData);
    g_saveMgr.add_saveData(client, &playerData);

    //登陆成功，应答0
    rsp.set_result(0);
    rsp.set_reason(convertToUTF8("登录成功..."));
    rsp.mutable_playerdata()->MergeFrom(playerData.syncdata());

Exit0:
    //应答结果
    fprintf(stdout, "--------------finish handle login: %d\n", rsp.result());
    len = encode(s_send_buff, SERVER_LOGIN_RSP, rsp.SerializeAsString().c_str(), rsp.ByteSize());
    sendData((uv_stream_t*)client, s_send_buff, len);

    //若登录成功，再发送商店列表
    if (rsp.result() == 0) {
        g_shopMgr.send_shopComplete_req(client);
    }

    return true;
}

// 玩家注册
bool PlayerMgr::player_create(uv_tcp_t* client, const PlayerCreateReq* req) {
    PlayerCreateRsp rsp;
    PlayerSaveData playerData;
    Player* player = nullptr;
    int len;
    bool result;

    //检查m_playerMap
    player = find_player(req->playerid());
    if (player != nullptr) {
        rsp.set_result(-1);
        rsp.set_reason(convertToUTF8("用户已登录..."));
        goto Exit0;
    }

    //根据id读取玩家数据
    len = _load_player(req->playerid(), &playerData);
    //读成功，说明用户已存在，响应错误码：-2
    if (len == 0) {
        rsp.set_result(-2);
        rsp.set_reason(convertToUTF8("用户名已存在..."));
        goto Exit0;
    }

    //创建玩家对象
    _init_saveData(req, &playerData);

    //存盘
    result = _save_player(&playerData);
    //存盘失败，响应错误码：-3
    if (!result) {
        rsp.set_result(-3);
        rsp.set_reason(convertToUTF8("保存数据时出错，请稍后尝试..."));
        goto Exit0;
    }

    //加入m_playerMap
    _add_player(client, &playerData);
    g_statusMgr.add_status(client, &playerData);
    g_bagMgr.add_bag(client, &playerData);
    g_shopMgr.add_state(client, &playerData);
    g_saveMgr.add_saveData(client, &playerData);

    //注册成功，应答0
    rsp.set_result(0);
    rsp.set_reason(convertToUTF8("注册成功"));
    rsp.mutable_playerdata()->MergeFrom(playerData.syncdata());

Exit0:
    fprintf(stdout, "--------------finish handle create: %d\n", rsp.result());
    //应答结果
    len = encode(s_send_buff, SERVER_CREATE_RSP, rsp.SerializeAsString().c_str(), rsp.ByteSize());
    sendData((uv_stream_t*)client, s_send_buff, len);

    //若注册成功，再发送商店列表
    if (rsp.result() == 0) {
        g_shopMgr.send_shopComplete_req(client);
    }

    return true;
}

//初始化玩家存档数据
bool PlayerMgr::_init_saveData(const PlayerCreateReq* createReq, PlayerSaveData* playerData) {
    playerData->set_password(createReq->password());
    PlayerSyncData* syncData = playerData->mutable_syncdata();
    syncData->set_playerid(createReq->playerid());
    syncData->set_name(createReq->name());
    syncData->set_positionx(-9.8);
    syncData->set_positiony(-3.2);
    syncData->set_rotation(0);
    syncData->set_iswalking(false);
    syncData->set_money(10000);
    syncData->clear_items();
    syncData->clear_buylimit();
    return true;
}

//添加一个玩家到m_playerMap
bool PlayerMgr::_add_player(uv_tcp_t* client, const PlayerSaveData* playerData) {
    Player* player = new Player();
    player->PlayerID = playerData->syncdata().playerid();
    player->Password = playerData->password();
    player->Name = playerData->syncdata().name();
    m_playerMap.insert(make_pair(client, player));
    return true;
}

// 从文件中加载玩家数据，成功返回true，失败返回false
int PlayerMgr::_load_player(string playerID, PlayerSaveData* playerData) {
    char buf[BUFSIZ];
    int len = load(playerID.c_str(), buf, BUFSIZ);
    if (len < 0) {
        fprintf(stdout, "load player error with: %d\n", len);
        goto Exit0;
    }
    playerData->ParseFromArray(buf, len);

Exit0:
    return len;
}

// 把玩家数据保存到文件中，成功返回true，失败返回false
bool PlayerMgr::_save_player(const PlayerSaveData* playerData) {
    bool ret = false;

    int len = save(playerData->syncdata().playerid().c_str(), playerData->SerializeAsString().c_str(), playerData->ByteSize());
    if (len < 0) {
        fprintf(stdout, "save player error with: %d\n", len);
        goto Exit0;
    }

    ret = true;
Exit0:
    return ret;
}


#pragma once
#include <stdint.h>
#include <string>
#include <map>
#include "uv.h"
#include "../proto/player.pb.h"


using namespace std;
using namespace TCCamp;

struct Player {
    string PlayerID;
    string Password;
    string Name;
};

class PlayerMgr {
public:
    PlayerMgr();
    ~PlayerMgr();

    bool init();
    bool un_init();

    // 处理用户请求
    bool player_login(uv_tcp_t* client, const PlayerLoginReq* req);
    bool player_create(uv_tcp_t* client, const PlayerCreateReq* req);
    bool announce_request(uv_tcp_t* client);

    Player* find_player(string playerID);
    Player* find_player(uv_tcp_t* client);

    // 有链接断开
    bool on_client_close(uv_tcp_t* client);
    
    // 广播公告
    bool broadcast_announce(string announce);
private:
    bool _init_saveData(const PlayerCreateReq* createReq, PlayerSaveData* playerData);
    bool _add_player(uv_tcp_t* client, const PlayerSaveData* playerData);

    int _load_player(string playerID, PlayerSaveData* playerData);
    bool _save_player(const PlayerSaveData* playerData);

    map<uv_tcp_t*, Player*> m_playerMap;
    string m_announce;
};

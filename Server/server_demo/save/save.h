#pragma once
#include <stdint.h>
#include <string>
#include <map>
#include "uv.h"
#include "../proto/player.pb.h"

using namespace std;
using namespace TCCamp;

class SaveMgr {
public:
    SaveMgr();
    ~SaveMgr();

    bool init();
    bool un_init();

    bool on_client_close(uv_tcp_t* client);
    PlayerSaveData* find_saveData(uv_tcp_t* client);
    bool add_saveData(uv_tcp_t* client, const PlayerSaveData* playerData);

    bool save_allPlayer();
    bool save_player(uv_tcp_t* client);

private:
    bool _update_player(uv_tcp_t* client);

	map<uv_tcp_t*, PlayerSaveData*> m_saveMap;
};
#pragma once
#include <stdint.h>
#include <string>
#include <map>
#include "uv.h"
#include "../proto/player.pb.h"


using namespace std;
using namespace TCCamp;

struct Status {
    string PlayerID;
    float PositionX;
    float PositionY;
    float Rotation;
    bool IsWalking;
};

class StatusMgr {
public:
    StatusMgr();
    ~StatusMgr();

    bool init();
    bool un_init();

    bool add_status(uv_tcp_t* client, const PlayerSaveData* playerData);
    Status* find_status(uv_tcp_t* client);

    bool on_status_req(uv_tcp_t* client, PlayerStatusReq* req);

    bool on_client_close(uv_tcp_t* client);

private:
    map<uv_tcp_t*, Status*> m_statusMap;
};

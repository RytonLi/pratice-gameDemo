#include "logic.h"
#include "../codec/codec.h"
#include "misc/misc.h"
#include <map>

using namespace std;

void player_login() {
    PlayerLoginReq req;
    req.set_playerid("zhongjunqi");
    req.set_password("123456");
    send_pb(CLIENT_LOGIN_REQ, &req);
    fprintf(stdout, "send player login request\n");
}

void player_login_response(const PlayerLoginRsp* rsp) {
    if (rsp->result() == 0) {
        fprintf(stdout, "player login success\n");
        return;
    }
        
    if (rsp->result() == -2) {
        player_create();
    }
    else {
        fprintf(stderr, "player login failed:%s\n", rsp->reason().c_str());
    }
}

void player_create() {
    PlayerCreateReq req;
    req.set_playerid("zhongjunqi");
    req.set_password("123456");
    req.set_name("����");
    send_pb(CLIENT_CREATE_REQ, &req);
    fprintf(stdout, "send player create request\n");
}

void player_create_response(const PlayerCreateRsp* rsp) {
    if (rsp->result() == 0) {
        fprintf(stdout, "create player success, name:%s\n", rsp->name().c_str());
    }        
    else {
        fprintf(stderr, "create player failed:%s\n", rsp->reason().c_str());
    }
}

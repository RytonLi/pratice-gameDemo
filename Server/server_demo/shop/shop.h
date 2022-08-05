#pragma once
#include <stdint.h>
#include <string>
#include <map>
#include "uv.h"
#include "../proto/player.pb.h"

using namespace std;
using namespace TCCamp;

struct ShopState {
    string PlayerID;
    bool IsSet;
};

struct BaseShopItem {
    int ID;
    string Name;
    string Introduce;
    int Price;
};

class ShopMgr {
public:
    ShopMgr();
    ~ShopMgr();

    bool init();
    bool un_init();

    bool add_state(uv_tcp_t* client, const PlayerSaveData* playerData);
    bool on_client_close(uv_tcp_t* client);

    bool send_shopComplete_req(uv_tcp_t* client); 
    bool on_shopComplete_rsp(uv_tcp_t* client, const ShopCompleteRsp* rsp);
    bool on_buy_req(uv_tcp_t* client, const PlayerBuyReq* req);

private:
    bool _load_items();
    bool _parseCfg(const char* cfg);
public:
    map<int, BaseShopItem*> m_shopMap;
    map<uv_tcp_t*, ShopState*> m_stateMap;
};

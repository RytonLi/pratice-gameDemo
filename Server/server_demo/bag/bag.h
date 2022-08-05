#pragma once
#include <stdint.h>
#include <string>
#include <map>
#include <queue>
#include "uv.h"
#include "../proto/player.pb.h"


using namespace std;
using namespace TCCamp;

struct Bag {
    string PlayerID;
    int Money;
    map<int, BagItem> ItemMap;
    queue<BagUpdate> updateQueue;
};

class BagMgr {
public:
    BagMgr();
    ~BagMgr();

    bool init();
    bool un_init();

    bool add_bag(uv_tcp_t* client, const PlayerSaveData* playerData);
    Bag* find_bag(uv_tcp_t* client);
    int get_money(uv_tcp_t* client);
    bool update_bag_byBuy(uv_tcp_t* client, int price, int itemID, int itemNum);
    bool on_client_close(uv_tcp_t* client);

    bool on_updateBag_req(uv_tcp_t* client, const PlayerBagUpdateReq* req);
    bool on_money_req(uv_tcp_t* client, const PlayerMoneyReq* req);

private:
    map<uv_tcp_t*, Bag*> m_bagMap;
};

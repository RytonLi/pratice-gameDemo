#pragma once
#include "shop.h"
#include <stdio.h>
#include <stdlib.h>
#include "../codec/codec.h"
#include "../cjson/cjson.h"
#include "../player/player.h"
#include "../utils/utils.h"
#include "../filedb/filedb.h"
#include "../bag/bag.h"

static char s_send_buff[1024 * 64];
extern PlayerMgr g_playerMgr;
extern BagMgr g_bagMgr;

ShopMgr::ShopMgr() {

}

ShopMgr::~ShopMgr() {

}

bool ShopMgr::init() {
    m_stateMap.clear();
    m_shopMap.clear();
    bool ret = _load_items();
    if (!ret) {
        fprintf(stdout, "error in loading items...\n");
    }
    return ret;
}

bool ShopMgr::un_init() {
    for (auto it = m_shopMap.begin(); it != m_shopMap.end(); ++it) {
        delete it->second;
    }
    m_shopMap.clear();

    for (auto it = m_stateMap.begin(); it != m_stateMap.end(); ++it) {
        delete it->second;
    }
    m_stateMap.clear();
    return true;
}

bool ShopMgr::on_client_close(uv_tcp_t* client) {
    auto it = m_stateMap.find(client);
    if (it != m_stateMap.end()) {
        free(it->second);
        m_stateMap.erase(it);
    }
    return true;
}

bool ShopMgr::add_state(uv_tcp_t* client, const PlayerSaveData* playerData) {
    ShopState* state = new ShopState();
    state->PlayerID = playerData->syncdata().playerid();
    state->IsSet = false;
    m_stateMap.insert(make_pair(client, state));
    return true;
}

bool ShopMgr::send_shopComplete_req(uv_tcp_t* client) {
    ShopCompleteReq req;
    for (auto it = m_shopMap.begin(); it != m_shopMap.end(); it++) {
        ShopItem* addItem = req.add_items();
        BaseShopItem* cur = it->second;
        addItem->set_id(cur->ID);
        addItem->set_name(cur->Name);
        addItem->set_introduce(cur->Introduce);
        addItem->set_price(cur->Price);
    }

    int len = encode(s_send_buff, SERVER_SHOPCOMPLETE_REQ, req.SerializeAsString().c_str(), req.ByteSize());
    return sendData((uv_stream_t*)client, s_send_buff, len);
}

bool ShopMgr::on_shopComplete_rsp(uv_tcp_t* client, const ShopCompleteRsp* rsp) {
    if (rsp->result() == 0 && m_stateMap[client]->PlayerID == rsp->playerid()) {
        m_stateMap[client]->IsSet = true;
        fprintf(stdout, "--------------shop complete rsp receive\n");
    }
    else {
        fprintf(stdout, "--------------shop complete rsp err\n");
    }
    return true;
}

bool ShopMgr::on_buy_req(uv_tcp_t* client, const PlayerBuyReq* req) {
    PlayerBuyRsp rsp;
    int money = g_bagMgr.get_money(client);
    int price;
    int len;
    
    //玩家数据校验
    if (m_stateMap.count(client) == 0 ||
        req->playerid() != m_stateMap[client]->PlayerID ||
        !m_stateMap[client]->IsSet ||
        money == -1) {
        rsp.set_result(-1);
        rsp.set_reason(convertToUTF8("玩家数据异常..."));
        goto Exit0;
    }
    
    //检查玩家金钱是否足够
    price = m_shopMap[req->itemid()]->Price;
    if (money < price * req->itemnum()) {
        rsp.set_result(-2);
        rsp.set_reason(convertToUTF8("玩家余额不足..."));
        goto Exit0;
    }

    //金钱足够，扣除金钱，获得对应物品
    g_bagMgr.update_bag_byBuy(client, price, req->itemid(), req->itemnum());

    rsp.set_result(0);
    rsp.set_reason(convertToUTF8("购买成功..."));

Exit0:
    fprintf(stdout, "--------------finish handle buy: %d\n", rsp.result());
    //应答结果
    len = encode(s_send_buff, SERVER_BUY_RSP, rsp.SerializeAsString().c_str(), rsp.ByteSize());
    sendData((uv_stream_t*)client, s_send_buff, len);
    return true;
}

bool ShopMgr::_parseCfg(const char* cfg) {
    bool ret = false;

    int itemNum;
    cJSON* monitor_json = nullptr;
    const cJSON* itemArray = nullptr;
    const cJSON* readItem = nullptr;
    const cJSON* id = nullptr;
    const cJSON* name = nullptr;
    const cJSON* introduce = nullptr;
    const cJSON* price = nullptr;
    BaseShopItem* item = nullptr;

    monitor_json = cJSON_Parse(cfg);
    if (monitor_json == nullptr) {
        const char* error_ptr = cJSON_GetErrorPtr();
        if (error_ptr != nullptr) {
            fprintf(stderr, "Error before: %s\n", error_ptr);
        }
        goto Exit0;
    }
    
    itemArray = cJSON_GetObjectItemCaseSensitive(monitor_json, "items");
    if (!cJSON_IsArray(itemArray)) {
        fprintf(stderr, "invalid config, gameserver field must object\n");
        goto Exit0;
    }
    itemNum = cJSON_GetArraySize(itemArray);
    //fprintf(stdout, "load item num: %d\n", itemNum);

    for (int i = 0; i < itemNum; i++) {
        readItem = cJSON_GetArrayItem(itemArray, i);
        item = new BaseShopItem();
        
        //获取物品Id
        id = cJSON_GetObjectItemCaseSensitive(readItem, "id");
        if (cJSON_IsNumber(id))
            item->ID = id->valueint;
        id = nullptr;

        //获取物品Name
        name = cJSON_GetObjectItemCaseSensitive(readItem, "name");
        if (cJSON_IsString(name))
            item->Name = name->valuestring;
        name = nullptr;

        //获取物品介绍
        introduce = cJSON_GetObjectItemCaseSensitive(readItem, "introduce");
        if (cJSON_IsString(introduce))
            item->Introduce = introduce->valuestring;
        introduce = nullptr;

        //获取物品价格
        price = cJSON_GetObjectItemCaseSensitive(readItem, "price");
        if (cJSON_IsNumber(price))
            item->Price = price->valueint;
        price = nullptr;

        m_shopMap.insert(make_pair(item->ID, item));
    }
    ret = true;
Exit0:
    cJSON_Delete(monitor_json);
    return ret;
}

//从itemConfig加载物品
bool ShopMgr::_load_items() {
    bool ret = false;
    const char* path = "config/itemConfig.json";
    char* temp = nullptr;
    int len = 0;
    len = readCfg(path, nullptr, 0);
    if (len < 0) {
        fprintf(stderr, "can't find %s\n", path);
        goto Exit0;
    }
    temp = (char*)malloc(len);
    readCfg(path, temp, len);

    ret = _parseCfg(temp);
    if (!ret) {
        goto Exit0;
    }

    free(temp);

    ret = true;
Exit0:
    return ret;
}
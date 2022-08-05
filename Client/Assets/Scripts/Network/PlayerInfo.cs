using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCCamp;

public class PlayerInfo
{
    private PlayerInfo()
    {
        m_HoldItems = new SortedDictionary<int, BagItem>();
    }
    private static PlayerInfo _Instance;

    public static PlayerInfo Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = new PlayerInfo();
            return _Instance;
        }
    }

    public string Id;
    public string Name;
    public float PositionX;
    public float PositionY;
    public float Rotation;
    public bool IsWalking;
    public int Money;
    public SortedDictionary<int, BagItem> m_HoldItems;

    public bool Init(PlayerSyncData syncData)
    {
        Id = syncData.PlayerID;
        Name = syncData.Name.ToStringUtf8();
        PositionX = syncData.PositionX;
        PositionY = syncData.PositionY;
        Rotation = syncData.Rotation;
        IsWalking = syncData.IsWalking;
        Money = syncData.Money;
        foreach(BagItem item in syncData.Items)
        {
            m_HoldItems.Add(item.Id, item);
        }

        return true;
    }
}

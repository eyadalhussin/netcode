using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct LobbyPlayerData : INetworkSerializable, System.IEquatable<LobbyPlayerData>
{

    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public bool IsReady;


    public LobbyPlayerData(ulong clientId, string name, bool isReady)
    {
        ClientId = clientId;
        PlayerName = new FixedString32Bytes(name);
        IsReady = isReady;
    }

    public bool Equals(LobbyPlayerData other)
    {
        return other.PlayerName.Equals(PlayerName);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref IsReady);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;

public class LobbyPlayer : NetworkBehaviour
{
    // Start is called before the first frame update
    private ulong ClientId;
    private string Name = "player";
    private bool Ready = false;
    public LobbyPlayer(ulong clientId, string name, bool ready)
    {
        ClientId = clientId;
        Name = name;
        Ready = ready;
    }

    public ulong GetClientId()
    {
        return ClientId;
    }
    public void SetClientId(ulong clientId)
    {
        ClientId = clientId;
    }

    public bool IsReady()
    {
        return Ready;
    }

    public string GetPlayerName()
    {
        return Name;
    }
    public void SetReady(bool ready)
    {
        Ready = ready;
    }

    public void SetPlayerName(string name)
    {
        Name = name;
    }

}

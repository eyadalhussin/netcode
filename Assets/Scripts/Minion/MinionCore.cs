using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEngine;
using Color = UnityEngine.Color;

public enum MinionType
{
    Minion,
    PlayerMinion
}

public class MinionCore : NetworkBehaviour
{
    //Type and ID
    private int _id = 999;
    private MinionType _minionType;

    private NetworkVariable<Color> _minionColor = new(Color.black);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _minionColor.OnValueChanged += OnMinionColorChanged;
    }

    public void SetMinionType(MinionType type)
    {
        _minionType = type;
    }

    public MinionType GetMinionType()
    {
        return _minionType;
    }

    public void SetID(int identification)
    {
        _id = identification;
    }

    public int GetID()
    {
        return _id;
    }

    public void ChangeColor()
    {
        if (!IsServer) return;
        if (_minionType == MinionType.PlayerMinion)
        {
            MinionPlayerAI mpai = GetComponent<MinionPlayerAI>();
            if (mpai)
            {
                // Directly get the color from some server-side logic.
                // This could be a stored color on the player object that the server is aware of.
                Color playerColor = mpai.GetControllingPlayerColor();
                _minionColor.Value = playerColor;
            }
        }
    }

    private void OnMinionColorChanged(Color oldColor, Color newColor)
    {
        // This will be called on clients when the SyncVar is updated from the server
        UpdateMinionColorClientRPC(newColor);
    }

    // UNUSED
    private void UpdateRendererColor(Color color)
    {
        if(!IsClient) return;
        Renderer rend = GetComponent<Renderer>();
        if (rend)
        {
            rend.material.color = color;
        }
        else
        {
            Debug.LogError("Renderer component not found on minion.");
        }
    }

    [ClientRpc]
    private void UpdateMinionColorClientRPC(Color color)
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend)
        {
            rend.material.color = color;
        }
    }
}

using GameServer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        if (_msg == "StartBattle" && Enemy.SetActive != 3)
        {
            Enemy.SetActive++;
            return;
        }
        Debug.Log($"Mesage from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived(); 
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System;
using GameServer;
using UnityEngine.Windows;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBuffersSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public int BotInMatch = 0;
    public TCP tcp;

    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exits,destroing objects!");
            Destroy(this);
        }
    }

    private void Start()
    {
        tcp = new TCP();
    }

    public void ConnectedToServer()
    {
        InitializeClientData();

        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private byte[] receiveBuffer;
        private Packet receiveData;

        public void Connect()
        {
            socket = new TcpClient
            {
              ReceiveBufferSize = dataBuffersSize,
              SendBufferSize = dataBuffersSize,
            };

            receiveBuffer = new byte[dataBuffersSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }
        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            receiveData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBuffersSize, ReceiveCallback, null);
        }
        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error Sending data to server TCP: {ex}");
            }
        }
        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    // отключение 
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receiveData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBuffersSize, ReceiveCallback, null);
                // handle data
            }
            catch
            {
                // отключеие 
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLenhgt = 0;

            receiveData.SetBytes(_data);

            if (receiveData.UnreadLength() >= 4)
            {
                _packetLenhgt |= receiveData.ReadInt();
                if (_packetLenhgt <= 0)
                {
                    return true;
                }
            }

            while (_packetLenhgt > 0 && _packetLenhgt <= receiveData.UnreadLength())
            {
                byte[] _packetBytes = receiveData.ReadBytes(_packetLenhgt);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });

                _packetLenhgt = 0;
                if (receiveData.UnreadLength() >= 4)
                {
                    _packetLenhgt |= receiveData.ReadInt();
                    if (_packetLenhgt <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLenhgt <= 1)
            {
                return true;
            }

            return false;
        }
    }
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome}
        };
        Debug.Log("initiliazed packets.");
    }
}


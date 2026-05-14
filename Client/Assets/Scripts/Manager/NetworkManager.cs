using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    private Socket _socket;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Connect(string host, int port)
    {
        // TODO : Phase 1 - TCP 연결 구현
    }

    public void Send(byte[] data)
    {
        // TODO : Phase 1 - 패킷 송신 구현
    }

    private void OnDestroy()
    {
        _socket?.Close();
    }
}

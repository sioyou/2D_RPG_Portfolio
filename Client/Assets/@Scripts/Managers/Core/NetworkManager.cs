using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;

public enum EServerType
{
    GameServer = 0,
}

public class ServerInstance
{
	ServerSession _session = null;
	Connector _connector = new Connector();

	public bool IsConnected()
	{
		if (_session == null)
			return false;

		return _session.IsConnected();
	}

	public void Send(IMessage packet)
	{
		if (_session != null)
			_session.Send(packet);
	}

	public void Connect(IPEndPoint endPoint, Action onSuccessCallback = null, Action onFailedCallback = null)
	{
		_session = new ServerSession();
		_connector.OnSuccessCallback = () => { PushAction(onSuccessCallback); _connector.OnSuccessCallback = null; };
		_connector.OnFailedCallback = () => { PushAction(onFailedCallback); _connector.OnFailedCallback = null; };
		_connector.Connect(endPoint, () => { return _session; });
	}

	public void Update()
	{
		ExecuteAction();

		if (_session == null)
			return;

		List<PacketMessage> list = PacketQueue.Instance.PopAll(_session);
		foreach (PacketMessage packet in list)
		{
			Action<PacketSession, IMessage> handler = ServerPacketHandler.Instance.GetPacketHandler(packet.Id);
			if (handler != null)
				handler.Invoke(_session, packet.Message);
		}
	}

	public void Disconnect()
	{
		if (_session != null)
			_session.Disconnect();

		_session = null;
	}

	#region ActionQueue
	object _lock = new object();
	Queue<Action> _actionQueue = new Queue<Action>();

	void PushAction(Action action)
	{
		lock (_lock)
		{
			_actionQueue.Enqueue(action);
		}
	}

	void ExecuteAction()
	{
		if (_actionQueue.Count == 0)
			return;

		lock (_lock)
		{
			while (_actionQueue.TryDequeue(out Action action))
			{
				action?.Invoke();
			}
		}
	}
	#endregion
}

public class NetworkManager
{
    public ServerInstance GameServer { get; } = new ServerInstance();

    public event Action OnGameServerConnected;
    public event Action OnGameServerConnectFailed;

    public void ConnectGameServer(string ip = "127.0.0.1", int port = 7777)
    {
        IPAddress ipAddr = IPAddress.Parse(ip);
        IPEndPoint endPoint = new IPEndPoint(ipAddr, port);
        GameServer.Connect(endPoint, OnConnectionSuccess, OnConnectionFailed);
    }

    private void OnConnectionSuccess()
    {
        Debug.Log("Connected To Server");
        OnGameServerConnected?.Invoke();
    }

    private void OnConnectionFailed()
    {
        Debug.Log("Failed To Connect To Server");
        OnGameServerConnectFailed?.Invoke();
    }

    public void Update()
    {
        GameServer.Update();
    }

    public void Send(IMessage packet, EServerType type = EServerType.GameServer)
    {
        if (type == EServerType.GameServer)
            GameServer.Send(packet);
	}
}
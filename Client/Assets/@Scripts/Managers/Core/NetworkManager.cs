using Google.Protobuf;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using Protocol;
using UnityEngine;

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

    public string PlayerId { get; set; } = "Player1";

    public event Action OnGameServerConnected;
    public event Action OnGameServerConnectFailed;
    public event Action OnLoginSuccess;
    public event Action OnLoginFailed;
    public event Action OnEnterGameSuccess;
    public event Action OnEnterGameFailed;

    public void ConnectGameServer(string ip = "127.0.0.1", int port = 7777, string playerId = "Test")
    {
        if (string.IsNullOrEmpty(playerId) == false)
            PlayerId = playerId;

        IPAddress ipAddr = IPAddress.Parse(ip);
        IPEndPoint endPoint = new IPEndPoint(ipAddr, port);
        GameServer.Connect(endPoint, OnConnectionSuccess, OnConnectionFailed);
    }

    public void SendLogin(string playerId = null)
    {
        if (GameServer.IsConnected() == false)
        {
            Debug.LogWarning("[NetworkManager] SendLogin failed: not connected to game server.");
            return;
        }

        string id = string.IsNullOrEmpty(playerId) ? PlayerId : playerId;
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("[NetworkManager] SendLogin failed: playerId is empty.");
            return;
        }

        PlayerId = id;

        C_S_LOGIN packet = new C_S_LOGIN
        {
            PlayerId = id,
        };

        Send(packet);
        Debug.Log($"[NetworkManager] C_S_LOGIN sent. playerId={id}");
    }

    public void HandleLoginResponse(bool success)
    {
        if (success)
        {
            Debug.Log("[NetworkManager] Login success.");
            OnLoginSuccess?.Invoke();
        }
        else
        {
            Debug.LogWarning("[NetworkManager] Login failed.");
            OnLoginFailed?.Invoke();
        }
    }

    public void SendEnterGame()
    {
        if (GameServer.IsConnected() == false)
        {
            Debug.LogWarning("[NetworkManager] SendEnterGame failed: not connected.");
            return;
        }

        Send(new C_S_ENTER_GAME());
        Debug.Log("[NetworkManager] C_S_ENTER_GAME sent.");
    }

    public void HandleEnterGameResponse(bool success, int myObjectId, int roomId, int mapId, Google.Protobuf.Collections.RepeatedField<SpawnEntry> spawns)
    {
        if (success)
        {
            if (Managers.Map.Init(mapId) == false)
            {
                Debug.LogError($"[NetworkManager] Enter game failed. mapId={mapId} could not be loaded.");
                OnEnterGameFailed?.Invoke();
                return;
            }

            Managers.Game.SetMyObjectId(myObjectId);
            Managers.Object.SpawnAll(spawns);
            Debug.Log($"[NetworkManager] Enter game success. myObjectId={myObjectId} roomId={roomId} mapId={mapId} spawnCount={spawns?.Count ?? 0}");
            OnEnterGameSuccess?.Invoke();
        }
        else
        {
            Debug.LogWarning("[NetworkManager] Enter game failed.");
            OnEnterGameFailed?.Invoke();
        }
    }

    public void SendAttack(float dirX, float dirY)
    {
        if (GameServer.IsConnected() == false)
            return;

        Send(new C_S_ATTACK
        {
            DirX = dirX,
            DirY = dirY,
        });
    }

    public void SendMoveSync(float posX, float posY, float dirX, float dirY, int stateFlags)
    {
        if (GameServer.IsConnected() == false)
            return;

        Send(new C_S_MOVE
        {
            PosX = posX,
            PosY = posY,
            DirX = dirX,
            DirY = dirY,
            StateFlags = stateFlags,
        });
    }

    public void SendLeaveGame()
    {
        if (GameServer.IsConnected() == false)
        {
            Debug.LogWarning("[NetworkManager] SendLeaveGame failed: not connected.");
            return;
        }

        Send(new C_S_LEAVE_GAME());
        Debug.Log("[NetworkManager] C_S_LEAVE_GAME sent.");
    }

    public void HandleLeaveGameResponse(bool success)
    {
        if (success == false)
        {
            Debug.LogWarning("[NetworkManager] Leave game failed.");
            return;
        }

        Debug.Log("[NetworkManager] Leave game success.");
        Managers.Object.Clear();
    }

    public void LeaveGameAndDisconnect()
    {
        if (Managers.Initialized == false)
            return;

        if (GameServer.IsConnected())
        {
            SendLeaveGame();
            GameServer.Disconnect();
        }

        Managers.Object.Clear();
    }

    private void OnConnectionSuccess()
    {
        Debug.Log("[NetworkManager] Connected to game server.");
        OnGameServerConnected?.Invoke();
    }

    private void OnConnectionFailed()
    {
        Debug.LogWarning("[NetworkManager] Failed to connect to game server.");
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

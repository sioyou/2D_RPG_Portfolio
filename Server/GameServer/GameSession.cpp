#include "pch.h"
#include "GameSession.h"
#include "GameSessionManager.h"
#include "PlayerManager.h"
#include "ClientPacketHandler.h"

GameSession::GameSession()
	: _packetQueue(MakeShared<JobQueue>())
{
}

GameSession::~GameSession()
{
	cout << "~GameSession" << endl;
}

GameSessionRef GameSession::GetGameSessionRef()
{
	return static_pointer_cast<GameSession>(GetPacketSessionRef());
}

void GameSession::OnConnected()
{
	GSessionManager.Add(GetGameSessionRef());
}

void GameSession::OnDisconnected()
{
	GameSessionRef session = GetGameSessionRef();
	GPlayerManager.Logout(session);
	GSessionManager.Remove(session);
}

void GameSession::OnRecvPacket(BYTE* buffer, int32 len)
{
	if (buffer == nullptr || len <= 0)
		return;

	Vector<BYTE> packet(buffer, buffer + len);
	GameSessionRef gameSession = GetGameSessionRef();

	_packetQueue->DoAsync([gameSession, packet = std::move(packet)]() mutable
		{
			if (packet.empty())
				return;

			PacketSessionRef session = gameSession;
			ClientPacketHandler::HandlePacket(session, packet.data(), static_cast<int32>(packet.size()));
		});
}

void GameSession::OnSend(int32 len)
{
}

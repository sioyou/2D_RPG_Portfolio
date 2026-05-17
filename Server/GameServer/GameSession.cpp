#include "pch.h"
#include "GameSession.h"
#include "GameSessionManager.h"
#include "PlayerManager.h"
#include "ClientPacketHandler.h"

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
	PacketSessionRef session = GetPacketSessionRef();
	ClientPacketHandler::HandlePacket(session, buffer, len);
}

void GameSession::OnSend(int32 len)
{
}

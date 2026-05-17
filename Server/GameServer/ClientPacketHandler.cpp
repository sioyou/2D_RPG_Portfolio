#include "pch.h"
#include "ClientPacketHandler.h"
#include "GameSession.h"
#include "PlayerManager.h"
#include "ZoneManager.h"

PacketHandlerFunc GPacketHandler[UINT16_MAX];

bool Handle_INVALID(PacketSessionRef& session, BYTE* buffer, int32 len)
{
	PacketHeader* header = reinterpret_cast<PacketHeader*>(buffer);
	// TODO : log
	return false;
}


bool Handle_C_S_LOGIN(PacketSessionRef& session, Protocol::C_S_LOGIN& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);

	Protocol::S_C_LOGIN sendPkt;
	PlayerRef player;
	sendPkt.set_success(GPlayerManager.Login(gameSession, pkt.playerid(), player));

	SendBufferRef sendBuffer = ClientPacketHandler::MakeSendBuffer(sendPkt);
	session->Send(sendBuffer);
	return true;
}

bool Handle_C_S_ENTER_GAME(PacketSessionRef& session, Protocol::C_S_ENTER_GAME& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
	PlayerRef player = GPlayerManager.FindBySession(gameSession);

	Protocol::S_C_ENTER_GAME enterPkt;
	if (GZoneManager.EnterGame(player, enterPkt) == false)
	{
		enterPkt.set_success(false);
		enterPkt.set_myobjectid(0);
	}

	SendBufferRef enterBuffer = ClientPacketHandler::MakeSendBuffer(enterPkt);
	session->Send(enterBuffer);
	return true;
}

bool Handle_C_S_LEAVE_GAME(PacketSessionRef& session, Protocol::C_S_LEAVE_GAME& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
	PlayerRef player = GPlayerManager.FindBySession(gameSession);

	Protocol::S_C_LEAVE_GAME leavePkt;

	if (player == nullptr)
	{
		leavePkt.set_success(false);
	}
	else
	{
		GZoneManager.LeaveGame(player);
		leavePkt.set_success(true);
	}

	SendBufferRef leaveBuffer = ClientPacketHandler::MakeSendBuffer(leavePkt);
	session->Send(leaveBuffer);
	return true;
}

bool Handle_C_S_MOVE(PacketSessionRef& session, Protocol::C_S_MOVE& pkt)
{

	return true;
}

bool Handle_C_S_ATTACK(PacketSessionRef& session, Protocol::C_S_ATTACK& pkt)
{

	return true;
}

bool Handle_C_S_CHAT(PacketSessionRef& session, Protocol::C_S_CHAT& pkt)
{

	return true;
}

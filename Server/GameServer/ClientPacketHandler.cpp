#include "pch.h"
#include "ClientPacketHandler.h"

PacketHandlerFunc GPacketHandler[UINT16_MAX];

bool Handle_INVALID(PacketSessionRef& session, BYTE* buffer, int32 len)
{
	PacketHeader* header = reinterpret_cast<PacketHeader*>(buffer);
	// TODO : log
	return false;
}


bool Handle_C_S_LOGIN(PacketSessionRef& session, Protocol::C_S_LOGIN& pkt)
{

	return true;
}

bool Handle_C_S_ENTER_GAME(PacketSessionRef& session, Protocol::C_S_ENTER_GAME& pkt)
{

	return true;
}

bool Handle_C_S_LEAVE_GAME(PacketSessionRef& session, Protocol::C_S_LEAVE_GAME& pkt)
{

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

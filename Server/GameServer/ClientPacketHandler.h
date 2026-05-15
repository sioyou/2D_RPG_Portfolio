#pragma once
#include "Protocol.pb.h"

using PacketHandlerFunc = std::function<bool(PacketSessionRef&, BYTE*, int32)>;
extern PacketHandlerFunc GPacketHandler[UINT16_MAX];

enum : uint16
{
	C_S_LOGIN = 1010,
	S_C_LOGIN = 1011,
	C_S_ENTER_GAME = 1012,
	S_C_ENTER_GAME = 1013,
	C_S_LEAVE_GAME = 1014,
	S_C_LEAVE_GAME = 1015,
	S_C_SPAWN = 1016,
	C_S_MOVE = 1017,
	S_C_MOVE = 1018,
	C_S_ATTACK = 1019,
	S_C_ATTACK = 1020,
	S_C_DIE = 1021,
	C_S_CHAT = 1022,
	S_C_CHAT = 1023,
};

// Custom Handlers
bool Handle_INVALID(PacketSessionRef& session, BYTE* buffer, int32 len);
bool Handle_C_S_LOGIN(PacketSessionRef& session, Protocol::C_S_LOGIN& pkt);
bool Handle_C_S_ENTER_GAME(PacketSessionRef& session, Protocol::C_S_ENTER_GAME& pkt);
bool Handle_C_S_LEAVE_GAME(PacketSessionRef& session, Protocol::C_S_LEAVE_GAME& pkt);
bool Handle_C_S_MOVE(PacketSessionRef& session, Protocol::C_S_MOVE& pkt);
bool Handle_C_S_ATTACK(PacketSessionRef& session, Protocol::C_S_ATTACK& pkt);
bool Handle_C_S_CHAT(PacketSessionRef& session, Protocol::C_S_CHAT& pkt);

class ClientPacketHandler
{
public:
	static void Init()
	{
		for (int32 i = 0; i < UINT16_MAX; i++)
			GPacketHandler[i] = Handle_INVALID;
		GPacketHandler[C_S_LOGIN] = [](PacketSessionRef& session, BYTE* buffer, int32 len) { return HandlePacket<Protocol::C_S_LOGIN>(Handle_C_S_LOGIN, session, buffer, len); };
		GPacketHandler[C_S_ENTER_GAME] = [](PacketSessionRef& session, BYTE* buffer, int32 len) { return HandlePacket<Protocol::C_S_ENTER_GAME>(Handle_C_S_ENTER_GAME, session, buffer, len); };
		GPacketHandler[C_S_LEAVE_GAME] = [](PacketSessionRef& session, BYTE* buffer, int32 len) { return HandlePacket<Protocol::C_S_LEAVE_GAME>(Handle_C_S_LEAVE_GAME, session, buffer, len); };
		GPacketHandler[C_S_MOVE] = [](PacketSessionRef& session, BYTE* buffer, int32 len) { return HandlePacket<Protocol::C_S_MOVE>(Handle_C_S_MOVE, session, buffer, len); };
		GPacketHandler[C_S_ATTACK] = [](PacketSessionRef& session, BYTE* buffer, int32 len) { return HandlePacket<Protocol::C_S_ATTACK>(Handle_C_S_ATTACK, session, buffer, len); };
		GPacketHandler[C_S_CHAT] = [](PacketSessionRef& session, BYTE* buffer, int32 len) { return HandlePacket<Protocol::C_S_CHAT>(Handle_C_S_CHAT, session, buffer, len); };
	}

	static bool HandlePacket(PacketSessionRef& session, BYTE* buffer, int32 len)
	{
		PacketHeader* header = reinterpret_cast<PacketHeader*>(buffer);
		return GPacketHandler[header->id](session, buffer, len);
	}
	static SendBufferRef MakeSendBuffer(Protocol::S_C_LOGIN& pkt) { return MakeSendBuffer(pkt, S_C_LOGIN); }
	static SendBufferRef MakeSendBuffer(Protocol::S_C_ENTER_GAME& pkt) { return MakeSendBuffer(pkt, S_C_ENTER_GAME); }
	static SendBufferRef MakeSendBuffer(Protocol::S_C_LEAVE_GAME& pkt) { return MakeSendBuffer(pkt, S_C_LEAVE_GAME); }
	static SendBufferRef MakeSendBuffer(Protocol::S_C_SPAWN& pkt) { return MakeSendBuffer(pkt, S_C_SPAWN); }
	static SendBufferRef MakeSendBuffer(Protocol::S_C_MOVE& pkt) { return MakeSendBuffer(pkt, S_C_MOVE); }
	static SendBufferRef MakeSendBuffer(Protocol::S_C_ATTACK& pkt) { return MakeSendBuffer(pkt, S_C_ATTACK); }
	static SendBufferRef MakeSendBuffer(Protocol::S_C_DIE& pkt) { return MakeSendBuffer(pkt, S_C_DIE); }
	static SendBufferRef MakeSendBuffer(Protocol::S_C_CHAT& pkt) { return MakeSendBuffer(pkt, S_C_CHAT); }

private:
	template<typename PacketType, typename ProcessFunc>
	static bool HandlePacket(ProcessFunc func, PacketSessionRef& session, BYTE* buffer, int32 len)
	{
		PacketType pkt;
		if (pkt.ParseFromArray(buffer + sizeof(PacketHeader), len - sizeof(PacketHeader)) == false)
			return false;

		return func(session, pkt);
	}

	template<typename T>
	static SendBufferRef MakeSendBuffer(T& pkt, uint16 pktId)
	{
		const uint16 dataSize = static_cast<uint16>(pkt.ByteSizeLong());
		const uint16 packetSize = dataSize + sizeof(PacketHeader);

		SendBufferRef sendBuffer = GSendBufferManager->Open(packetSize);
		PacketHeader* header = reinterpret_cast<PacketHeader*>(sendBuffer->Buffer());
		header->size = packetSize;
		header->id = pktId;
		ASSERT_CRASH(pkt.SerializeToArray(&header[1], dataSize));
		sendBuffer->Close(packetSize);

		return sendBuffer;
	}
};
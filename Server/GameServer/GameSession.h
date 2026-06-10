#pragma once
#include "Session.h"
#include "JobQueue.h"

class GameSession : public PacketSession
{
public:
	GameSession();
	~GameSession() override;

	GameSessionRef GetGameSessionRef();

	void OnConnected() override;
	void OnDisconnected() override;
	void OnRecvPacket(BYTE* buffer, int32 len) override;
	void OnSend(int32 len) override;

private:
	JobQueueRef _packetQueue;
};

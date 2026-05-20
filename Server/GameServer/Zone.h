#pragma once
#include "Player.h"
#include "Protocol.pb.h"

class Zone
{
public:
	explicit Zone(int32 zoneId);

	int32 GetZoneId() const { return _zoneId; }

	bool HasPlayer(PlayerRef player);
	bool EnterPlayer(PlayerRef player);
	void LeavePlayer(PlayerRef player);

	void FillEnterGameSpawns(Protocol::S_C_ENTER_GAME& pkt);
	void Broadcast(SendBufferRef sendBuffer);

	void ValidateClientPosition(PlayerRef player, float clientX, float clientY, float& outX, float& outY);

	PlayerRef FindPlayer(int32 objectId);

private:
	int32 _zoneId = 0;

	USE_LOCK;
	HashMap<int32, PlayerRef> _players;
	Set<GameSessionRef> _sessions;
};

using ZoneRef = shared_ptr<Zone>;

#pragma once
#include "Creature.h"
#include "Player.h"

/* Zone: spatial cell inside a Room (AOI unit). */
class Zone
{
public:
	explicit Zone(int32 zoneIndex);

	int32 GetZoneIndex() const { return _zoneIndex; }

	bool HasPlayer(int32 objectId);
	void AddPlayer(PlayerRef player);
	void RemovePlayer(int32 objectId);

	void AddCreature(CreatureRef creature);
	void RemoveCreature(int32 objectId);

	template<typename Func>
	void ForEachCreature(Func&& func)
	{
		READ_LOCK;
		for (const auto& pair : _creatures)
			func(pair.second);
	}

	template<typename Func>
	void ForEachPlayer(Func&& func)
	{
		READ_LOCK;
		for (const auto& pair : _players)
			func(pair.second);
	}

	void CollectSessions(Vector<GameSessionRef>& out);

private:
	int32 _zoneIndex = 0;

	USE_LOCK;
	HashMap<int32, PlayerRef> _players;
	HashMap<int32, CreatureRef> _creatures;
};

using ZoneRef = shared_ptr<Zone>;

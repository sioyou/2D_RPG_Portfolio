#include "pch.h"
#include "Zone.h"
#include "GameSession.h"

Zone::Zone(int32 zoneIndex)
	: _zoneIndex(zoneIndex)
{
}

bool Zone::HasPlayer(int32 objectId)
{
	READ_LOCK;
	return _players.find(objectId) != _players.end();
}

void Zone::AddPlayer(PlayerRef player)
{
	if (player == nullptr)
		return;

	WRITE_LOCK;
	_players[player->GetObjectId()] = player;
	_creatures[player->GetObjectId()] = player;
}

void Zone::RemovePlayer(int32 objectId)
{
	WRITE_LOCK;
	_players.erase(objectId);
	_creatures.erase(objectId);
}

void Zone::AddCreature(CreatureRef creature)
{
	if (creature == nullptr)
		return;

	WRITE_LOCK;
	_creatures[creature->GetObjectId()] = creature;

	if (creature->GetObjectType() == Protocol::OBJECT_TYPE_PLAYER)
	{
		PlayerRef player = static_pointer_cast<Player>(creature);
		_players[player->GetObjectId()] = player;
	}
}

void Zone::RemoveCreature(int32 objectId)
{
	WRITE_LOCK;
	_creatures.erase(objectId);
	_players.erase(objectId);
}

void Zone::CollectSessions(Vector<GameSessionRef>& out)
{
	READ_LOCK;
	for (const auto& pair : _players)
	{
		GameSessionRef session = pair.second->GetSession();
		if (session != nullptr)
			out.push_back(session);
	}
}

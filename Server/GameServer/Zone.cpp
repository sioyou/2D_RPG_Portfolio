#include "pch.h"
#include "Zone.h"
#include "GameSession.h"
#include "GameProtocolUtil.h"
#include "MonsterManager.h"

Zone::Zone(int32 zoneId)
	: _zoneId(zoneId)
{
}

bool Zone::HasPlayer(PlayerRef player)
{
	if (player == nullptr)
		return false;

	READ_LOCK;
	return _players.find(player->GetObjectId()) != _players.end();
}

bool Zone::EnterPlayer(PlayerRef player)
{
	if (player == nullptr)
		return false;

	GameSessionRef session = player->GetSession();
	if (session == nullptr)
		return false;

	WRITE_LOCK;

	auto it = _players.find(player->GetObjectId());
	if (it != _players.end())
		return it->second == player;

	player->SetZoneId(_zoneId);
	_players[player->GetObjectId()] = player;
	_sessions.insert(session);
	return true;
}

void Zone::LeavePlayer(PlayerRef player)
{
	if (player == nullptr)
		return;

	GameSessionRef session = player->GetSession();

	WRITE_LOCK;
	_players.erase(player->GetObjectId());

	if (session != nullptr)
		_sessions.erase(session);

	player->SetZoneId(0);
}

void Zone::FillEnterGameSpawns(Protocol::S_C_ENTER_GAME& pkt)
{
	{
		READ_LOCK;
		for (const auto& pair : _players)
		{
			Protocol::ObjectInfo* info = pkt.add_spawns();
			GameProtocolUtil::FillObjectInfo(pair.second, info);
		}
	}

	GMonsterManager.ForEachInZone(_zoneId, [&pkt](MonsterRef monster)
		{
			Protocol::ObjectInfo* info = pkt.add_spawns();
			GameProtocolUtil::FillObjectInfo(monster, info);
		});
}

void Zone::Broadcast(SendBufferRef sendBuffer)
{
	if (sendBuffer == nullptr)
		return;

	Vector<GameSessionRef> targets;
	{
		READ_LOCK;
		targets.reserve(_sessions.size());
		for (GameSessionRef session : _sessions)
			targets.push_back(session);
	}

	for (GameSessionRef session : targets)
		session->Send(sendBuffer);
}

PlayerRef Zone::FindPlayer(int32 objectId)
{
	READ_LOCK;
	auto it = _players.find(objectId);
	if (it == _players.end())
		return nullptr;
	return it->second;
}

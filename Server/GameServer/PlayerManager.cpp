#include "pch.h"
#include "PlayerManager.h"
#include "GameSession.h"
#include "ObjectIdGenerator.h"
#include "ZoneManager.h"
#include "CreatureManager.h"

PlayerManager GPlayerManager;

bool PlayerManager::Login(GameSessionRef session, const string& playerId, PlayerRef& outPlayer)
{
	outPlayer = nullptr;

	if (session == nullptr || playerId.empty())
		return false;

	{
		READ_LOCK;
		if (_playersById.find(playerId) != _playersById.end())
			return false;
	}

	PlayerRef player = make_shared<Player>();
	player->Init(playerId, ObjectIdGenerator::Generate());
	player->SetLoginTick(::GetTickCount64());
	player->SetState(EPlayerState::Lobby);
	player->SetSession(session);

	if (Register(player) == false)
		return false;

	GCreatureManager.Add(player);

	outPlayer = player;
	cout << "[PlayerManager] Login. playerId=" << playerId
		<< " objectId=" << player->GetObjectId() << endl;
	return true;
}

void PlayerManager::Logout(GameSessionRef session)
{
	if (session == nullptr)
		return;

	PlayerRef player;
	{
		READ_LOCK;
		auto it = _playersBySession.find(session.get());
		if (it == _playersBySession.end())
			return;
		player = it->second;
	}

	if (player == nullptr)
		return;

	cout << "[PlayerManager] Logout. playerId=" << player->GetPlayerId() << endl;
	GZoneManager.LeaveGame(player);
	GCreatureManager.Remove(player->GetObjectId());
	Unregister(player);
}

void PlayerManager::LogoutByPlayerId(const string& playerId)
{
	PlayerRef player = FindByPlayerId(playerId);
	if (player == nullptr)
		return;

	GameSessionRef session = player->GetSession();
	if (session)
		session->Disconnect(L"LogoutByPlayerId");
}

PlayerRef PlayerManager::FindByPlayerId(const string& playerId)
{
	READ_LOCK;
	auto it = _playersById.find(playerId);
	if (it == _playersById.end())
		return nullptr;
	return it->second;
}

PlayerRef PlayerManager::FindByObjectId(int32 objectId)
{
	READ_LOCK;
	auto it = _playersByObjectId.find(objectId);
	if (it == _playersByObjectId.end())
		return nullptr;
	return it->second;
}

PlayerRef PlayerManager::FindBySession(GameSessionRef session)
{
	if (session == nullptr)
		return nullptr;

	READ_LOCK;
	auto it = _playersBySession.find(session.get());
	if (it == _playersBySession.end())
		return nullptr;
	return it->second;
}

void PlayerManager::UpdateSession(PlayerRef player, GameSessionRef newSession)
{
	if (player == nullptr || newSession == nullptr)
		return;

	WRITE_LOCK;

	GameSessionRef oldSession = player->GetSession();
	if (oldSession == newSession)
		return;

	if (oldSession != nullptr)
		_playersBySession.erase(oldSession.get());

	player->SetSession(newSession);
	_playersBySession[newSession.get()] = player;
}

bool PlayerManager::IsOnline(const string& playerId)
{
	return FindByPlayerId(playerId) != nullptr;
}

int32 PlayerManager::GetOnlineCount()
{
	READ_LOCK;
	return static_cast<int32>(_playersById.size());
}

bool PlayerManager::Register(PlayerRef player)
{
	if (player == nullptr)
		return false;

	GameSessionRef session = player->GetSession();
	if (session == nullptr)
		return false;

	WRITE_LOCK;

	if (_playersById.find(player->GetPlayerId()) != _playersById.end())
		return false;

	_playersById[player->GetPlayerId()] = player;
	_playersByObjectId[player->GetObjectId()] = player;
	_playersBySession[session.get()] = player;

	return true;
}

void PlayerManager::Unregister(PlayerRef player)
{
	if (player == nullptr)
		return;

	WRITE_LOCK;

	_playersById.erase(player->GetPlayerId());
	_playersByObjectId.erase(player->GetObjectId());

	GameSessionRef session = player->GetSession();
	if (session)
		_playersBySession.erase(session.get());

	player->SetSession(nullptr);
	player->SetState(EPlayerState::Offline);
}

#include "pch.h"
#include "Player.h"
#include "GameSession.h"
#include "Data/DataManager.h"

Player::~Player()
{
	cout << "Player 소멸자" << endl;
}

void Player::ApplyData(const PlayerData& data)
{
	_playerDataId = data.id;

	CreatureStat& stat = GetStat();
	stat.SetLevel(data.level);
	stat.SetMaxHp(data.maxHp);
	stat.SetHp(data.maxHp);

	_attackDamage = data.attackDamage;
	_attackRange = data.attackRange;
	_attackCooldownMs = data.attackCooldownMs;
	_moveSpeed = data.moveSpeed;
}

void Player::Init(const string& playerId, int32 objectId, int32 playerDataId)
{
	_playerId = playerId;
	SetObjectId(objectId);

	const PlayerData* data = GDataManager.GetPlayer(playerDataId);
	if (data != nullptr)
	{
		ApplyData(*data);
		return;
	}

	PlayerData fallback;
	fallback.id = playerDataId;
	ApplyData(fallback);
}

void Player::SetSession(GameSessionRef session)
{
	_session = session;
}

GameSessionRef Player::GetSession()
{
	return _session.lock();
}

bool Player::IsOnline() const
{
	return _session.lock() != nullptr;
}

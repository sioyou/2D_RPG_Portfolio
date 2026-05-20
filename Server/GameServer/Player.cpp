#include "pch.h"
#include "Player.h"
#include "GameSession.h"

Player::~Player()
{
	cout << "Player 소멸자" << endl;
}

void Player::Init(const string& playerId, int32 objectId)
{
	_playerId = playerId;
	_objectId = objectId;
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

void Player::SetMoveDirection(float dirX, float dirY)
{
	const float lengthSq = dirX * dirX + dirY * dirY;
	if (lengthSq < 0.0001f)
	{
		_moveDirX = 0.f;
		_moveDirY = 0.f;
		return;
	}

	const float invLength = 1.f / sqrtf(lengthSq);
	_moveDirX = dirX * invLength;
	_moveDirY = dirY * invLength;
}

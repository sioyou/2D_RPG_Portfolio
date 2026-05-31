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
	SetObjectId(objectId);
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

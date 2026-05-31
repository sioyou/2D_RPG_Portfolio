#pragma once
#include "Creature.h"

enum class EPlayerState : uint8
{
	Offline = 0,
	Lobby = 1,
	InGame = 2,
};

class Player : public Creature
{
public:
	~Player();

	Protocol::GameObjectType GetObjectType() const override { return Protocol::OBJECT_TYPE_PLAYER; }
	const string& GetDisplayName() const override { return _playerId; }

	const string& GetPlayerId() const { return _playerId; }
	EPlayerState GetState() const { return _state; }

	void Init(const string& playerId, int32 objectId);
	void SetZoneId(int32 zoneId) { Creature::SetZoneId(zoneId); }
	void SetState(EPlayerState state) { _state = state; }

	void SetSession(GameSessionRef session);
	GameSessionRef GetSession();
	bool IsOnline() const;

	uint64 GetLoginTick() const { return _loginTick; }
	void SetLoginTick(uint64 tick) { _loginTick = tick; }

	uint64 GetLastMoveTick() const { return _lastMoveTick; }
	void SetLastMoveTick(uint64 tick) { _lastMoveTick = tick; }

	uint64 GetLastAttackTick() const { return _lastAttackTick; }
	void SetLastAttackTick(uint64 tick) { _lastAttackTick = tick; }

private:
	string _playerId;
	EPlayerState _state = EPlayerState::Lobby;

	uint64 _lastMoveTick = 0;
	uint64 _lastAttackTick = 0;
	uint64 _loginTick = 0;
	weak_ptr<class GameSession> _session;
};

using PlayerRef = shared_ptr<Player>;

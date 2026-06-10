#pragma once
#include "Creature.h"
#include "Data/DataTypes.h"

enum class EPlayerState : uint8
{
	Offline = 0,
	Lobby = 1,
	InGame = 2,
};

class Player : public Creature
{
public:
	~Player() override;

	Protocol::GameObjectType GetObjectType() const override { return Protocol::OBJECT_TYPE_PLAYER; }
	const string& GetDisplayName() const override { return _playerId; }

	const string& GetPlayerId() const { return _playerId; }
	int32 GetPlayerDataId() const { return _playerDataId; }
	EPlayerState GetState() const { return _state; }

	void Init(const string& playerId, int32 objectId, int32 playerDataId = DEFAULT_PLAYER_DATA_ID);
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

	int32 GetAttackDamage() const { return _attackDamage; }
	float GetAttackRange() const { return _attackRange; }
	uint64 GetAttackCooldownMs() const { return _attackCooldownMs; }
	float GetMoveSpeed() const { return _moveSpeed; }

private:
	void ApplyData(const PlayerData& data);

	string _playerId;
	int32 _playerDataId = DEFAULT_PLAYER_DATA_ID;
	EPlayerState _state = EPlayerState::Lobby;

	int32 _attackDamage = 10;
	float _attackRange = 1.5f;
	uint64 _attackCooldownMs = 500;
	float _moveSpeed = 5.f;

	uint64 _lastMoveTick = 0;
	uint64 _lastAttackTick = 0;
	uint64 _loginTick = 0;
	weak_ptr<class GameSession> _session;
};

using PlayerRef = shared_ptr<Player>;

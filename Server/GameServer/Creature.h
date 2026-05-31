#pragma once
#include "CreatureStat.h"
#include "Enum.pb.h"

class Creature : public enable_shared_from_this<Creature>
{
public:
	virtual ~Creature() = default;

	virtual Protocol::GameObjectType GetObjectType() const = 0;
	virtual const string& GetDisplayName() const = 0;

	int32 GetObjectId() const { return _objectId; }
	int32 GetZoneId() const { return _zoneId; }

	const CreatureStat& GetStat() const { return _stat; }
	CreatureStat& GetStat() { return _stat; }

	bool IsAlive() const { return _hasDied == false && _stat.GetHp() > 0; }
	bool IsDead() const { return _hasDied; }

	int32 GetStateFlags() const { return _stateFlags; }
	void SetStateFlags(int32 stateFlags) { _stateFlags = stateFlags; }

	int32 TakeDamage(int32 damage);
	void Heal(int32 amount);

	void SetMoveDirection(float dirX, float dirY);
	float GetMoveDirX() const { return _moveDirX; }
	float GetMoveDirY() const { return _moveDirY; }

	void FaceToward(float worldX, float worldY);

protected:
	void SetObjectId(int32 objectId) { _objectId = objectId; }
	void SetZoneId(int32 zoneId) { _zoneId = zoneId; }

	void Die();
	virtual void OnDied();

	int32 _objectId = 0;
	int32 _zoneId = 0;

	CreatureStat _stat;
	int32 _stateFlags = Protocol::CREATURE_STATE_NONE;
	bool _hasDied = false;

	float _moveDirX = 1.f;
	float _moveDirY = 0.f;
};

using CreatureRef = shared_ptr<Creature>;

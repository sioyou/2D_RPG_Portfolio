#include "pch.h"
#include "Creature.h"
#include "CreatureStateUtil.h"

void Creature::SetMoveDirection(float dirX, float dirY)
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

void Creature::FaceToward(float worldX, float worldY)
{
	const float dx = worldX - _stat.GetPosX();
	const float dy = worldY - _stat.GetPosY();
	SetMoveDirection(dx, dy);
}

int32 Creature::TakeDamage(int32 damage)
{
	if (IsAlive() == false || damage <= 0)
		return 0;

	const int32 hpBefore = _stat.GetHp();
	const bool died = _stat.TakeDamage(damage);
	const int32 damageDealt = hpBefore - _stat.GetHp();

	if (died)
		Die();

	return damageDealt;
}

void Creature::Heal(int32 amount)
{
	if (IsAlive() == false)
		return;

	_stat.Heal(amount);
}

void Creature::Die()
{
	if (_hasDied)
		return;

	_hasDied = true;
	_stateFlags = CreatureStateUtil::ToBitMask(Protocol::CREATURE_STATE_DEAD);
	OnDied();
}

void Creature::OnDied()
{
}

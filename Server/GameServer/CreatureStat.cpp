#include "pch.h"
#include "CreatureStat.h"

void CreatureStat::SetLevel(int32 level)
{
	if (level < 1)
		level = 1;

	_level = level;
}

void CreatureStat::SetMaxHp(int32 maxHp)
{
	if (maxHp < 1)
		maxHp = 1;

	_maxHp = maxHp;
	if (_hp > _maxHp)
		_hp = _maxHp;
}

bool CreatureStat::SetHp(int32 hp)
{
	if (hp < 0)
		return false;

	if (hp > _maxHp)
		hp = _maxHp;

	_hp = hp;
	return true;
}

void CreatureStat::Heal(int32 amount)
{
	if (amount <= 0)
		return;

	const int64 nextHp = static_cast<int64>(_hp) + amount;
	_hp = static_cast<int32>(nextHp > _maxHp ? _maxHp : nextHp);
}

bool CreatureStat::TakeDamage(int32 damage)
{
	if (damage <= 0)
		return false;

	if (_hp <= damage)
	{
		_hp = 0;
		return true;
	}

	_hp -= damage;
	return false;
}

void CreatureStat::SetPosition(float posX, float posY)
{
	_posX = posX;
	_posY = posY;
}

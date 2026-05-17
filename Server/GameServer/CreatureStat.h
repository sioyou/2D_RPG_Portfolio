#pragma once

/*----------------------
	CreatureStat
	- Player / Monster 공통 전투·위치 수치
-----------------------*/
class CreatureStat
{
public:
	CreatureStat() = default;

	int32 GetLevel() const { return _level; }
	int32 GetHp() const { return _hp; }
	int32 GetMaxHp() const { return _maxHp; }
	float GetPosX() const { return _posX; }
	float GetPosY() const { return _posY; }

	void SetLevel(int32 level);
	void SetMaxHp(int32 maxHp);
	bool SetHp(int32 hp);
	void Heal(int32 amount);
	bool TakeDamage(int32 damage);
	void SetPosition(float posX, float posY);

private:
	int32 _level = 1;
	int32 _hp = 100;
	int32 _maxHp = 100;
	float _posX = 0.f;
	float _posY = 0.f;
};

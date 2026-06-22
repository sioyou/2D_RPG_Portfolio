#pragma once

#include "Data/DataTypes.h"

inline constexpr float CREATURE_COLLISION_RADIUS = 0.35f;

class MapCollision
{
public:
	static MapCollision FromData(const MapCollisionData& data);

	bool IsLoaded() const { return _width > 0 && _height > 0; }
	bool IsInsideWorldBounds(float worldX, float worldY, float radius) const;
	bool IsWalkable(float worldX, float worldY) const;
	bool IsWalkableWithRadius(float worldX, float worldY, float radius) const;
	void ResolveMove(float fromX, float fromY, float toX, float toY, float radius, float& outX, float& outY) const;

	int32 GetMapId() const { return _mapId; }

private:
	bool IsTileWalkable(int32 tileX, int32 tileY) const;
	void WorldToTile(float worldX, float worldY, int32& outTileX, int32& outTileY) const;

	int32 _mapId = 0;
	float _originX = 0.f;
	float _originY = 0.f;
	float _tileSize = 1.f;
	int32 _width = 0;
	int32 _height = 0;
	std::vector<uint8> _blocked;
};

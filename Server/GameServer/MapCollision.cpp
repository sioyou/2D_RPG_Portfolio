#include "pch.h"
#include "MapCollision.h"

#include <algorithm>
#include <cmath>

MapCollision MapCollision::FromData(const MapCollisionData& data)
{
	MapCollision map;
	map._mapId = data.mapId;
	map._originX = data.originX;
	map._originY = data.originY;
	map._tileSize = data.tileSize > 0.f ? data.tileSize : 1.f;
	map._width = data.width;
	map._height = data.height;

	const size_t expectedSize = static_cast<size_t>(data.width) * static_cast<size_t>(data.height);
	map._blocked.resize(expectedSize, 0);

	for (size_t i = 0; i < expectedSize && i < data.cells.size(); ++i)
		map._blocked[i] = (data.cells[i] == '0') ? 1 : 0;

	return map;
}

bool MapCollision::IsInsideWorldBounds(float worldX, float worldY, float radius) const
{
	if (IsLoaded() == false)
		return false;

	const float minX = _originX + radius;
	const float maxX = _originX + static_cast<float>(_width) * _tileSize - radius;
	const float minY = _originY + radius;
	const float maxY = _originY + static_cast<float>(_height) * _tileSize - radius;

	if (minX > maxX || minY > maxY)
		return IsWalkable(worldX, worldY);

	return worldX >= minX && worldX <= maxX && worldY >= minY && worldY <= maxY;
}

bool MapCollision::IsTileWalkable(int32 tileX, int32 tileY) const
{
	if (tileX < 0 || tileY < 0 || tileX >= _width || tileY >= _height)
		return false;

	const size_t index = static_cast<size_t>(tileY) * static_cast<size_t>(_width) + static_cast<size_t>(tileX);
	if (index >= _blocked.size())
		return false;

	return _blocked[index] == 0;
}

void MapCollision::WorldToTile(float worldX, float worldY, int32& outTileX, int32& outTileY) const
{
	outTileX = static_cast<int32>(floorf((worldX - _originX) / _tileSize));
	outTileY = static_cast<int32>(floorf((worldY - _originY) / _tileSize));
}

bool MapCollision::IsWalkable(float worldX, float worldY) const
{
	if (IsLoaded() == false)
		return false;

	int32 tileX = 0;
	int32 tileY = 0;
	WorldToTile(worldX, worldY, tileX, tileY);
	return IsTileWalkable(tileX, tileY);
}

bool MapCollision::IsWalkableWithRadius(float worldX, float worldY, float radius) const
{
	if (IsLoaded() == false)
		return false;

	if (IsInsideWorldBounds(worldX, worldY, radius) == false)
		return false;

	if (radius <= 0.f)
		return IsWalkable(worldX, worldY);

	const float minX = worldX - radius;
	const float maxX = worldX + radius;
	const float minY = worldY - radius;
	const float maxY = worldY + radius;

	int32 minTileX = static_cast<int32>(floorf((minX - _originX) / _tileSize));
	int32 maxTileX = static_cast<int32>(floorf((maxX - _originX) / _tileSize));
	int32 minTileY = static_cast<int32>(floorf((minY - _originY) / _tileSize));
	int32 maxTileY = static_cast<int32>(floorf((maxY - _originY) / _tileSize));

	minTileX = std::max(minTileX, 0);
	minTileY = std::max(minTileY, 0);
	maxTileX = min(maxTileX, _width - 1);
	maxTileY = min(maxTileY, _height - 1);

	if (minTileX > maxTileX || minTileY > maxTileY)
		return false;

	for (int32 ty = minTileY; ty <= maxTileY; ++ty)
	{
		for (int32 tx = minTileX; tx <= maxTileX; ++tx)
		{
			if (IsTileWalkable(tx, ty) == false)
				return false;
		}
	}

	return true;
}

void MapCollision::ResolveMove(float fromX, float fromY, float toX, float toY, float radius, float& outX, float& outY) const
{
	if (IsLoaded() == false)
	{
		outX = fromX;
		outY = fromY;
		return;
	}

	if (IsWalkableWithRadius(toX, toY, radius))
	{
		outX = toX;
		outY = toY;
		return;
	}

	if (IsWalkableWithRadius(toX, fromY, radius))
	{
		outX = toX;
		outY = fromY;
		return;
	}

	if (IsWalkableWithRadius(fromX, toY, radius))
	{
		outX = fromX;
		outY = toY;
		return;
	}

	outX = fromX;
	outY = fromY;
}

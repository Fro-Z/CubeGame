using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator
{
	public const int MIN_HEIGHT = 1;
	public const int MAX_HEIGHT = 9;

	const float noiseScale = 0.1233356f; //must be a non-integer to avoid perlin artifacts
	const float areaNoiseScale = 0.0177657f;

	public void FillChunk(Chunk chunk, Vector2Int chunkPos)
	{
		var dirtBlockType = BlockRegistry.GetTypeByName("Dirt");
		var grassBlockType = BlockRegistry.GetTypeByName("Grass");

		

		for (int z = 0; z < Chunk.CHUNK_SIZE; ++z)
			for (int x = 0; x < Chunk.CHUNK_SIZE; ++x)
			{
				int worldX = (int)(chunkPos.x * Chunk.CHUNK_SIZE + x);
				int worldZ = (int)(chunkPos.y * Chunk.CHUNK_SIZE + z);

				int height = HeightFunction(worldX, worldZ);
				

				//set top to be grass, the rest is dirt
				chunk.SetBlockType(x, height - 1, z, grassBlockType);
				for(int y = height-2; y>=0; y--)
				{
					chunk.SetBlockType(x, y, z, dirtBlockType);
				}
			}
	}

	int HeightFunction(int worldX, int worldZ)
	{
		float localNoise = Mathf.PerlinNoise(worldX * noiseScale, worldZ * noiseScale);
		float areaNoise = Mathf.PerlinNoise(worldX * areaNoiseScale, worldZ * areaNoiseScale);

		// mix local and area noise 3:1
		float combinedNoise = (localNoise + areaNoise * 3) / 4;

		return (int)(combinedNoise*(MAX_HEIGHT - MIN_HEIGHT)) + MIN_HEIGHT;
	}
}



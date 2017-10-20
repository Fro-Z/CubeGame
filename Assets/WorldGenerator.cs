using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator
{


	public const int MIN_HEIGHT = 1;
	public const int MAX_HEIGHT = 5;

	public void FillChunk(Chunk chunk, Vector2Int chunkPos)
	{
		var dirtBlockType = BlockRegistry.GetTypeByName("Dirt");
		var grassBlockType = BlockRegistry.GetTypeByName("Grass");

		const float noiseScale = 0.123456f;

		for (int z = 0; z < Chunk.CHUNK_SIZE; ++z)
			for (int x = 0; x < Chunk.CHUNK_SIZE; ++x)
			{
				int worldX = (int)(chunkPos.x * Chunk.CHUNK_SIZE + x);
				int worldZ = (int)(chunkPos.y * Chunk.CHUNK_SIZE + z);

				int height = (int)(Mathf.PerlinNoise(worldX*noiseScale, worldZ*noiseScale) * (MAX_HEIGHT - MIN_HEIGHT)) + MIN_HEIGHT;
				

				//set top to be grass, the rest is dirt
				chunk.SetBlockType(x, height - 1, z, grassBlockType);
				for(int y = height-2; y>=0; y--)
				{
					chunk.SetBlockType(x, y, z, dirtBlockType);
				}
			}
	}
}



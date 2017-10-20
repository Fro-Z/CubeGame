using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator
{
	public void FillChunk(Chunk chunk, Vector2Int chunkPos)
	{
		uint usedBlockType = 1;

		for (int z = 0; z < Chunk.CHUNK_SIZE; ++z)
			for (int y = 0; y < Chunk.CHUNK_SIZE; ++y)
				for (int x = 0; x < Chunk.CHUNK_SIZE; ++x)
				{
					int height = (x + z) % 2 + 5;
					if (y <= height)
						chunk.SetBlockType(x, y, z, usedBlockType);
				}
	}
}


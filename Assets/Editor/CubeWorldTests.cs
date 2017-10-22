using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class CubeWorldTests {
	[Test]
	public void Test_GetBlockPos()
	{
		float cubeSize = CubeWorld.CUBE_SIZE;
		Assert.AreEqual(CubeWorld.GetBlockPos(new Vector3(1, 2, 3) * cubeSize), new Vector3Int(1, 2, 3));
		Assert.AreEqual(CubeWorld.GetBlockPos(new Vector3(1.01f, 2.99f, 3.5f) * cubeSize), new Vector3Int(1, 2, 3));

		Assert.AreEqual(CubeWorld.GetBlockPos(new Vector3(-1.5f, 0f, 3.5f) * cubeSize), new Vector3Int(-2, 0, 3));
	}

	[Test]
	public void Test_GetChunkPos()
	{
		Assert.IsTrue(Chunk.CHUNK_SIZE > 1);
		Assert.AreEqual(CubeWorld.GetChunkPos(new Vector3Int(0, 1, Chunk.CHUNK_SIZE - 1)), new Vector2Int(0, 0));

		Assert.AreEqual(CubeWorld.GetChunkPos(new Vector3Int(-1, 0, 0)), new Vector2Int(-1, 0));
		Assert.AreEqual(CubeWorld.GetChunkPos(new Vector3Int(-Chunk.CHUNK_SIZE, 0, 0)), new Vector2Int(-1, 0));
	}

	[Test]
	public void Test_GetChunkOffset()
	{
		Assert.IsTrue(Chunk.CHUNK_SIZE > 1);
		Assert.AreEqual(CubeWorld.GetChunkOffset(new Vector3Int(1, 2, 3)), new Vector3Int(1, 2, 3));
		Assert.AreEqual(CubeWorld.GetChunkOffset(new Vector3Int(Chunk.CHUNK_SIZE + 1, 2, 3)), new Vector3Int(1, 2, 3));

		Assert.AreEqual(CubeWorld.GetChunkOffset(new Vector3Int(-1, 0, 0)), new Vector3Int(15, 0, 0));
		Assert.AreEqual(CubeWorld.GetChunkOffset(new Vector3Int(-15, 0, 0)), new Vector3Int(1, 0, 0));
		Assert.AreEqual(CubeWorld.GetChunkOffset(new Vector3Int(-16, 0, 0)), new Vector3Int(0, 0, 0));
		Assert.AreEqual(CubeWorld.GetChunkOffset(new Vector3Int(-17, 0, 0)), new Vector3Int(15, 0, 0));
		Assert.AreEqual(CubeWorld.GetChunkOffset(new Vector3Int(-32, 0, 0)), new Vector3Int(0, 0, 0));
	}
}

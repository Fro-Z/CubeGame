using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using System;

using BlockType = System.Int32;

static class FlagExtension
{
	static public bool HasFlag(this Faces faces, Faces flag)
	{
		return ((faces & flag) == flag);
	}
}

[Flags]
public enum Faces : byte
{ NONE = 0, TOP = 1, LEFT = 2, RIGHT = 4, FRONT = 8, BACK = 16, BOTTOM = 32 }

public class Chunk : MonoBehaviour
{
	private struct BlockData
	{
		public BlockData(BlockType blockType, int damage)
		{
			this.blockType = blockType;
			this.damage = damage;
			this.visibleFaces = 0;
		}

		public BlockType blockType;
		public int damage;

		public Faces visibleFaces;

	}

	public const int CHUNK_SIZE = 16;
	private BlockData[] blocks = new BlockData[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];

	private const int MASTER_TEXTURE_WIDTH = 128;
	private const int CUBE_TEXTURE_WIDTH = 16;
	private const int BLOCK_AIR = 0;

	private Mesh mesh;
	/// Does chunk need to regenerate mesh data
	private bool isDirty = false;

	public BlockType GetBlockType(int x, int y, int z)
	{
		Assert.IsTrue(x >= 0 && y >= 0 && z >= 0 && x < CHUNK_SIZE && y < CHUNK_SIZE && z < CHUNK_SIZE, "Invalid chunk block position");
		return blocks[GetBlockIdx(x, y, z)].blockType;
	}

	public void SetBlockType(int x, int y, int z, BlockType type)
	{
		Assert.IsTrue(x >= 0 && y >= 0 && z >= 0 && x < CHUNK_SIZE && y < CHUNK_SIZE && z < CHUNK_SIZE, "Invalid chunk block position");
		var blockIdx = GetBlockIdx(x, y, z);
		blocks[blockIdx].blockType = type;
		blocks[blockIdx].damage = 0;

		isDirty = true;
	}

	private int GetBlockIdx(int x, int y, int z)
	{
		Assert.IsTrue(x >= 0 && y >= 0 && z >= 0 && x < CHUNK_SIZE && y < CHUNK_SIZE && z < CHUNK_SIZE, "Invalid chunk block position");
		return (int)(x + CHUNK_SIZE * y + (CHUNK_SIZE * CHUNK_SIZE) * z);
	}

	void Update()
	{
		if (isDirty)
			GenerateMesh();
	}

	void Start()
	{
		// create a new mesh instance for this chunk
		mesh = new Mesh();

		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (!meshFilter)
		{
			meshFilter = gameObject.AddComponent<MeshFilter>();
		}
		meshFilter.mesh = mesh;
	}

	/// <summary>
	/// Generate  mesh for this chunk. Will only generate blocks that are visible
	/// </summary>
	public void GenerateMesh()
	{
		var startTime = Time.realtimeSinceStartup;
		mesh.Clear();

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> triangleIndices = new List<int>();

		for (int z = 0; z < CHUNK_SIZE; ++z)
			for (int y = 0; y < CHUNK_SIZE; ++y)
				for (int x = 0; x < CHUNK_SIZE; ++x)
				{
					UpdateVisibleFaces(x, y, z);
					if (IsBlockVisible(x, y, z))
						GenerateBlock(x, y, z, vertices, uvs, triangleIndices);
				}

		mesh.vertices = vertices.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = triangleIndices.ToArray();
		mesh.RecalculateNormals();
		isDirty = false;

		float timeToGenMesh = (Time.realtimeSinceStartup - startTime) * 1000;

		var collisionGenStartTime = Time.realtimeSinceStartup;
		var meshCollider = GetComponent<MeshCollider>();
		if(meshCollider)
		{
			meshCollider.sharedMesh = mesh;
		}


		float timeToGenCollision = (Time.realtimeSinceStartup - collisionGenStartTime) * 1000;
		print("generating mesh took " + timeToGenMesh + "ms, + " + timeToGenCollision +"ms to generate collision");
	}

	public bool IsBlockVisible(int x, int y, int z)
	{
		var blockIdx = GetBlockIdx(x, y, z);
		return blocks[blockIdx].visibleFaces != Faces.NONE && blocks[blockIdx].blockType != 0;
	}

	private void UpdateVisibleFaces(int x, int y, int z)
	{
		var blockIdx = GetBlockIdx(x, y, z);

		if (blocks[blockIdx].blockType == BLOCK_AIR)
		{
			// air blocks have no visible faces
			blocks[blockIdx].visibleFaces = Faces.NONE;
			return;
		}

		if (x == 0 || GetBlockType(x - 1, y, z) == BLOCK_AIR)
			blocks[blockIdx].visibleFaces |= Faces.LEFT;

		if (x == CHUNK_SIZE - 1 || GetBlockType(x + 1, y, z) == BLOCK_AIR)
			blocks[blockIdx].visibleFaces |= Faces.RIGHT;


		if (y == 0 || GetBlockType(x, y - 1, z) == BLOCK_AIR)
			blocks[blockIdx].visibleFaces |= Faces.BOTTOM;

		if (y == CHUNK_SIZE - 1 || GetBlockType(x, y + 1, z) == BLOCK_AIR)
			blocks[blockIdx].visibleFaces |= Faces.TOP;


		if (z == 0 || GetBlockType(x, y, z - 1) == BLOCK_AIR)
			blocks[blockIdx].visibleFaces |= Faces.FRONT;

		if (z == CHUNK_SIZE - 1 || GetBlockType(x, y, z + 1) == BLOCK_AIR)
			blocks[blockIdx].visibleFaces |= Faces.BACK;
	}



	/// <summary>
	/// Generate mesh data for each visible face on a block
	/// </summary>
	void GenerateBlock(int x, int y, int z, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
	{
		Vector3 origin = new Vector3(x, y, z) * CubeWorld.CUBE_SIZE;
		Faces faces = blocks[GetBlockIdx(x, y, z)].visibleFaces;
		var blockType = GetBlockType(x, y, z);

		//top face
		if (faces.HasFlag(Faces.TOP))
		{
			var idxStart = vertices.Count;
			vertices.Add(new Vector3(0, 1, 0) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(1, 1, 0) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(0, 1, 1) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(1, 1, 1) * CubeWorld.CUBE_SIZE + origin);
			FillUVs(uvs, BlockRegistry.GetTextureIndex(blockType, Faces.TOP));
			triangles.AddRange(CreateTriangles(idxStart));
		}

		//bottom face
		if (faces.HasFlag(Faces.BOTTOM))
		{
			var idxStart = vertices.Count;
			vertices.Add(new Vector3(0, 0, 0) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(0, 0, 1) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(1, 0, 0) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(1, 0, 1) * CubeWorld.CUBE_SIZE + origin);
			FillUVs(uvs, BlockRegistry.GetTextureIndex(blockType, Faces.BOTTOM));
			triangles.AddRange(CreateTriangles(idxStart));
		}

		//left face
		if (faces.HasFlag(Faces.LEFT))
		{
			var idxStart = vertices.Count;
			vertices.Add(new Vector3(0, 0, 1) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(0, 0, 0) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(0, 1, 1) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(0, 1, 0) * CubeWorld.CUBE_SIZE + origin);
			FillUVs(uvs, BlockRegistry.GetTextureIndex(blockType, Faces.LEFT));
			triangles.AddRange(CreateTriangles(idxStart));
		}

		//right face
		if (faces.HasFlag(Faces.RIGHT))
		{
			var idxStart = vertices.Count;
			vertices.Add(new Vector3(1, 0, 0) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(1, 0, 1) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(1, 1, 0) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(1, 1, 1) * CubeWorld.CUBE_SIZE + origin);
			FillUVs(uvs, BlockRegistry.GetTextureIndex(blockType, Faces.RIGHT));
			triangles.AddRange(CreateTriangles(idxStart));
		}

		//front face
		if (faces.HasFlag(Faces.FRONT))
		{
			var idxStart = vertices.Count;
			vertices.Add(new Vector3(0, 0, 0) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(1, 0, 0) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(0, 1, 0) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(1, 1, 0) * CubeWorld.CUBE_SIZE + origin);
			FillUVs(uvs, BlockRegistry.GetTextureIndex(blockType, Faces.FRONT));
			triangles.AddRange(CreateTriangles(idxStart));
		}

		//back face
		if (faces.HasFlag(Faces.BACK))
		{
			var idxStart = vertices.Count;
			vertices.Add(new Vector3(1, 0, 1) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(0, 0, 1) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(1, 1, 1) * CubeWorld.CUBE_SIZE + origin);
			vertices.Add(new Vector3(0, 1, 1) * CubeWorld.CUBE_SIZE + origin);
			FillUVs(uvs, BlockRegistry.GetTextureIndex(blockType, Faces.BACK));
			triangles.AddRange(CreateTriangles(idxStart));
		}
	}

	/// <summary>
	/// Add UV coords to match textureIndex.
	/// </summary>
	/// <param name="textureIndex">position of texture in master texture</param>
	private void FillUVs(List<Vector2> uvs, uint textureIndex)
	{
		var texturesPerLine = MASTER_TEXTURE_WIDTH / CUBE_TEXTURE_WIDTH;
		Vector2 subtexturePos = new Vector2(textureIndex % texturesPerLine, textureIndex / texturesPerLine);

		// uvs are in range [0; 1], not pixels
		uvs.Add((new Vector2(0, 0) + subtexturePos) / texturesPerLine);
		uvs.Add((new Vector2(1, 0) + subtexturePos) / texturesPerLine);
		uvs.Add((new Vector2(0, 1) + subtexturePos) / texturesPerLine);
		uvs.Add((new Vector2(1, 1) + subtexturePos) / texturesPerLine);
	}

	/// <summary>
	/// Create array of triangle indices from a starting index of a quad.
	/// </summary>
	int[] CreateTriangles(int startIndex)
	{
		return new[] {startIndex + 2, startIndex + 1, startIndex, //first triangle
			startIndex + 1, startIndex + 2, startIndex + 3}; //second triangle
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;

[ExecuteInEditMode]
public class Chunk : MonoBehaviour
{
	public struct BlockData
	{
		public BlockData(uint blockType, uint damage)
		{
			this.blockType = blockType;
			this.damage = damage;
		}

		public uint blockType;
		public uint damage;
	}

	public const uint CHUNK_SIZE = 16;
	private BlockData[] blocks = new BlockData[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];

	private const uint MASTER_TEXTURE_WIDTH = 128;
	private const uint CUBE_TEXTURE_WIDTH = 16;

	private Mesh mesh;
	/// Does chunk need to regenerate mesh data
	private bool isDirty = false;

	public BlockData GetBlock(int x, int y, int z)
	{
		Assert.IsTrue(x >= 0 && y >= 0 && z >= 0 && x < CHUNK_SIZE && y < CHUNK_SIZE && z < CHUNK_SIZE, "Invalid chunk block position");
		return blocks[x + CHUNK_SIZE * y + (CHUNK_SIZE * CHUNK_SIZE) * z];
	}

	public void SetBlock(int x, int y, int z, BlockData block)
	{
		Assert.IsTrue(x >= 0 && y >= 0 && z >= 0 && x < CHUNK_SIZE && y < CHUNK_SIZE && z < CHUNK_SIZE, "Invalid chunk block position");
		blocks[x + CHUNK_SIZE * y + (CHUNK_SIZE * CHUNK_SIZE) * z] = block;
		isDirty = true;
	}

	void Update()
	{
		if (isDirty)
			GenerateMesh();
	}

	void Start()
	{
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
		mesh.Clear();

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> triangleIndices = new List<int>();

		for (int z = 0; z < CHUNK_SIZE; ++z)
			for (int y = 0; y < CHUNK_SIZE; ++y)
				for (int x = 0; x < CHUNK_SIZE; ++x)
				{
					if (IsBlockVisible(x, y, z))
						GenerateBlock(x, y, z, vertices, uvs, triangleIndices);
				}

		mesh.vertices = vertices.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = triangleIndices.ToArray();
		isDirty = false;

		var meshCollider = GetComponent<MeshCollider>();
		if(meshCollider)
		{
			meshCollider.sharedMesh = mesh;
		}
	}

	/// <summary>
	/// Is block visible from outside. 
	/// Blocks on chunk border are considered visible even when possibly occluded by another chunk's blocks.
	/// </summary>
	bool IsBlockVisible(int x, int y, int z)
	{
		if (GetBlock(x, y, z).blockType == 0)
			return false; //do not render air blocks

		//block is visible if it's located on chunk border, or neighboring block is empty
		if (x == 0 || x == CHUNK_SIZE - 1 ||
			y == 0 || y == CHUNK_SIZE - 1 ||
			z == 0 || z == CHUNK_SIZE - 1)
			return true;

		//test neighbors
		if (GetBlock(x - 1, y, z).blockType == 0 || GetBlock(x + 1, y, z).blockType == 0)
			return true;
		if (GetBlock(x, y - 1, z).blockType == 0 || GetBlock(x, y + 1, z).blockType == 0)
			return true;
		if (GetBlock(x, y, z - 1).blockType == 0 || GetBlock(x, y, z + 1).blockType == 0)
			return true;

		return false;
	}

	/// <summary>
	/// Generate mesh data for a single block
	/// </summary>
	void GenerateBlock(int x, int y, int z, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
	{
		//top face
		Vector3 origin = new Vector3(x, y, z) * CubeWorld.CUBE_SIZE;
		var idxStart = vertices.Count;
		vertices.Add(new Vector3(0, 1, 0) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(1, 1, 0) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(0, 1, 1) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(1, 1, 1) * CubeWorld.CUBE_SIZE + origin);
		FillUVs(uvs, 0);
		triangles.AddRange(CreateTriangles(idxStart));

		//bottom face
		idxStart = vertices.Count;
		vertices.Add(new Vector3(0, 0, 0) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(0, 0, 1) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(1, 0, 0) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(1, 0, 1) * CubeWorld.CUBE_SIZE + origin);
		FillUVs(uvs, 0);
		triangles.AddRange(CreateTriangles(idxStart));

		//left face
		idxStart = vertices.Count;
		vertices.Add(new Vector3(0, 0, 0) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(0, 1, 0) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(0, 0, 1) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(0, 1, 1) * CubeWorld.CUBE_SIZE + origin);
		FillUVs(uvs, 0);
		triangles.AddRange(CreateTriangles(idxStart));

		//right face
		idxStart = vertices.Count;
		vertices.Add(new Vector3(1, 0, 0) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(1, 0, 1) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(1, 1, 0) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(1, 1, 1) * CubeWorld.CUBE_SIZE + origin);
		FillUVs(uvs, 0);
		triangles.AddRange(CreateTriangles(idxStart));

		//front face
		idxStart = vertices.Count;
		vertices.Add(new Vector3(0, 0, 0) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(1, 0, 0) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(0, 1, 0) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(1, 1, 0) * CubeWorld.CUBE_SIZE + origin);
		FillUVs(uvs, 0);
		triangles.AddRange(CreateTriangles(idxStart));

		//back face
		idxStart = vertices.Count;
		vertices.Add(new Vector3(0, 0, 1) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(0, 1, 1) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(1, 0, 1) * CubeWorld.CUBE_SIZE + origin);
		vertices.Add(new Vector3(1, 1, 1) * CubeWorld.CUBE_SIZE + origin);
		FillUVs(uvs, 0);
		triangles.AddRange(CreateTriangles(idxStart));
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
		uvs.Add((new Vector2(0, 1) + subtexturePos) / texturesPerLine);
		uvs.Add((new Vector2(1, 0) + subtexturePos) / texturesPerLine);
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
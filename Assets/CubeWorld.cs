using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BlockType = System.Int32;

public class CubeWorld : MonoBehaviour {

	public const int CUBE_SIZE = 2;
	public const int viewDistance = 4;

	public Material chunkMaterial;
	private WorldGenerator generator = new WorldGenerator();

	Dictionary<Vector2Int, GameObject> loadedChunks = new Dictionary<Vector2Int, GameObject>();

	void Start ()
	{
		transform.position = new Vector3(0, 0, 0);

		for(int x = -viewDistance; x< viewDistance; x++)
			for(int y = -viewDistance; y< viewDistance; y++)
			{
				LoadChunk(new Vector2Int(x, y));
			}
	}

	/// <summary>
	/// Get position of a block at world position
	/// </summary>
	public static Vector3Int GetBlockPos(Vector3 worldPos)
	{
		Vector3Int blockPos = new Vector3Int((int)worldPos.x / CUBE_SIZE, (int)worldPos.y / CUBE_SIZE, (int)worldPos.z / CUBE_SIZE);

		//if worldPos coordinates are negative, we need to move resulting block pos by -1
		if (worldPos.x < 0)
			blockPos.x -= 1;
		if (worldPos.y < 0)
			blockPos.y -= 1;
		if (worldPos.z < 0)
			blockPos.z -= 1;

		return blockPos;
	}

	/// <summary>
	/// Get chunk position from world position
	/// </summary>
	public static Vector2Int GetChunkPos(Vector3Int worldPos)
	{
		Vector2Int chunkPos = new Vector2Int(worldPos.x / Chunk.CHUNK_SIZE, worldPos.z / Chunk.CHUNK_SIZE);

		//negative coordinates require special handling
		if (worldPos.x < 0 && worldPos.x % Chunk.CHUNK_SIZE != 0)
			chunkPos.x -= 1;
		if (worldPos.z < 0 && worldPos.z % Chunk.CHUNK_SIZE != 0)
			chunkPos.y -= 1;

		return chunkPos;
	}

	/// <summary>
	/// Get chunk offset (position of a block inside a chunk) from world pos. Correctly handles negative coordinates.
	/// </summary>
	public static Vector3Int GetChunkOffset(Vector3Int worldPos)
	{
		Vector2Int chunkPos = GetChunkPos(worldPos);
		Vector3Int chunkOffset = new Vector3Int(
		(worldPos.x - chunkPos.x * Chunk.CHUNK_SIZE) % Chunk.CHUNK_SIZE,
		worldPos.y,
		(worldPos.z - chunkPos.y * Chunk.CHUNK_SIZE) % Chunk.CHUNK_SIZE);

		return chunkOffset;
	}


	public void SetBlockType(Vector3Int worldPos, BlockType type)
	{
		Vector2Int chunkPos = GetChunkPos(worldPos);
		
		GameObject chunkObject;
		if(loadedChunks.TryGetValue(chunkPos, out chunkObject))
		{
			Vector3Int offset = GetChunkOffset(worldPos);

			chunkObject.GetComponent<Chunk>().SetBlockType(offset.x, offset.y, offset.z, type);
		}
	}

	public BlockType GetBlockType(Vector3Int worldPos)
	{
		Vector2Int chunkPos = GetChunkPos(worldPos);

		GameObject chunkObject;
		if (loadedChunks.TryGetValue(chunkPos, out chunkObject))
		{
			Vector3Int offset = GetChunkOffset(worldPos);

			return chunkObject.GetComponent<Chunk>().GetBlockType(offset.x, offset.y, offset.z);
		}
		else
			return 0;
	}

	/// <summary>
	/// Apply damage to a block. Damage exceeding block's maxDamage will destroy the block.
	/// </summary>
	public void DamageBlock(Vector3Int worldPos)
	{
		Vector2Int chunkPos = GetChunkPos(worldPos);

		GameObject chunkObject;
		if (loadedChunks.TryGetValue(chunkPos, out chunkObject))
		{
			Vector3Int offset = GetChunkOffset(worldPos);

			chunkObject.GetComponent<Chunk>().DamageBlock(offset.x, offset.x, offset.x);
		}
	}

	void Update()
	{
		UpdateVisibleChunks();
	}

	private GameObject CreateChunk(Vector2Int chunkPos)
	{
		GameObject chunkObject = new GameObject("chunk", typeof(MeshFilter), typeof(MeshRenderer), typeof(Chunk), typeof(MeshCollider));

		// move to correct position
		chunkObject.transform.parent = this.transform;
		Vector2 pos2D = chunkPos * (int)(Chunk.CHUNK_SIZE)*CUBE_SIZE;
		chunkObject.transform.localPosition = new Vector3(pos2D.x, 0, pos2D.y);

		// set default material for chunk
		chunkObject.GetComponent<MeshRenderer>().material = chunkMaterial;

		// generate blocks
		Chunk chunk = chunkObject.GetComponent<Chunk>();
		generator.FillChunk(chunk, chunkPos);

		return chunkObject;
	}

	private void UpdateVisibleChunks()
	{
		var cameraPos = Camera.main.gameObject.transform.position;
		cameraPos /= (Chunk.CHUNK_SIZE * CUBE_SIZE); //transform to chunk coordinates

		Vector2Int centerChunk = new Vector2Int((int)(cameraPos.x), (int)(cameraPos.z));

		//find all chunks in an area around camera
		HashSet<Vector2Int> visibleChunks = new HashSet<Vector2Int>();
		for(int x = centerChunk.x - viewDistance; x< centerChunk.x + viewDistance; x++)
			for (int y = centerChunk.y - viewDistance; y < centerChunk.y + viewDistance; y++)
			{
				visibleChunks.Add(new Vector2Int(x, y));
			}

		var chunksToRemove = new HashSet<Vector2Int>(loadedChunks.Keys);
		chunksToRemove.ExceptWith(visibleChunks);
		RemoveChunks(chunksToRemove);

		var chunksToLoad = new HashSet<Vector2Int>(visibleChunks);
		chunksToLoad.ExceptWith(loadedChunks.Keys);
		LoadChunks(chunksToLoad);
	}

	private void RemoveChunks(HashSet<Vector2Int> chunksToRemove)
	{
		foreach (Vector2Int chunkPos in chunksToRemove)
		{
			GameObject chunk;
			if(loadedChunks.TryGetValue(chunkPos,out chunk))
			{
				loadedChunks.Remove(chunkPos);
				Destroy(chunk);
			}
		}
	}

	private void LoadChunks(HashSet<Vector2Int> chunksToLoad)
	{
		foreach (Vector2Int pos in chunksToLoad)
			LoadChunk(pos);
	}

	private void LoadChunk(Vector2Int pos)
	{
		loadedChunks.Add(pos, CreateChunk(pos));
	}
}



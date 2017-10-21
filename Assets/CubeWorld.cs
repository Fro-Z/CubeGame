using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;



public class CubeWorld : MonoBehaviour {

	public const int CUBE_SIZE = 2;
	public const int viewDistance = 4;

	public Material chunkMaterial;
	private WorldGenerator generator = new WorldGenerator();

	Dictionary<Vector2Int, GameObject> loadedChunks = new Dictionary<Vector2Int, GameObject>();

	void Start ()
	{
		for(int x = -viewDistance; x< viewDistance; x++)
			for(int y = -viewDistance; y< viewDistance; y++)
			{
				LoadChunk(new Vector2Int(x, y));
			}

	}

	GameObject CreateChunk(Vector2Int chunkPos)
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

	void Update ()
	{
		UpdateVisibleChunks();
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

	void LoadChunk(Vector2Int pos)
	{
		loadedChunks.Add(pos, CreateChunk(pos));
	}


}

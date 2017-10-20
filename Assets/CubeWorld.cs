using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;



public class CubeWorld : MonoBehaviour {

	public const uint CUBE_SIZE = 2;

	public Material chunkMaterial;
	private WorldGenerator generator = new WorldGenerator();

	void Start ()
	{
		for(int x = -5; x< 5; x++)
			for(int y = -5; y< 5; y++)
			{
				CreateChunk(new Vector2Int(x, y));
			}

	}

	void CreateChunk(Vector2Int chunkPos)
	{
		GameObject chunkObject = new GameObject("chunk", typeof(MeshFilter), typeof(MeshRenderer), typeof(Chunk), typeof(MeshCollider));

		// move to correct position
		chunkObject.transform.parent = this.transform;
		Vector2 pos2D = chunkPos * (int)(Chunk.CHUNK_SIZE);
		chunkObject.transform.localPosition = new Vector3(pos2D.x, 0, pos2D.y);

		// set default material for chunk
		chunkObject.GetComponent<MeshRenderer>().material = chunkMaterial;

		// generate blocks
		Chunk chunk = chunkObject.GetComponent<Chunk>();
		generator.FillChunk(chunk, chunkPos);
	}

	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;



public class CubeWorld : MonoBehaviour {

	public const int CUBE_SIZE = 2;
	public const int viewDistance = 5;

	public Material chunkMaterial;
	private WorldGenerator generator = new WorldGenerator();

	void Start ()
	{
		for(int x = -viewDistance; x< viewDistance; x++)
			for(int y = -viewDistance; y< viewDistance; y++)
			{
				CreateChunk(new Vector2Int(x, y));
			}

	}

	void CreateChunk(Vector2Int chunkPos)
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
	}

	void Update () {
		
	}
}

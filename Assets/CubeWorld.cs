using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


[ExecuteInEditMode]
public class CubeWorld : MonoBehaviour {

	public const uint CUBE_SIZE = 2;

	public Material chunkMaterial;

	GameObject testChunkObject;
	
	
	void Start ()
	{
		runInEditMode = true;
		if(!testChunkObject)
		{
			testChunkObject = new GameObject("chunk", typeof(MeshFilter), typeof(MeshRenderer), typeof(Chunk), typeof(MeshCollider));

			//set default material for chunk
			var meshRenderer = testChunkObject.GetComponent<MeshRenderer>();
			if(meshRenderer)
			{
				meshRenderer.material = chunkMaterial;
			}

		}

		Chunk testChunk = testChunkObject.GetComponent<Chunk>();

		for (int z = 0; z < Chunk.CHUNK_SIZE; ++z)
			for (int y = 0; y < Chunk.CHUNK_SIZE; ++y)
				for (int x = 0; x < Chunk.CHUNK_SIZE; ++x)
				{
					if ((x + y + z) % 7 == 0)
						testChunk.SetBlock(x, y, z, new Chunk.BlockData(1, 0));
				}
	}

	private void OnDestroy()
	{
		DestroyImmediate(testChunkObject);
	}

	void Update () {
		
	}
}

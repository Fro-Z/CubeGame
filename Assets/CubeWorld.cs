using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;



public class CubeWorld : MonoBehaviour {

	public const uint CUBE_SIZE = 2;

	public Material chunkMaterial;
	private WorldGenerator generator = new WorldGenerator();

	GameObject testChunkObject;
	
	
	void Start ()
	{
		runInEditMode = true;
		if(!testChunkObject)
		{
			testChunkObject = new GameObject("chunk", typeof(MeshFilter), typeof(MeshRenderer), typeof(Chunk), typeof(MeshCollider));
			testChunkObject.transform.parent = this.transform;

			//set default material for chunk
			var meshRenderer = testChunkObject.GetComponent<MeshRenderer>();
			if(meshRenderer)
			{
				meshRenderer.material = chunkMaterial;
			}

		}

		Chunk testChunk = testChunkObject.GetComponent<Chunk>();
		generator.FillChunk(testChunk, new Vector2Int(0, 0));
	}

	private void OnDestroy()
	{
		DestroyImmediate(testChunkObject.gameObject);
	}

	void Update () {
		
	}
}

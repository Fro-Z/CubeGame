using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BlockType = System.Int32;

/// <summary>
/// Player switches between build and destroy mode with right mouse button. When in build mode block preview is shown.
/// </summary>
public class Player : MonoBehaviour {

	enum InteractionMode { BUILD, DESTROY}

	InteractionMode mode = InteractionMode.DESTROY;
	BlockType selectedBlock;
	CubeWorld world;

	float distanceLimit = 8;

	GameObject placementCube; //cube to show placement position of a new block
	public Material placementCubeMaterial;
	public GameObject progressBarObject;
	
	const BlockType BLOCK_AIR = 0;

	void Start ()
	{
		selectedBlock = BlockRegistry.GetTypeByName("Dirt");

		var worldObject = GameObject.Find("World");
		if(worldObject)
			world = worldObject.GetComponent<CubeWorld>();

		placementCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		placementCube.GetComponent<MeshRenderer>().material = placementCubeMaterial;
		placementCube.GetComponent<MeshRenderer>().enabled = false;
		placementCube.GetComponent<BoxCollider>().enabled = false;

		//cube is slightly bigger, to prevent depth fighting
		placementCube.transform.localScale = new Vector3(CubeWorld.CUBE_SIZE, CubeWorld.CUBE_SIZE, CubeWorld.CUBE_SIZE)*1.05f;
		

	}

	void UpdateProgressBar()
	{
		var progressBar = progressBarObject.GetComponent<Image>();
		if (progressBar == null)
			return;

		Vector3Int aimpoint;
		if(GetAimpointNextBlock(out aimpoint))
		{
			progressBar.enabled = true;
			progressBar.fillAmount = world.GetBlockHealthRatio(aimpoint);
		}
		else
		{
			progressBar.enabled = false;
		}

		
	}
	
	void Update ()
	{
		if (Input.GetKeyDown("mouse 1"))
			ToggleMode();

		if (mode == InteractionMode.BUILD)
			HandleBuildModeLogic();
		else
			HandleDestroyModeLogic();

		UpdatePlacementCube();
		UpdateProgressBar();
	}

	void ToggleMode()
	{
		if(mode == InteractionMode.BUILD)
		{
			mode = InteractionMode.DESTROY;
		}
		else
		{
			mode = InteractionMode.BUILD;
		}
	}

	private void HandleBuildModeLogic()
	{
		if(Input.GetKeyDown("mouse 0"))
		{
			PlaceBlock();
		}

		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			selectedBlock = BlockRegistry.GetTypeByName("Dirt");
		}

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			selectedBlock = BlockRegistry.GetTypeByName("Grass");
		}

		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			selectedBlock = BlockRegistry.GetTypeByName("Stone");
		}

		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			selectedBlock = BlockRegistry.GetTypeByName("Sand");
		}

		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			selectedBlock = BlockRegistry.GetTypeByName("Brick");
		}
	}

	void PlaceBlock()
	{
		Vector3Int blockPos;
		if(GetAimpointPreviousBlock(out blockPos))
		{
			if(CanBuild(blockPos))
				world.SetBlockType(blockPos, selectedBlock);
		}
	}

	bool CanBuild(Vector3Int blockPos)
	{
		Vector3Int playerBlockPos = CubeWorld.GetBlockPos(gameObject.transform.position);
		if (blockPos.x == playerBlockPos.x && blockPos.z == playerBlockPos.z &&
			Mathf.Abs(playerBlockPos.y - blockPos.y) <= 2)
			return false; //prevent building inside yourself and falling through the new mesh

		return world.GetBlockType(blockPos) == BLOCK_AIR;
	}

	/// <summary>
	/// Get cube before raycast hit from camera
	/// </summary>
	bool GetAimpointPreviousBlock(out Vector3Int blockPos)
	{
		//raycast to find block empty block position
		Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, distanceLimit))
		{
			//move hit position to roughly be in previous block
			hit.point += (hit.normal * 0.1f);
			blockPos = CubeWorld.GetBlockPos(hit.point);
			Debug.DrawLine(ray.origin, hit.point, Color.red, 10.0f, true);
			return true;
		}

		blockPos = new Vector3Int();
		return false;
	}

	/// <summary>
	/// Get cube after raycast hit from camera
	/// </summary>
	bool GetAimpointNextBlock(out Vector3Int blockPos)
	{
		//raycast to find block empty block position
		Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, distanceLimit))
		{
			//move hit position to roughly be in previous block
			hit.point -= (hit.normal * 0.1f);
			blockPos = CubeWorld.GetBlockPos(hit.point);
			return true;
		}

		blockPos = new Vector3Int();
		return false;
	}

	private void HandleDestroyModeLogic()
	{
		if (Input.GetKeyDown("mouse 0"))
		{
			DamageBlock();
		}
	}

	private void UpdatePlacementCube()
	{
		// find aimpoint block. When in build mode look for blocks before hit point.
		Vector3Int aimpointBlock;
		if(mode == InteractionMode.DESTROY)
		{
			if (!GetAimpointNextBlock(out aimpointBlock))
			{
				placementCube.GetComponent<MeshRenderer>().enabled = false;
				return;
			}
		}
		else
		{ //Interaction == BUILD
			if(!GetAimpointPreviousBlock(out aimpointBlock))
			{
				placementCube.GetComponent<MeshRenderer>().enabled = false;
				return;
			}
		}


		placementCube.GetComponent<MeshRenderer>().enabled = true;
		Vector3 pivotOffset = new Vector3(0.5f, 0.5f, 0.5f) * CubeWorld.CUBE_SIZE;
		placementCube.transform.position = aimpointBlock * CubeWorld.CUBE_SIZE + pivotOffset;
	}

	/// <summary>
	/// Damage block in front of a player
	/// </summary>
	private void DamageBlock()
	{
		Vector3Int aimpointBlock;
		if(GetAimpointNextBlock(out aimpointBlock))
		{
			world.DamageBlock(aimpointBlock);
		}
	}

}

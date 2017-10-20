using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BlockType = System.Int32;

/// <summary>
/// Global registry for block types.
/// </summary>
public class BlockRegistry
{
	/// <summary>
	/// Class to contain block type info. Single Block instance exists for each block type.
	/// </summary>
	private class Block
	{
		public Block(string name, uint textureStartIdx)
		{
			this._name = name;
			this.textureStartIdx = textureStartIdx;
		}

		public string name
		{
			get { return _name; }
		}

		private string _name;
		protected uint textureStartIdx;

		public virtual uint GetTextureIndex(Faces face)
		{
			return textureStartIdx;
		}
	}

	/// <summary>
	/// Block with separate textures for BOTTOM, SIDE, TOP
	/// </summary>
	private class MultiFaceBlock : Block
	{
		public MultiFaceBlock(string name, uint textureStartIdx): base(name, textureStartIdx)
		{

		}

		public override uint GetTextureIndex(Faces face)
		{
			if (face == Faces.LEFT || face == Faces.RIGHT || face == Faces.FRONT || face == Faces.BACK)
				return textureStartIdx + 1;

			if (face == Faces.TOP)
				return textureStartIdx + 2;

			return textureStartIdx; //for BOTTOM, and invalid values
		}

	}

	public static uint GetTextureIndex(BlockType blockType, Faces face)
	{
		return blockTypes[blockType].GetTextureIndex(face);
	}

	private static Block[] blockTypes =
	{
		new Block("Air", 0), //dummy block
		new Block("Dirt", 0),
		new MultiFaceBlock("Grass", 0),
		new Block("Stone", 3),
		new Block("Sand", 4),
		new Block("Brick", 5)
	};

	public static BlockType GetTypeByName(string name)
	{
		for (int i = 0; i < blockTypes.Length; i++)
			if (blockTypes[i].name == name)
				return (BlockType)(i);

		throw new Exception("Invalid block type");
	}



}



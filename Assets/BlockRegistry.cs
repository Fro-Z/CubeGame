using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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
		public Block(string name, uint textureStartIdx, int maxDamage)
		{
			this._name = name;
			this.textureStartIdx = textureStartIdx;
			this._maxDamage = maxDamage;
		}

		public string name
		{
			get { return _name; }
		}

		public int maxDamage
		{
			get { return _maxDamage; }
		}

		private string _name;
		private int _maxDamage;
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
		public MultiFaceBlock(string name, uint textureStartIdx, int maxDamage): base(name, textureStartIdx, maxDamage)
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
		new Block("Air", 0, 0), //dummy block
		new Block("Dirt", 0, 2),
		new MultiFaceBlock("Grass", 0, 2),
		new Block("Stone", 3, 3),
		new Block("Sand", 4, 1),
		new Block("Brick", 5, 4)
	};

	public static BlockType GetTypeByName(string name)
	{
		for (int i = 0; i < blockTypes.Length; i++)
			if (blockTypes[i].name == name)
				return (BlockType)(i);

		throw new Exception("Invalid block type");
	}

	public static int GetMaxDamage(BlockType type)
	{
		Assert.IsTrue(type < blockTypes.Length);
		return blockTypes[type].maxDamage;
	}

}



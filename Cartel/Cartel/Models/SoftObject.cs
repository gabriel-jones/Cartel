﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Cartel.Managers;
using Microsoft.Xna.Framework;
using Cartel.Interfaces;

namespace Cartel.Models {
	public enum SoftObjectType {
		Concrete
	}

	public class SoftObject : Interfaces.IDrawable, ISelectable {
		// Fields
		SoftObjectType type;
		int stackCount = 50;

		// Properties
		public SoftObjectType Type {
			get { return type; }
		}

		public int StackCount {
			get { return stackCount; }
		}

		public int Count { get; protected set; }

		public bool IsSelected { get; set; }

		public Texture2D Texture {
			get {
				switch (type) {
					case SoftObjectType.Concrete:
						return AssetManager.GetAsset<Texture2D>("so_concrete");
				}
				return null;
			}
		}

		// Constructor
		public SoftObject(SoftObjectType type, int amount = 0) {
			this.type = type;
			Count = amount;
		}

		public SoftObject(SoftObject clone, int amount) {
			type = clone.Type;
			Count = amount;
		}

		// Methods
		public SoftObject Clone(int? amount = null) {
			return new SoftObject(this, amount ?? Count);
		}

		public void Draw(SpriteBatch spriteBatch, int x, int y) {
			if (Texture == null) {
				return;
			}

			if (IsSelected) {
				ShapeManager.DrawRectangle(spriteBatch, new Rectangle(x * World.BlockSize, y * World.BlockSize, World.BlockSize, World.BlockSize), Color.Transparent, Color.Red);
			}

			spriteBatch.Draw(Texture,
				new Rectangle(x * World.BlockSize, y * World.BlockSize, World.BlockSize, World.BlockSize),
				new Rectangle(0, 0, Texture.Width, Texture.Height),
				Color.White,
				0.0f,
				new Vector2(0),
				SpriteEffects.None,
				DrawLayer.SoftObject
			);

			spriteBatch.DrawString(AssetManager.GetDefault<SpriteFont>(), "" + Count, new Vector2(x * World.BlockSize, y * World.BlockSize), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, DrawLayer.SoftObject + 0.01f);
		}

		public List<SoftObject> Split() {
			List<SoftObject> split = new List<SoftObject>();
			while (Count > StackCount) {
				ReduceCount(StackCount);
				int amount = Math.Min(StackCount, Count);
				split.Add(new SoftObject(this, amount));
			}
			split.Add(this);
			return split;
		}

		public void ReduceCount(int amount) {
			Count -= amount;
			if (Count < 0) {
				Count = 0;
			}
		}

		public int IncreaseCount(int amount, bool limitToStackCount = false) {
			Count += amount;
			if (limitToStackCount && Count > StackCount) {
				int remainder = Count - StackCount;
				Count = StackCount;
				return remainder;
			}
			return 0;
		}
	}
}

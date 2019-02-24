using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Cartel.Managers;

namespace Cartel.Models {
	public class Tile : Constructable {
		// Enums
		public enum TileType {
			Dirt, Grass, Sand, Water
		};

		// Fields
		TileType tileType;

		public override Texture2D Texture {
			get { return AssetManager.GetAsset<Texture2D>("tile_atlas"); }
		}

		public override List<SoftObject> BuildRequirements {
			get {
				return new List<SoftObject> {
					// TODO: 
				};
			}
		}

		// Constructor
		public Tile(Cell cell, TileType tileType) : base(cell) {
			this.tileType = tileType;
		}

		// Methods
		public override void Draw(SpriteBatch spriteBatch, int x, int y) {
			base.Draw(spriteBatch, x, y, DrawLayer.Tile);
		}

		public override Rectangle CalculateSourceRectangle() {
			int index = (int)tileType;
			int width = Texture.Width / 4;
			int height = Texture.Height / 4;
			int row = (int)((float)index / 4.0f);
			int column = index % 4;

			return new Rectangle(width * column, height * row, width, height);
		}

	}
}

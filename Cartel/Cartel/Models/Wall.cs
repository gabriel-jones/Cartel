using Cartel.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Cartel.Models {
	public class Wall: Structure {
		// Fields


		// Properties
		public override Texture2D Texture {
			get {
				return AssetManager.GetAsset<Texture2D>("wall");
			}
		}

		public override List<SoftObject> BuildRequirements {
			get {
				return new List<SoftObject> {
					new SoftObject(SoftObjectType.Concrete, 2)
				};
			}
		}

		public override bool IsTraversible {
			get { return false; }
		}

		// Constructor
		public Wall(Cell cell) : base(cell) {

		}

		// Methods
		public override Rectangle CalculateSourceRectangle() {
			Cell east = World.GetCellAt(X + 1, Y);
			Cell north = World.GetCellAt(X, Y - 1);
			Cell west = World.GetCellAt(X - 1, Y);
			Cell south = World.GetCellAt(X, Y + 1);

			bool tileEast = (east == null || east.Structure == null) ? false : east.Structure == this;
			bool tileNorth = (north == null || north.Structure == null) ? false : north.Structure == this;
			bool tileWest = (west == null || west.Structure == null) ? false : west.Structure == this;
			bool tileSouth = (south == null || south.Structure == null) ? false : south.Structure == this;

			int index = (1 * (tileNorth ? 1 : 0)) + (2 * (tileWest ? 1 : 0)) + (4 * (tileEast ? 1 : 0)) + (8 * (tileSouth ? 1 : 0));

			int width = Texture.Width / 4;
			int height = Texture.Height / 4;
			int row = (int)((float)index / 4.0f);
			int column = index % 4;

			return new Rectangle(width * column, height * row, width, height);
		}
	}
}

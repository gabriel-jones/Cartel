using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cartel.Models;
using Microsoft.Xna.Framework;
using Cartel.Managers;

namespace Cartel.Models {
	public abstract class Constructable : Interfaces.IDrawable, Interfaces.ISelectable {
		// Fields
		protected Cell cell;

		// Properties
		public bool IsSelected { get; set; }

		public World World {
			get { return cell.World; }
		}

		public int X {
			get { return cell.X; }
		}

		public int Y {
			get { return cell.Y; }
		}

		public Cell Cell {
			get { return cell; }
		}

		public abstract Texture2D Texture { get; }

		public virtual Tuple<int, int> Dimensions {
			get { return Tuple.Create(1, 1); }
		}

		public virtual List<SoftObject> BuildRequirements { get { return new List<SoftObject>(); } }

		// Constructors
		public Constructable(Cell cell) {
			this.cell = cell;
		}

		// Methods
		public virtual void Draw(SpriteBatch spriteBatch, int x, int y) {
			this.Draw(spriteBatch, x, y, DrawLayer.Tile);
		}

		protected void Draw(SpriteBatch spriteBatch, int x, int y, float layerDepth) {
			spriteBatch.Draw(Texture ?? AssetManager.GetDefault<Texture2D>(),
				new Rectangle(x * World.BlockSize, y * World.BlockSize, World.BlockSize * Dimensions.Item1, World.BlockSize * Dimensions.Item2),
				CalculateSourceRectangle(),
				Color.White,
				0.0f,
				new Vector2(0),
				SpriteEffects.None,
				layerDepth
			);

			if (IsSelected) {
				ShapeManager.DrawRectangle(spriteBatch, new Rectangle(x * World.BlockSize, y * World.BlockSize, World.BlockSize, World.BlockSize), Color.Transparent, Color.Green);
			}
		}

		// Virtual Methods
		public virtual Rectangle CalculateSourceRectangle() {
			return new Rectangle(0, 0, Texture.Width, Texture.Height);
		}
	}
}

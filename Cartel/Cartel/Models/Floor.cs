using Cartel.Managers;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static Cartel.Models.SoftObject;

namespace Cartel.Models {
	public enum FloorType {
		Concrete
	}

	public class Floor: Constructable {
		// Fields
		FloorType type;

		// Properties
		override public Texture2D Texture {
			get {
				switch (type) {
					case FloorType.Concrete:
						return AssetManager.GetAsset<Texture2D>("floor_concrete");
				}
				return null;
			}
		}

		public override List<SoftObject> BuildRequirements {
			get {
				return new List<SoftObject> {
					new SoftObject(SoftObjectType.Concrete, 5)
				};
			}
		}

		public FloorType Type {
			get { return type; }
		}

		// Constructor
		public Floor(Cell cell, FloorType floorType) : base(cell) {
			this.type = floorType;
		}

		// Methods
		public override void Draw(SpriteBatch spriteBatch, int x, int y) {
			base.Draw(spriteBatch, x, y, DrawLayer.Floor);
		}
	}
}

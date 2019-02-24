using Cartel.Managers;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Models {
	public class ObjectInfo {
		// Enums
		public enum ObjectType {
			Door
		}

		// Properties
		public ObjectType Type { get; protected set; }
		public Tuple<int, int> Dimensions { get; protected set; }

		public Texture2D Texture {
			get {
				switch (Type) {
					case ObjectType.Door:
						return AssetManager.GetAsset<Texture2D>("door");
				}
				return null;
			}
		}

		public List<SoftObject> BuildRequirements {
			get {
				return new List<SoftObject> { // TODO: dynamic
					new SoftObject(SoftObjectType.Concrete, 5)
				};
			}
		}

		// Constructors
		public ObjectInfo(ObjectType type) {
			this.Type = type;
			this.Dimensions = Tuple.Create(1, 1);
		}

		public ObjectInfo(ObjectType type, Tuple<int, int> dimensions) : this(type) {
			this.Dimensions = dimensions;
		}
	}
}

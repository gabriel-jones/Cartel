using Cartel.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Models {
	public enum ObjectType {
		Door, Bench
	}

	public class ObjectInfo {
		// Properties
		public ObjectType Type { get; protected set; }
		public Point Dimensions { get; protected set; }

		public Texture2D Texture {
			get {
				switch (Type) {
					case ObjectType.Door:
						return AssetManager.GetAsset<Texture2D>("door");
					case ObjectType.Bench:
						return AssetManager.GetAsset<Texture2D>("bench");
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
			Type = type;
			Dimensions = new Point(1, 1);
		}

		public ObjectInfo(ObjectType type, Point dimensions) : this(type) {
			Dimensions = dimensions;
		}
	}
}

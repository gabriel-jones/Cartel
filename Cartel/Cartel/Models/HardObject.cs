using Cartel.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Cartel.Models.ObjectInfo;

namespace Cartel.Models {
	public class HardObject : Structure {
		// Fields
		ObjectInfo objectInfo;

		// Properties
		public override Texture2D Texture {
			get { return objectInfo.Texture; }
		}

		public override float MovementCost {
			get { return 0.8f; }
		}

		public override Point Dimensions {
			get { return objectInfo.Dimensions; }
		}

		public override List<SoftObject> BuildRequirements {
			get { return objectInfo.BuildRequirements; }
		}

		// Constructor
		public HardObject(ObjectInfo objectInfo, Cell cell) : base(cell) {
			this.objectInfo = objectInfo;
		}
	}
}

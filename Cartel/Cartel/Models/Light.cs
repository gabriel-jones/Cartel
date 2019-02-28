using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Cartel.Managers;

namespace Cartel.Models {
	public class Light : Structure {
		public override Texture2D Texture {
			get { return AssetManager.GetAsset<Texture2D>("light"); }
		}

		public Light(Cell cell) : base(cell) {

		}


	}
}

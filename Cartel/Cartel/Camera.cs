using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cartel {
    public class Camera {
        // Properties
        private Matrix transform;
        Vector2 center;
        Viewport viewport;

        // Accessors
        public Matrix Transform {
            get { return transform; }
        }

		public Vector2 Center {
			get { return center; }
		}

        // Constructor
        public Camera(Viewport viewport) {
            this.viewport = viewport;
        }

        // Methods
        public void Update(Vector2 position, float zoom, int offsetX, int offsetY) {
            center = position;
			transform = Matrix.CreateTranslation(new Vector3(-center.X, -center.Y, 0));
			transform *= Matrix.CreateScale(new Vector3(zoom, zoom, 1));
			transform *= Matrix.CreateTranslation(new Vector3(viewport.Width / 2, viewport.Height / 2, 0));
        }
    }
}

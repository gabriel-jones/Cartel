using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Cartel.Interfaces {
    public interface IDrawable {
		Texture2D Texture { get; }
		void Draw(SpriteBatch spriteBatch, int x, int y);
    }
}

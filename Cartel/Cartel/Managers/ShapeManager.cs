using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cartel.Managers {
	class ShapeManager {
		static Texture2D basicTexture;
		static Texture2D circleTexture {
			get {
				return AssetManager.GetAsset<Texture2D>("gui_circle");
			}
		}

		public static void Setup(GraphicsDevice graphicsDevice) {
			basicTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
			basicTexture.SetData(new Color[] { Color.White });
		}

		public static void DrawLine(SpriteBatch spriteBatch, Point start, Point end, Color color, int thickness = 2) {
			float dX = end.X - start.X;
			float dY = end.Y - start.Y;
			float rotation = (float)Math.Atan2(dY, dX);
			int length = (int)Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2));
			spriteBatch.Draw(basicTexture, new Rectangle(start.X - (thickness / 2), start.Y - (thickness / 2), length + thickness, thickness), null, color, rotation, Vector2.Zero, SpriteEffects.None, DrawLayer.Primitive);
		}

		public static void DrawCircle(SpriteBatch spriteBatch, Point center, int radius, Color borderColor) {
			spriteBatch.Draw(circleTexture, new Rectangle(center.X - radius, center.Y - radius, 2 * radius, 2 * radius), null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, DrawLayer.Primitive);
		}

		public static void DrawRectangle(SpriteBatch spriteBatch, Rectangle area, Color fillColor, Color borderColor, BitArray mask = null, float drawLayer = 1.0f) {
			if (mask == null) {
				mask = new BitArray(4, true);
			}

			int thickness = 2;
			spriteBatch.Draw(basicTexture, area, null, fillColor, 0f, Vector2.Zero, SpriteEffects.None, drawLayer - 0.01f); // fill

			// N E S W
			if (mask[0]) {
				//DrawLine(spriteBatch, new Point(area.Left, area.Top), new Point(area.Right, area.Top), borderColor); // top
				spriteBatch.Draw(basicTexture, new Rectangle(area.X, area.Y, area.Width, thickness), null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, drawLayer);
			}
			if (mask[1]) {
				//DrawLine(spriteBatch, new Point(area.Right, area.Top), new Point(area.Right, area.Bottom), borderColor); // right
				spriteBatch.Draw(basicTexture, new Rectangle((area.Right - thickness), area.Y, thickness, area.Height), null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, drawLayer);
			}
			if (mask[2]) {
				//DrawLine(spriteBatch, new Point(area.Left, area.Bottom), new Point(area.Right, area.Bottom), borderColor); // bottom
				spriteBatch.Draw(basicTexture, new Rectangle(area.X, area.Bottom - thickness, area.Width, thickness), null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, drawLayer);
			}
			if (mask[3]) {
				//DrawLine(spriteBatch, new Point(area.Left, area.Top), new Point(area.Left, area.Bottom), borderColor); // left
				spriteBatch.Draw(basicTexture, new Rectangle(area.X, area.Y, thickness, area.Height), null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, drawLayer);
			}

		}
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Managers {
	public class FPSManager {

		float fps = 0.0f;
		long memoryUsed = 0;

		public void Update(float deltaTime, float gameSpeed) {
			fps = 1 / (deltaTime / gameSpeed);
			memoryUsed = GC.GetTotalMemory(false) / 1000;
		}

		public void Draw(SpriteBatch spriteBatch, Viewport viewport, long updateMs, long drawMs, bool isSlow) {
			string fpsString = String.Format("{0} FPS", (int)Math.Round(fps));
			string memoryUsage = String.Format("{0} kb", memoryUsed);
			string updateTime = String.Format("{0} ms", updateMs);
			string drawTime = String.Format("{0} ms", drawMs);

			spriteBatch.Begin();

			DrawString(spriteBatch, viewport, fpsString, 10);
			DrawString(spriteBatch, viewport, memoryUsage, 30);
			DrawString(spriteBatch, viewport, updateTime, 50);
			DrawString(spriteBatch, viewport, drawTime, 70);
			if (isSlow) {
				DrawString(spriteBatch, viewport, "LAG", 90, Color.Red);
			}

			spriteBatch.End();
		}

		private void DrawString(SpriteBatch spriteBatch, Viewport viewport, String stringValue, int y, Color? textColor = null) {
			SpriteFont font = AssetManager.GetDefault<SpriteFont>();
			Vector2 contentSize = font.MeasureString(stringValue);
			spriteBatch.DrawString(font, stringValue, new Vector2(viewport.Width - contentSize.X - 10, y), textColor ?? Color.Black);
		}
	}
}

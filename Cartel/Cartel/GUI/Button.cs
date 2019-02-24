using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Cartel.Managers;

namespace Cartel.GUI {
	public class Button: Control {
		string stringValue;
		public int contentPadding = 4;

		public override Texture2D Texture {
			get { return AssetManager.GetAsset<Texture2D>("gui_button"); }
		}

		public Button(Rectangle frameRectangle, string stringValue) : base(frameRectangle) {
			this.stringValue = stringValue;
		}

		public override void Draw(SpriteBatch spriteBatch, int x, int y) {
			Color tintColor = Color.White;
			if (isClicking) {
				tintColor = Color.Gray;
			} else if (isActive) {
				tintColor = Color.Red;
			} else if (isHovering) {
				tintColor = Color.LightGray;
			}

			// Left side
			float flankRatio = 7.0f / 50.0f;
			int flankWidth = (int)(flankRatio * (float)Frame.Height);
			spriteBatch.Draw(Texture, new Rectangle(x, y, flankWidth, Frame.Height + (2 * contentPadding)), new Rectangle(0, 0, 7, 50), tintColor);

			// Content
			SpriteFont font = AssetManager.GetDefault<SpriteFont>();
			Vector2 contentSize = font.MeasureString(stringValue);
			int contentWidth = Frame.Width - (2 * flankWidth) - (2 * contentPadding);
			int contentHeight = (int)contentSize.Y;
			spriteBatch.Draw(Texture, new Rectangle(x + flankWidth, y, contentWidth + (2 * contentPadding), Frame.Height + (2 * contentPadding)), new Rectangle(8, 0, 34, 50), tintColor);


			// Right side
			spriteBatch.Draw(Texture, new Rectangle(x + flankWidth + contentWidth + (2 * contentPadding), y, flankWidth, Frame.Height + (2 * contentPadding)), new Rectangle(44, 0, 7, 50), tintColor);

			// Text
			spriteBatch.DrawString(font, stringValue, new Vector2(x + flankWidth + contentPadding + (Frame.Width / 2) - (contentWidth / 2), y + contentPadding + (Frame.Height / 2) - (contentHeight / 2)), Color.Black, 0.0f, new Vector2(0), 1.0f, SpriteEffects.None, DrawLayer.GUI);

		}

		public override void UpdateControlState(ControlState state) {
			if(state == ControlState.JUST_RELEASED) { // Clicked button
				
			}
		}
	}
}

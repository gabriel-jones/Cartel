using Cartel.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.GUI {
	public abstract class Control: Interfaces.IDrawable {
		// Enums
		public enum ControlState {
			PRESSED, JUST_RELEASED, HOVER, NONE
		}

		// Fields
		public GUIManager guiManager;
		public Action<Control> handler;

		Rectangle frame;
		private bool wasClicking = false;
		ControlState state;
		Dictionary<string, object> userInfo;

		protected bool isHovering = false;
		protected bool isClicking = false;
		protected bool isActive = false;

		// Properties
		public Rectangle Frame {
			get { return frame; }
		}

		public int X {
			get { return frame.X; }
		}

		public int Y {
			get { return frame.Y; }
		}

		public ControlState State {
			get { return state; }
		}

		public bool IsActive {
			get { return isActive; }
		}

		public Dictionary<string, object> UserInfo {
			get { return userInfo; }
		}

		public abstract Texture2D Texture { get; }

		// Constructor
		public Control(Rectangle frameRectangle) {
			frame = frameRectangle;
			userInfo = new Dictionary<string, object>();
		}

		// Methods
		public void SetUserInfo(string key, object value) {
			userInfo[key] = value;
		}

		public void SetActive(bool isActive) {
			this.isActive = isActive;
		}

		public void Update() {
			MouseState mouse = Mouse.GetState();
			bool mouseOver = frame.Contains(new Point(mouse.X, mouse.Y));
			bool isClicking = mouse.LeftButton == ButtonState.Pressed;

			this.isClicking = false;
			this.isHovering = false;

			if (mouseOver && isClicking) {
				this.state = ControlState.PRESSED;
				this.isClicking = true;
			} else if (mouseOver && !isClicking && wasClicking) {
				this.state = ControlState.JUST_RELEASED;
				this.isClicking = false;
				handler?.Invoke(this);
			} else if (mouseOver) {
				this.state = ControlState.HOVER;
				this.isHovering = true;
			} else {
				this.state = ControlState.NONE;
			}

			UpdateControlState(state);

			wasClicking = isClicking;

			if(state == ControlState.PRESSED && guiManager != null) {
				guiManager.isHandled = true;
			}
		}

		// Abstract Methods
		public abstract void Draw(SpriteBatch spriteBatch, int x, int y);
		public abstract void UpdateControlState(ControlState state);
	}
}

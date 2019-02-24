using Cartel.GUI;
using Cartel.GUI.Tasks;
using Cartel.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cartel.Managers {
	public class GUIManager {
		// Fields
		SpriteBatch spriteBatch;
		List<Control> controls;
		public bool isHandled = false;
		World world;

		InputController inputController;

		KeyboardState previous;

		// Properties
		public SpriteBatch SpriteBatch {
			get {
				return spriteBatch;
			}
		}

		public List<Control> Controls {
			get {
				return controls;
			}
		}

		// Constructor
		public GUIManager(World world, ViewportManager viewportManager, GraphicsDevice graphicsDevice) {
			spriteBatch = new SpriteBatch(graphicsDevice);
			controls = new List<Control>();
			this.world = world;

			inputController = new InputController(world, viewportManager, graphicsDevice);

			AddControl(new Button(new Rectangle(10, 10 + controls.Count * (10 + 30), 150, 30), "BRK"), (control, manager) => {
				System.Console.WriteLine("BREAKPOINT");
			});

			for (int i = 0; i < 5; i++) {
				Button speedButton = new Button(new Rectangle(10 + (i * (10 + 50)), 10 + (controls.Count - i) * (10 + 30), 50, 30), (i == 0 ? "*" : i + "x"));
				speedButton.SetUserInfo("targetSpeed", (float)i);
				if (i == 1) {
					speedButton.SetActive(true);
				}
				AddControl(speedButton, (button, manager) => {
					world.SetGameSpeed((float)button.UserInfo["targetSpeed"]);
					manager.Controls.ForEach((ctr) => {
						Button b = (Button)ctr;
						if (b != null && b != button && b.UserInfo.ContainsKey("targetSpeed")) {
							b.SetActive(false);
						}
					});
					button.SetActive(true);
				});
			}
		}

		// Methods
		private void AddControl(Control control, Action<Control, GUIManager> handler) {
			control.guiManager = this;
			control.handler = (ctr) => {
				handler(ctr, this);
			};
			controls.Add(control);
		}

		public void AddTask(String name, InputTask task) {
			Button taskButton = new Button(new Rectangle(10, 10 + controls.Count * (10 + 30), 150, 30), name);
			taskButton.SetUserInfo("task", true);
			AddControl(taskButton, (control, manager) => {
				Button button = (Button)control;
				if (button == null) {
					return;
				}
				manager.Controls.ForEach((ctr) => {
					Button b = (Button)ctr;
					if (b != null && b != button && b.UserInfo.ContainsKey("task")) {
						b.SetActive(false);
					}
				});
				button.SetActive(!button.IsActive);
				inputController.SetTask(button.IsActive ? task : null);
			});
		}

		public void Update() {
			isHandled = false;
			foreach (Control control in controls) {
				control.Update();
			}
			UpdateInputControllers();

			KeyboardState current = Keyboard.GetState();
			if (current.IsKeyDown(Keys.Escape)) {
				inputController.SetTask(null);
				controls.ForEach(c => c.SetActive(false));
			}

			if (previous != null) {
				if (current.IsKeyUp(Keys.Space) && previous.IsKeyDown(Keys.Space)) {
					world.SetGameSpeed(world.GameSpeed == 0 ? 1 : 0);
					List<Control> speedControls = controls.Where(c => c.UserInfo.ContainsKey("targetSpeed")).ToList();
					speedControls.ForEach((ctr) => {
						ctr.SetActive(false);
					});
					speedControls[world.GameSpeed == 0 ? 0 : 1].SetActive(true);
				}
			}

			previous = current;
		}

		private void UpdateInputControllers() {
			inputController.isHandled = isHandled;

			inputController.Update();
		}

		public void Draw() {
			inputController.Draw();

			spriteBatch.Begin();

			foreach (Control control in controls) {
				control.Draw(spriteBatch, control.X, control.Y);
			}

			spriteBatch.DrawString(AssetManager.GetDefault<SpriteFont>(), TimeHelper.DayProgressToTime(world.DayProgress), new Vector2(10, 10 + controls.Count * (10 + 30)), Color.Black);

			spriteBatch.End();
		}
	}
}

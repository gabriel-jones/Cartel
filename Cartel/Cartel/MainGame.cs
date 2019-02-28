using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Cartel.Models;
using Cartel.Managers;
using Cartel.GUI.Tasks;
using Cartel.Models.Zones;

namespace Cartel {
	public class MainGame : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager graphics;

        World world;
        ViewportManager viewportManager;
		WorldRenderer worldRenderer;
		JobManager jobManager;
		GUIManager guiManager;
		FPSManager fpsManager;

		long updateTime = 0;
		long drawTime = 0;

        public MainGame() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            base.Initialize();

			IsMouseVisible = true;
			Window.Title = "Cartel Game";
			
			graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
			graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
			//graphics.IsFullScreen = true;
			graphics.ApplyChanges();

			var control = System.Windows.Forms.Control.FromHandle(Window.Handle);
			var form = control.FindForm();
			form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			form.WindowState = System.Windows.Forms.FormWindowState.Maximized;

			world = new World(100, 100);
			jobManager = new JobManager(world);
			world.SetJobManager(jobManager);
			viewportManager = new ViewportManager(GraphicsDevice.Viewport, world);
			worldRenderer = new WorldRenderer(world, viewportManager, GraphicsDevice);
			guiManager = new GUIManager(world, viewportManager, GraphicsDevice);
			fpsManager = new FPSManager();

			guiManager.AddTask("Build Walls", InputTask.BlueprintFactory(InputMode.Line, cell => new Wall(cell)));
			guiManager.AddTask("Build Floors", InputTask.BlueprintFactory(InputMode.Area, cell => new Floor(cell, FloorType.Concrete)));
			guiManager.AddTask("Build Light", InputTask.BlueprintFactory(InputMode.Single, cell => new Light(cell)));
			guiManager.AddTask("Spawn Worker", InputTask.PawnFactory(PawnType.Worker));
			guiManager.AddTask("Spawn Gardener", InputTask.PawnFactory(PawnType.Gardener));
			guiManager.AddTask("Add Concrete", InputTask.SoftObjectFactory(SoftObjectType.Concrete, 45));
			guiManager.AddTask("Create GrowZone", InputTask.ZoneFactory(() => {
				return new GrowZone(PlantType.Indica);
			}));
			ObjectInfo benchInfo = new ObjectInfo(ObjectType.Bench, new Point(2, 1));
			guiManager.AddTask("Create HardObject", InputTask.BlueprintFactory(InputMode.Single, cell => new HardObject(benchInfo, cell)));
			guiManager.AddTask("Bulldoze", InputTask.BulldozeFactory());
		}

        protected override void LoadContent() {
			Console.WriteLine("Loading content...");
			AssetManager.SetContentManager(Content);
		}

        protected override void UnloadContent() {
			
        }

        protected override void Update(GameTime gameTime) {
			var watch = System.Diagnostics.Stopwatch.StartNew();
			watch.Start();

			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) {
				this.Exit();
			}

			float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
			
			deltaTime *= world.GameSpeed;

			viewportManager.Update();
			guiManager.Update();
			world.Update(deltaTime);
			fpsManager.Update(deltaTime, world.GameSpeed);

			watch.Stop();
			updateTime = watch.ElapsedMilliseconds;

			base.Update(gameTime);
		}

        protected override void Draw(GameTime gameTime) {
			var watch = System.Diagnostics.Stopwatch.StartNew();
			watch.Start();

			worldRenderer.Draw();
			guiManager.Draw();
			fpsManager.Draw(guiManager.SpriteBatch, GraphicsDevice.Viewport, updateTime, drawTime, gameTime.IsRunningSlowly);

			watch.Stop();
			drawTime = watch.ElapsedMilliseconds;

			base.Draw(gameTime);
        }
    }
}

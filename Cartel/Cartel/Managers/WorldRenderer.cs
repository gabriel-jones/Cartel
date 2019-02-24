using Cartel.Models;
using Cartel.Models.Zones;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Managers {
	class WorldRenderer {
		public enum ViewMode {
			Default, Rooms, Utilities
		}

		World world;
		SpriteBatch spriteBatch;
		ViewportManager viewportManager;

		ViewMode currentView = ViewMode.Default;

		RenderTarget2D lightsTarget;
		RenderTarget2D mainTarget;

		Texture2D lightTexture;
		Effect lightEffect;

		public WorldRenderer(World world, ViewportManager viewportManager, GraphicsDevice graphicsDevice) {
			this.world = world;
			this.viewportManager = viewportManager;
			spriteBatch = new SpriteBatch(graphicsDevice);
			ShapeManager.Setup(graphicsDevice);

			lightTexture = AssetManager.GetAsset<Texture2D>("lightmask");
			lightEffect = AssetManager.GetAsset<Effect>("fx_world");

			lightsTarget = new RenderTarget2D(spriteBatch.GraphicsDevice, viewportManager.Viewport.Width, viewportManager.Viewport.Height);
			mainTarget = new RenderTarget2D(spriteBatch.GraphicsDevice, viewportManager.Viewport.Width, viewportManager.Viewport.Height);
		}

		public void SetViewMode(ViewMode mode) {
			currentView = mode;
		}

		public void Draw() {
			// Get viewport space
			Vector2 originCoordinates = viewportManager.GetWorldCoordinates(new Vector2(0, 0));
			Vector2 boundsCoordinates = viewportManager.GetWorldCoordinates(new Vector2(viewportManager.Viewport.Width, viewportManager.Viewport.Height));

			Vector2 originWorldSpace = world.WorldCoordsToCellPosition(originCoordinates);
			Vector2 boundsWorldSpace = world.WorldCoordsToCellPosition(boundsCoordinates);
			
			
			// Draw lighting
			spriteBatch.GraphicsDevice.SetRenderTarget(lightsTarget);
			spriteBatch.GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, viewportManager.Camera.Transform);

			if (world.DayProgress < 0.3 || world.DayProgress > 0.8) {
				Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				Vector2 mouseWorldPosition = viewportManager.GetWorldCoordinates(mousePosition);
				spriteBatch.Draw(lightTexture, new Rectangle((int)mouseWorldPosition.X - 256, (int)mouseWorldPosition.Y - 256, 512, 512), Color.White);
			}

			spriteBatch.End();
			
			// Draw world
			spriteBatch.GraphicsDevice.SetRenderTarget(mainTarget);
			spriteBatch.GraphicsDevice.Clear(Color.Gray);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, viewportManager.Camera.Transform);

			for (int x = (int)originWorldSpace.X; x <= (int)boundsWorldSpace.X; x++) {
				for (int y = (int)originWorldSpace.Y; y <= (int)boundsWorldSpace.Y; y++) {
					Cell cell = world.GetCellAt(x, y);
					if (cell != null) {
						cell.Tile.Draw(spriteBatch, x, y);
					}
				}
			}

			foreach (Zone zone in world.Zones) {
				zone.Draw(spriteBatch, currentView == ViewMode.Rooms ? 1.0f : 0.25f);
			}

			for (int x = (int)originWorldSpace.X; x <= (int)boundsWorldSpace.X; x++) {
				for (int y = (int)originWorldSpace.Y; y <= (int)boundsWorldSpace.Y; y++) {
					Cell cell = world.GetCellAt(x, y);
					if (cell != null) {
						if (cell.Floor != null) {
							cell.Floor.Draw(spriteBatch, x, y);
						}
						if (cell.Structure != null) {
							cell.Structure.Draw(spriteBatch, x, y);
						}

						if (cell.StructureBlueprint != null) {
							cell.StructureBlueprint.Draw(spriteBatch, x, y);
						} else if (cell.FloorBlueprint != null) {
							cell.FloorBlueprint.Draw(spriteBatch, x, y);
						}

						if (cell.SoftObject != null) {
							cell.SoftObject.Draw(spriteBatch, x, y);
						}
					}
				}
			}

			spriteBatch.End();


			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, viewportManager.Camera.Transform);

			foreach (Pawn pawn in world.Pawns) {
				pawn.Draw(spriteBatch, (int)(pawn.X * World.BlockSize), (int)(pawn.Y * World.BlockSize));
			}

			spriteBatch.End();
			
			// Combine with lighting
			lightEffect.Parameters["lightMask"].SetValue(lightsTarget);
			lightEffect.Parameters["lightIntensity"].SetValue(world.LightIntensity);

			spriteBatch.GraphicsDevice.SetRenderTarget(null);
			spriteBatch.GraphicsDevice.Clear(Color.Gray);

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, lightEffect);

			spriteBatch.Draw(mainTarget, new Vector2(0), Color.White);

			spriteBatch.End();
		}

	}
}

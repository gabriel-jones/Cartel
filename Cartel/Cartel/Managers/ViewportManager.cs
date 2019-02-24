using Cartel.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Cartel.Managers {
    public class ViewportManager {
        // Fields
        World world;
		Viewport viewport;
        Camera camera;
        Vector2 cameraPosition;

        float cameraSpeed = 5.0f;
        bool isDragging = false;
		Vector2 startOffset;
		float cameraZoom = 1.0f;
		float targetZoom = 1.0f;
		bool targetZoomIn;

		int previousScrollWheelValue;

		// Properties
		public Camera Camera {
			get { return camera; }
		}

		public Vector2 CameraPosition {
			get { return cameraPosition; }
		}

		public Viewport Viewport {
			get {
				return viewport;
			}
		}

        // Constructor
        public ViewportManager(Viewport viewport, World world) {
            camera = new Camera(viewport);
            this.world = world;
			this.viewport = viewport;
        }

        // Methods
        public void Update() {
            UpdateCameraPosition(Keyboard.GetState(), Mouse.GetState());
            camera.Update(cameraPosition, cameraZoom, world.Width, world.Height);
        }

        public void UpdateCameraPosition(KeyboardState keyboard, MouseState mouse) {
			// Panning with keyboard speed
			float speed = cameraSpeed / cameraZoom;

            if (keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift)) {
                speed *= 2.0f;
            }

			// Panning with keyboard 
            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) {
                cameraPosition.X += speed;
            } else if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) {
                cameraPosition.X -= speed;
            }

            if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) {
                cameraPosition.Y += speed;
            } else if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) {
                cameraPosition.Y -= speed;
            }

			// Panning with mouse 
			// FIXME: startOffset getting messed up, dragging not working
			/*
			if (mouse.MiddleButton == ButtonState.Pressed && !isDragging) {
                isDragging = true;
				startOffset = new Vector2(cameraPosition.X + mouse.X, cameraPosition.Y + mouse.Y);
            }

			if (isDragging) {
				if (mouse.MiddleButton == ButtonState.Released) {
					isDragging = false;
					startOffset = cameraPosition;
				}
				cameraPosition.X = (startOffset.X - mouse.X) / cameraZoom;
				cameraPosition.Y = (startOffset.Y - mouse.Y) / cameraZoom;
			}
			*/

			// SMALLER cameraZoom means ZOOMED OUT
			// LARGER cameraZoom means ZOOMED IN

			// Zooming with mouse
			if(mouse.ScrollWheelValue < previousScrollWheelValue) {
				targetZoom -= 0.1f;
				targetZoomIn = false;
			} else if (mouse.ScrollWheelValue > previousScrollWheelValue) {
				targetZoom += 0.1f;
				targetZoomIn = true;
			}

			previousScrollWheelValue = mouse.ScrollWheelValue;

			// Zooming with keyboard
			if(keyboard.IsKeyDown(Keys.PageDown)) {
				cameraZoom -= 0.05f * cameraZoom;
				targetZoom = cameraZoom;
			} else if(keyboard.IsKeyDown(Keys.PageUp)) {
				cameraZoom += 0.05f * cameraZoom;
				targetZoom = cameraZoom;
			}

			// Smooth zoom to target
			if (cameraZoom < targetZoom && targetZoomIn) {
				cameraZoom += 0.05f;
			} else if (cameraZoom > targetZoom && !targetZoomIn) {
				cameraZoom -= 0.05f;
			}

			// Clamp Values
			cameraZoom = MathHelper.Clamp(cameraZoom, 0.1f, 1.0f);
			targetZoom = MathHelper.Clamp(targetZoom, 0.1f, 1.0f);
			cameraPosition.X = MathHelper.Clamp(cameraPosition.X, 0.0f, world.Width * World.BlockSize);
			cameraPosition.Y = MathHelper.Clamp(cameraPosition.Y, 0.0f, world.Height * World.BlockSize);
		}

		public Vector2 GetWorldCoordinates(Vector2 screenCoordinates) {
			return Vector2.Transform(screenCoordinates, Matrix.Invert(Camera.Transform));
		}
	}
}

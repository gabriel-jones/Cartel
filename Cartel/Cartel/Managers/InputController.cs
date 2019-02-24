using Cartel.GUI.Tasks;
using Cartel.Interfaces;
using Cartel.Models;
using Cartel.Models.Jobs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Cartel.Managers {
	public class InputController {
		// Fields
		public bool isHandled = false;

		World world;
		ViewportManager viewportManager;
		SpriteBatch spriteBatch;

		InputTask currentTask;

		KeyboardState previousKeyboard;
		KeyboardState currentKeyboard;

		MouseState previousMouse;
		MouseState currentMouse;

		Vector2 mousePosition;
		Vector2 mouseWorldPosition;
		Vector2 mouseWorldSpace;
		Cell selectedCell;

		bool justReleasedMouse = false;

		bool isDragging = false;
		Vector2 startDrag;
		Rectangle dragArea;
		Rectangle dragAreaCells;

		bool isDraggingRight = false;
		Vector2 startDragRight;
		Rectangle dragAreaRight;
		Rectangle dragAreaCellsRight;

		List<ISelectable> selectedObjects;
		List<ISelectable> targetsSelected;

		// Properties
		public bool IsDragging {
			get { return isDragging; }
		}

		public InputTask Task {
			get { return currentTask; }
		}

		public Cell CellUnderMouse {
			get { return selectedCell; }
		}

		// Constructor
		public InputController(World world, ViewportManager viewportManager, GraphicsDevice graphicsDevice) {
			this.world = world;
			this.viewportManager = viewportManager;
			spriteBatch = new SpriteBatch(graphicsDevice);

			selectedObjects = new List<ISelectable>();
			targetsSelected = new List<ISelectable>();
		}

		// Methods
		public void SetTask(InputTask newTask) {
			currentTask = newTask;
		}

		public void Update() {
			if (isHandled) {
				return;
			}

			currentMouse = Mouse.GetState();
			mousePosition = new Vector2(currentMouse.X, currentMouse.Y);
			mouseWorldPosition = viewportManager.GetWorldCoordinates(mousePosition);
			mouseWorldSpace = world.WorldCoordsToCellPosition(mouseWorldPosition);
			selectedCell = world.GetCellAt(mouseWorldPosition);

			justReleasedMouse = currentMouse.LeftButton == ButtonState.Released && previousMouse != null && previousMouse.LeftButton == ButtonState.Pressed;

			UpdateDraggingLMB();
			UpdateDraggingRMB();

			UpdateSelection();

			currentKeyboard = Keyboard.GetState();

			if (currentKeyboard.IsKeyDown(Keys.Escape) && !isDragging) {
				currentTask = null;
				dragArea = Rectangle.Empty;
				dragAreaCells = Rectangle.Empty;
			}

			if (previousKeyboard != null) {
				// on key release here
			}

			previousMouse = currentMouse;
			previousKeyboard = currentKeyboard;
		}

		void UpdateSelection() {
			if (dragArea == null || currentTask != null) {
				// Is the user inputting something (e.g. pawn) or are they not dragging?
				return;
			}

			// Reset all selected targets
			targetsSelected.ForEach((target) => {
				if (!selectedObjects.Contains(target)) {
					target.IsSelected = false;
				}
			});

			// If escape key is down, remove all selections
			if (currentKeyboard.IsKeyDown(Keys.Escape)) {
				selectedObjects.ForEach(t => t.IsSelected = false);
				selectedObjects = new List<ISelectable>();
				return;
			}

			// Find everything selected under the mouse and select it
			targetsSelected = FindSelectableTargets();
			targetsSelected.ForEach(t => t.IsSelected = true);

			// User did release their current selection
			if (justReleasedMouse) {
				// Unselect everything currently selected (unless user chaining selections)
				selectedObjects.ForEach(t => t.IsSelected = false);
				if (!currentKeyboard.IsKeyDown(Keys.LeftShift)) {
					selectedObjects = new List<ISelectable>();
				}

				for (int i = 0; i < targetsSelected.Count; i++) { // For every selected target,
					ISelectable target = targetsSelected[i];
					if (!selectedObjects.Contains(target)) { // Check if it's already selected.
						selectedObjects.Add(target); // If not, select it
						target.IsSelected = true;
						targetsSelected.Remove(target); // Remove it from selected targets - it doesn't need that anymore, it's properly selected
						i--;
					}
				}
			}
		}

		List<ISelectable> FindSelectableTargets() {
			List<ISelectable> targets = new List<ISelectable>();
			if (dragAreaCells.Width != 0 && dragAreaCells.Height != 0) { // If user has dragged over an area of cells
				for (int x = dragAreaCells.Left; x <= dragAreaCells.Right ; x++) {
					for (int y = dragAreaCells.Top; y <= dragAreaCells.Bottom; y++) {
						Cell cell = world.GetCellAt(x, y);
						ISelectable targetInCell = FindSelectableTarget(cell);
						if (targetInCell != null) {
							targets.Add(targetInCell);
						}
					}
				}
			} else { // User is just hovering over a single cell
				ISelectable target = FindSelectableTarget(selectedCell);
				if (target != null) {
					targets.Add(target);
				}
			}
			return targets;
		}

		ISelectable FindSelectableTarget(Cell cell) {
			if (cell == null) {
				return null;
			}
			foreach (Pawn pawn in world.Pawns) {
				// If mouse over pawn
				Rectangle pawnBounds = new Rectangle((int)pawn.X * World.BlockSize, (int)pawn.Y * World.BlockSize, World.BlockSize, World.BlockSize);
				if (pawnBounds.Contains(new Point((int)mouseWorldPosition.X, (int)mouseWorldPosition.Y))) {
					return pawn;
				}
			}

			if (cell.SoftObject != null) {
				return cell.SoftObject;
			}

			return null;
		}

		void UpdateDraggingLMB() {
			if (currentMouse.LeftButton == ButtonState.Pressed && !isDragging) {
				isDragging = true;
				startDrag = mouseWorldPosition;
			}

			if (currentMouse.LeftButton == ButtonState.Pressed && isDragging) {
				int startX = (int)startDrag.X;
				int endX = (int)mouseWorldPosition.X;
				if (startX > endX) {
					int temp = endX;
					endX = startX;
					startX = temp;
				}

				int startY = (int)(startDrag.Y);
				int endY = (int)(mouseWorldPosition.Y);
				if (startY > endY) {
					int temp = endY;
					endY = startY;
					startY = temp;
				}

				int anchorX = (int)startDrag.X;
				int anchorY = (int)startDrag.Y;

				if (currentTask != null && currentTask.Mode == InputMode.Line) {
					if (endX - startX > endY - startY) {
						startY = anchorY;
						endY = anchorY;
					} else {
						endX = anchorX;
						startX = anchorX;
					}
				} else if (currentTask != null && currentTask.Mode == InputMode.Single) {
					startX = anchorX;
					endX = anchorX;
					startY = anchorY;
					endY = anchorY;
				}

				dragArea = new Rectangle(startX, startY, endX - startX, endY - startY);
				dragAreaCells = world.WorldCoordsToCellPosition(dragArea);
			}

			if (currentMouse.LeftButton == ButtonState.Released && isDragging) {
				isDragging = false;
				if (currentTask != null) {
					List<Cell> selectedCells = new List<Cell>();
					for (int x = dragAreaCells.Left; x <= dragAreaCells.Right; x++) {
						for (int y = dragAreaCells.Top; y <= dragAreaCells.Bottom; y++) {
							Cell cell = world.GetCellAt(x, y);
							if (cell != null) {
								if (currentTask.Mode == InputMode.Box && dragAreaCells.Left != cell.X && dragAreaCells.Top != cell.Y && dragAreaCells.Right != cell.X && dragAreaCells.Bottom != cell.Y) {
									continue;
								}
								selectedCells.Add(cell);
							}
						}
					}
					currentTask.Action?.Invoke(selectedCells);
				}
			}
		}

		void UpdateDraggingRMB() {
			if (currentMouse.RightButton == ButtonState.Pressed && !isDraggingRight) {
				isDraggingRight = true;
				startDragRight = mouseWorldPosition;
			}

			if (currentMouse.RightButton == ButtonState.Pressed && isDraggingRight) {
				int startX = (int)startDragRight.X;
				int endX = (int)mouseWorldPosition.X;
				if (startX > endX) {
					int temp = endX;
					endX = startX;
					startX = temp;
				}

				int startY = (int)(startDragRight.Y);
				int endY = (int)(mouseWorldPosition.Y);
				if (startY > endY) {
					int temp = endY;
					endY = startY;
					startY = temp;
				}

				dragAreaRight = new Rectangle(startX, startY, endX - startX, endY - startY);
				dragAreaCellsRight = world.WorldCoordsToCellPosition(dragAreaRight);
			}

			if (currentMouse.RightButton == ButtonState.Released && isDraggingRight) {
				isDraggingRight = false;
				List<Cell> selectedCells = new List<Cell>();
				for (int x = dragAreaCellsRight.Left; x <= dragAreaCellsRight.Right; x++) {
					for (int y = dragAreaCellsRight.Top; y <= dragAreaCellsRight.Bottom; y++) {
						Cell cell = world.GetCellAt(x, y);
						if (cell != null) {
							selectedCells.Add(cell);
						}
					}
				}
				if (currentTask != null && currentTask.CancelAction != null) {
					currentTask.CancelAction(selectedCells);
				}
			}
		}

		public void Draw() {
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, viewportManager.Camera.Transform);

			if (currentTask != null && currentTask.Preview != null) {
				if (isDragging) {
					for (int x = dragAreaCells.Left; x <= dragAreaCells.Right; x++) {
						for (int y = dragAreaCells.Top; y <= dragAreaCells.Bottom; y++) {
							Cell cell = world.GetCellAt(x, y);
							if (cell != null) {
								currentTask.Preview(cell, spriteBatch);
							}
						}
					}
				} else if (CellUnderMouse != null) {
					currentTask.Preview(CellUnderMouse, spriteBatch);
				}
			}

			if (isDraggingRight) {
				ShapeManager.DrawRectangle(spriteBatch, dragAreaRight, Color.Transparent, Color.Red);
				for (int x = dragAreaCellsRight.Left; x <= dragAreaCellsRight.Right; x++) {
					for (int y = dragAreaCellsRight.Top; y <= dragAreaCellsRight.Bottom; y++) {
						Cell cell = world.GetCellAt(x, y);
						if (cell != null && (currentTask != null && currentTask.CanCancel != null && currentTask.CanCancel(cell))) {
							Blueprint preview = new Blueprint(cell.World, new Floor(cell, FloorType.Concrete));
							preview.Draw(spriteBatch, cell.X, cell.Y, Blueprint.BlueprintState.Invalid);
						}
					}
				}
			}

			spriteBatch.End();
		}
	}
}
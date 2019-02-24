using System;
using Microsoft.Xna.Framework.Graphics;
using Cartel.Managers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Cartel.Interfaces;
using System.Linq;

namespace Cartel.Models {
	public enum PawnType {
		Worker, Gardener
	}

	public class Pawn : Interfaces.IDrawable, ISelectable {
		public enum Facing {
			North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
		}

		// Fields
		PawnType type;
		World world;

		Job currentJob;
		float jobSearchCooldown = 0f;

		float movementPercentage = 0f;
		Cell currentCell;
		Cell nextCell;
		Cell destinationCell;

		Facing direction = Facing.South;

		PathfinderAStar pathfinder;
		float speed = 3.0f;
		bool isIdling = true;

		SoftObject carrying;

		// Properties
		public bool IsSelected { get; set; }

		public Texture2D Texture {
			get {
				switch (type) {
					case PawnType.Worker:
						return AssetManager.GetAsset<Texture2D>("pawn_worker");
					case PawnType.Gardener:
						return AssetManager.GetAsset<Texture2D>("pawn_worker"); //TODO: texture
				}
				return null;
			}
		}

		public PawnType Type {
			get { return type; }
		}

		public SoftObject Carrying {
			get { return carrying; }
			set { carrying = value; }
		}

		public float X {
			get {
				if (nextCell == null)
					return currentCell.X;
				return MathHelper.Lerp(currentCell.X, nextCell.X, movementPercentage);
			}
		}

		public float Y {
			get {
				if (nextCell == null)
					return currentCell.Y;
				return MathHelper.Lerp(currentCell.Y, nextCell.Y, movementPercentage);
			}
		}

		public Cell CurrentCell {
			get { return currentCell; }
			set {
				if (currentCell != null && currentCell.Pawns.Contains(this)) {
					currentCell.Pawns.Remove(this);
				}
				currentCell = value;
				if (!currentCell.Pawns.Contains(this)) {
					currentCell.Pawns.Add(this);
				}
			}
		}

		public Cell DestinationCell {
			get { return destinationCell; }
			set {
				destinationCell = value;
				pathfinder = null;
			}
		}

		public Cell NextCell {
			get { return nextCell; }
			set {
				nextCell = value;
				List<Cell> neighbors = currentCell.GetNeighbors(true); // N E S W SE NE NW SW
				int index = neighbors.IndexOf(nextCell);
				if (index != -1) {
					direction = new Facing[8] {
						// N E S W SE NE NW SW
						Facing.North, Facing.East, Facing.South, Facing.West, Facing.South, Facing.North, Facing.North, Facing.South
					}[index];
				}
			}
		}

		public bool IsIdling {
			get { return isIdling; }
		}

		public Facing Direction {
			get { return direction; }
		}

		// Constructor
		public Pawn(World world, PawnType type, Cell cell) {
			this.world = world;
			this.type = type;
			CurrentCell = cell;
			DestinationCell = cell;
			NextCell = cell;
		}

		// Methods
		public void Update(float deltaTime) {
			// didError = false;
			UpdateJob(deltaTime);
			UpdateMovement(deltaTime);
		}

		public void Draw(SpriteBatch spriteBatch, int x, int y) {
			Color color = IsIdling ? Color.Green : Color.Orange;

			spriteBatch.Draw(Texture,
				new Rectangle(x, y , World.BlockSize, World.BlockSize),
				new Rectangle(0, 0, Texture.Width, Texture.Height),
				color,
				0.0f,
				new Vector2(0),
				SpriteEffects.None,
				DrawLayer.Pawn
			);

			if (IsSelected) {
				ShapeManager.DrawCircle(spriteBatch, new Point(x + World.BlockSize / 2, y + World.BlockSize / 2), World.BlockSize / 2, Color.Red);
			}

			if (pathfinder != null && pathfinder.Path != null) {
				Cell previous = nextCell;

				foreach (Cell next in pathfinder.Path) {
					ShapeManager.DrawLine(spriteBatch,
						new Point(previous.X * World.BlockSize + World.BlockSize / 2, previous.Y * World.BlockSize + World.BlockSize / 2),
						new Point(next.X * World.BlockSize + World.BlockSize / 2, next.Y * World.BlockSize + World.BlockSize / 2),
						Color.Magenta
					);
					previous = next;
				}
			}
		}

		public void GoToCell(Cell target) {
			AbandonJob();
			isIdling = false;

			pathfinder = new PathfinderAStar(world, currentCell, target);
			if (pathfinder.Length != 0) {
				destinationCell = target;
			}
		}

		void UpdateJob(float deltaTime) {
			// Find Job
			if (currentJob == null && IsIdling) {
				jobSearchCooldown -= deltaTime;
				if (jobSearchCooldown > 0)
					return;

				FindJob();

				if (currentJob == null) {
					jobSearchCooldown = 0.5f;
					//destinationCell = currentCell;
					return;
				}
			}

			if (!IsIdling && currentJob == null) {
				return;
			}

			if (!currentJob.MeetsBuildRequirements()) {
				SoftObject next = currentJob.NextRequirement();
				if (carrying != null) { // We are carrying something
					if (carrying.Type == next.Type) { // We are carrying the thing that's needed
						if (currentCell == currentJob.cell || currentCell.GetNeighbors(false).Contains(currentJob.cell)) { // We are in the job cell, TODO: understand / fix everything in this branch not sure it works with count and stackCount for soft objects
							currentJob.DepositSoftObject(carrying);
							carrying.ReduceCount(next.Count);
							if (carrying.Count == 0) {
								carrying = null;
							} else {
								world.DropSoftObject(this, currentCell, Carrying.Count);
							}
						} else { // We aren't in the job cell, keep moving towards the destination
							destinationCell = currentJob.cell;
							return;
						}
					} else { // We don't have required inventory, drop current thing
						world.DropSoftObject(this, currentCell, Carrying.Count);
					}
				} else { // We aren't carrying anything
					if (currentCell.SoftObject != null && currentCell.SoftObject.Type == next.Type) { // We're standing on top of a cell with required inventory
						world.PickupSoftObject(this, currentCell, currentCell.SoftObject.Count);
					} else { // Need to walk to nearest required inventory
						if (currentCell != nextCell) { // We're moving somewhere rn so keep doing that
							return;
						}

						Cell lastCell = pathfinder == null ? null : pathfinder.LastCell();
						if (pathfinder == null || lastCell == null || lastCell.SoftObject == null || lastCell.SoftObject.Type != next.Type) { // we ain't going somewhere useful so recalculate 
							PathfinderAStar newPathfinder = world.PathForNearestSoftObject(next.Type, currentCell);
							if (newPathfinder == null || newPathfinder.Length == 0) {
								Console.WriteLine("Abandoning because pathfinder unsuccessful for soft object search");
								AbandonJob();
								return;
							}
							pathfinder = newPathfinder;
							destinationCell = pathfinder.LastCell();
							NextCell = pathfinder.Dequeue();
						}						
					}
				}
				return;
			}

			// Work Job
			// destinationCell = currentJob.cell;
			if (currentCell == currentJob.cell || currentCell.GetNeighbors(false).Contains(currentJob.cell)) {
				currentJob.Progress(deltaTime, this);
			}
		}

		void FindJob() {
			currentJob = world.JobManager.FindJobForPawn(this);
			if (currentJob == null) {
				Console.Write("no jobs found");
				return;
			}

			isIdling = false;
			currentJob.SetReserved(true);
			destinationCell = currentJob.cell;
			currentJob.RegisterJobCompletedCallback(JobFinished);

			pathfinder = FindPathToCurrentJob(true);
			if (pathfinder == null) {
				currentJob.SetReachable(false);
				AbandonJob();
			}
		}

		PathfinderAStar FindPathToCurrentJob(bool withNeighbors) {
			PathfinderAStar newPathfinder = new PathfinderAStar(world, currentCell, destinationCell); // May not be used if going to soft object first but need this for verifying that we can reach target area
			if (newPathfinder.Length == 0) {
				if (!withNeighbors) {
					return null;
				}
				foreach (Cell neighbor in destinationCell.GetNeighbors(false)) {
					newPathfinder = new PathfinderAStar(world, currentCell, neighbor);
					if (newPathfinder != null && newPathfinder.Length > 0) {
						return newPathfinder;
					}
				}
				return null;
			}
			return newPathfinder;
		}

		void JobFinished(Job job) {
			job.UnregisterJobCompletedCallback(JobFinished);
				
			if (job != currentJob) {
				Console.Error.WriteLine("Job finished for not currentJob: remember to unregister callbacks");
				return;
			}
			isIdling = true;
			currentJob = null;
		}

		void AbandonJob() {
			if (currentJob == null) {
				return;
			}
			isIdling = true;
			currentJob.SetReserved(false);
			currentJob.UnregisterJobCompletedCallback(JobFinished);
			NextCell = destinationCell = currentCell;
			currentJob = null;
			jobSearchCooldown = 0.5f;
		}

		float idleCooldown = 3f;

		void UpdateMovement(float deltaTime) {
			if (IsIdling && destinationCell == currentCell) {
				if (idleCooldown <= 0) {
					Cell randomCell = null;
					Random rand = new Random();
					while (randomCell == null) {
						int x = currentCell.X;
						int y = currentCell.Y;
						x += rand.Next(-2, 2);
						y += rand.Next(-2, 2);
						randomCell = world.GetCellAt(x, y);
					}
					pathfinder = new PathfinderAStar(world, currentCell, randomCell);
					destinationCell = pathfinder.LastCell();
					idleCooldown = 3f;
				} else {
					idleCooldown -= deltaTime;
				}
				return;
			}

			// If pawn is at destination, remove pathfinder
			if (currentCell == destinationCell) {
				pathfinder = null;
				isIdling = currentJob == null;
				return;
			}

			// Calculate next cell to go to, if there isn't one already in progress
			if (nextCell == null || nextCell == currentCell) {
				if (pathfinder == null || pathfinder.Length == 0) {
					bool withNeighbors = currentJob != null && currentJob.cell == destinationCell;
					pathfinder = FindPathToCurrentJob(withNeighbors);
					if (pathfinder == null) {
						if (!IsIdling) {
							AbandonJob();
						}
						return;
					}
				}
				NextCell = pathfinder.Dequeue();
			}

			// Increase movement to destination cell
			int dX = currentCell.X - nextCell.X;
			int dY = currentCell.Y - nextCell.Y;
			float travelDistance = (float)Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2));

			float frameDistance = (speed * (IsIdling ? 0.5f : 1f)) / nextCell.MovementCost * deltaTime;
			float framePercentage = frameDistance / travelDistance;
			movementPercentage += framePercentage;

			if (movementPercentage >= 1) {
				CurrentCell = nextCell;
				movementPercentage = 0f; // TODO: set to 0? test this out
			}
		}
	}
}

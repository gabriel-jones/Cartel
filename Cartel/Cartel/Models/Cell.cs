using Cartel.Models.Zones;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Cartel.Models {
	public class Cell  {

		// Fields
		Tile tile;
		Floor floor;
		Blueprint floorBlueprint;
		Structure structure;
		Blueprint structureBlueprint;
		SoftObject softObject;

		// Properties
		public World World { get; protected set; }
		public int X { get; protected set; }
		public int Y { get; protected set; }
		

		public Zone Zone { get; set; }
		public List<Pawn> Pawns { get; set; }

		public bool HasBlueprints {
			get {
				return floorBlueprint != null || structureBlueprint != null;
			}
		}

		public bool CanPlant {
			get {
				if (structure != null) {
					return false;
				}
				if (floor != null && !floor.CanGrow) {
					return false;
				}
				return true;
			}
		}

		public bool CanGrow {
			get {
				return World.DayProgress > 0.3 && World.DayProgress < 0.8;
			}
		}
		
		public Tile Tile {
			get { return tile; }
			set {
				if (tile != value) {
					cellChangedCallback(this);
				}
				tile = value;
			}
		}

		public Floor Floor {
			get { return floor; }
			set {
				if (floor != value) {
					cellChangedCallback(this);
				}
				floor = value;
			}
		}

		public Blueprint FloorBlueprint {
			get { return floorBlueprint; }
			set {
				if (floorBlueprint != value) {
					cellChangedCallback(this);
				}
				floorBlueprint = value;
			}
		}

		public Structure Structure {
			get { return structure; }
			set {
				if (structure != value) {
					cellChangedCallback(this);
				}
				structure = value;
				if (MovementCost == 0 && Zone != null) {
					if (Zone.Cells.Count == 0) {
						World.RemoveZone(Zone);
					}
					Zone.RemoveCell(this);
					Zone = null;
				}
			}
		}

		public Blueprint StructureBlueprint {
			get { return structureBlueprint; }
			set {
				if (structureBlueprint != value) {
					cellChangedCallback(this);
				}
				structureBlueprint = value;
			}
		}

		public SoftObject SoftObject {
			get { return softObject; }
			set {
				if (softObject != value) {
					cellChangedCallback(this);
				}
				softObject = value;
			}
		}

		public float MovementCost {
			get {
				float baseMovementCost = 1.0f;

				if (structure != null) {
					return structure.IsTraversible ? baseMovementCost * structure.MovementCost : 0f;
				}

				return baseMovementCost;
			}
		}

		// Constructor
		public Cell(World world, int x, int y) {
			World = world;
			X = x;
			Y = y;
			tile = new Tile(this, Tile.TileType.Dirt);
			Pawns = new List<Pawn>();
        }

		// Methods
		public override string ToString() {
			return string.Format("cell({0},{1})", X, Y);
		}

		public void Update(float deltaTime) {
			if (structure != null) {
				structure.Update(deltaTime);
			}
		}

		public void SetConstruct<T>(T construct) where T : Constructable {
			Type constructType = typeof(T);
			if (constructType == typeof(Structure)) {
				structure = (Structure)(object)construct;
			} else if (constructType == typeof(Floor)) {
				floor = (Floor)(object)construct;
			} else if (constructType == typeof(Tile)) {
				tile = (Tile)(object)construct;
			}
		}

		public T GetConstruct<T>() where T : Constructable {
			Type constructType = typeof(T);
			if (constructType == typeof(Structure)) {
				return (T)(object)structure;
			} else if (constructType == typeof(Floor)) {
				return (T)(object)floor;
			} else if (constructType == typeof(Tile)) {
				return (T)(object)tile;
			}
			return null;
		}

		public void CancelJobs() {
			foreach (Job job in World.JobManager.JobsForCell(this)) {
				job.Cancel();
				World.JobManager.RemoveJob(job);
			}
		}

		public List<Cell> GetNeighbors(bool includeDiagonal) {
			List<Cell> neighbors = new List<Cell>();
			// N E S W SE NE NW SW

			Cell neighbor;

			neighbor = World.GetCellAt(X, Y - 1);
			neighbors.Add(neighbor);

			neighbor = World.GetCellAt(X + 1, Y);
			neighbors.Add(neighbor);

			neighbor = World.GetCellAt(X, Y + 1);
			neighbors.Add(neighbor);

			neighbor = World.GetCellAt(X - 1, Y);
			neighbors.Add(neighbor);

			if (includeDiagonal) {
				neighbor = World.GetCellAt(X + 1, Y + 1);
				neighbors.Add(neighbor);

				neighbor = World.GetCellAt(X + 1, Y - 1);
				neighbors.Add(neighbor);

				neighbor = World.GetCellAt(X - 1, Y - 1);
				neighbors.Add(neighbor);

				neighbor = World.GetCellAt(X - 1, Y + 1);
				neighbors.Add(neighbor);
			}

			return neighbors;
		}

		public float DistanceToCell(Cell target) {
			float dX = Math.Abs(X - target.X);
			float dY = Math.Abs(Y - target.Y);
			return (float)Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2));
		}

		// Callbacks
		Action<Cell> cellChangedCallback;

		public void RegisterCellChangedCallback(Action<Cell> callback) {
			cellChangedCallback += callback;
		}

		public void UnregisterCellChangedCallback(Action<Cell> callback) {
			cellChangedCallback -= callback;
		}
	}
}
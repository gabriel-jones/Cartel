using Cartel.Pathfinder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cartel.Managers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Cartel.Models.Zones;

namespace Cartel.Models {
	public class World {
		// Fields
		Cell[,] cells;
		List<Pawn> pawns;
		List<Zone> zones;
		int width, height;

		JobManager jobManager;
		public PathfinderCellGraph cellGraph;

		float gameSpeed = 1.0f;
		float dayTime;
		float dayCurrentTime;

		// Properties
		public int Width {
			get { return width; }
		}

		public int Height {
			get { return height; }
		}

		public static int BlockSize {
			get { return 64; }
		}

		public float GameSpeed {
			get { return gameSpeed; }
		}

		public List<Pawn> Pawns {
			get { return pawns; }
		}

		public List<Zone> Zones {
			get { return zones; }
		}

		public JobManager JobManager {
			get { return jobManager; }
		}

		public float DayProgress { // 0 = midnight, 0.5 = noon, 1.0 = midnight
			get { return dayCurrentTime / dayTime; }
		}

		public float LightIntensity {
			get {
				// I don't fuckin know lol
				float dayValue = (float)Math.Sin((DayProgress / 1.4f) * (float)Math.PI * 2f + 5f); 
				return Math.Min(Math.Max(dayValue, 0.1f), 1.0f);
			}
		}

		protected Predicate<Cell> ValidSoftObjectPlacementPredicate(SoftObject target) {
			return (cell) => {
				SoftObject placement = cell.SoftObject;
				if (placement == null) {
					return true;
				}
				if (placement != null && target != null && target.Type == placement.Type && placement.Count < placement.StackCount) {
					return true;
				}
				return false;
			};
		}

		// Constructor
		public World(int width, int height) {
			this.width = width;
			this.height = height;
			cells = new Cell[width, height];
			pawns = new List<Pawn>();
			zones = new List<Zone>();

			dayTime = TimeHelper.RealSecondsPerGameDay;
			dayCurrentTime = TimeHelper.GameTimeToReal(TimeUnit.Hour, 9);

			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					bool flag = (y + x) % 2 == 0;
					Cell cell = new Cell(this, x, y);
					cell.RegisterCellChangedCallback(CellChanged);
					cell.Tile = new Tile(cell, flag ? Tile.TileType.Grass : Tile.TileType.Sand);
					cells[x, y] = cell;
				}
			}
		}

		// Methods
		public void Update(float deltaTime) {
			dayCurrentTime += deltaTime;
			if (dayCurrentTime > dayTime) {
				dayCurrentTime = 0.0f;
			}

			foreach (Cell cell in cells) {
				cell.Update(deltaTime);
			}

			foreach (Pawn pawn in pawns) {
				pawn.Update(deltaTime);
			}

			foreach (Zone zone in zones) {
				zone.Update(deltaTime);
			}
		}

		public void SetGameSpeed(float speed) {
			gameSpeed = speed;
		}

		public void SetJobManager(JobManager jobManager) {
			this.jobManager = jobManager;
		}

		public void AddZone(Zone newZone) {
			zones.Add(newZone);
		}

		public void RemoveZone(Zone target) {
			zones.Remove(target);
		}

		public void InvalidatePathfindingGraph() {
			cellGraph = null;
			if (jobManager != null) {
				jobManager.RemovePathfindingFailures();
			}
		}

		void CellChanged(Cell cell) {
			// InvalidatePathfindingGraph();
			if (cellGraph != null) {
				cellGraph.UpdateCell(cell);
			}
			if (jobManager != null) {
				jobManager.RemovePathfindingFailures();
			}
		}

		public Vector2 WorldCoordsToCellPosition(Vector2 worldCoordinates) {
			return new Vector2((int)((float)worldCoordinates.X / (float)BlockSize),
				(int)((float)worldCoordinates.Y / (float)BlockSize)
			);
		}

		public Rectangle WorldCoordsToCellPosition(Rectangle worldArea) {
			int startX = (int)Math.Floor((float)worldArea.Left / (float)BlockSize);
			int startY = (int)Math.Floor((float)worldArea.Top / (float)BlockSize);
			int endX = (int)Math.Floor((float)worldArea.Right / (float)BlockSize);
			int endY = (int)Math.Floor((float)worldArea.Bottom / (float)BlockSize);

			return new Rectangle(startX, startY, endX - startX, endY - startY);
		}

		public Cell GetCellAt(int x, int y) {
            if (x >= width || x < 0 || y >= height || y < 0) {
                return null;
            }
            return cells[x, y];
        }

		public Cell GetCellAt(Vector2 worldCoordinate) {
			if (worldCoordinate.X >= width * BlockSize || worldCoordinate.X < 0 || worldCoordinate.Y >= height * BlockSize || worldCoordinate.Y < 0) {
				return null;
			}

			int x = (int)(worldCoordinate.X / (float)BlockSize);
			int y = (int)(worldCoordinate.Y / (float)BlockSize);

			if (x >= width || x < 0 || y >= height || y < 0) {
				return null;
			}
			return cells[x, y];
		}

		public bool ValidatePlacement(int x, int y, Blueprint blueprint) {
			Cell cell = GetCellAt(x, y);
			if (cell == null) {
				return false;
			}

			if (blueprint.Construct is Structure && (cell.Structure != null || cell.StructureBlueprint != null)) {
				return false;
			} else if (blueprint.Construct is Floor && (cell.Floor != null || cell.FloorBlueprint != null)) {
				return false;
			}

			return true;
		}

		public void AddPawn(Pawn pawn) {
			pawns.Add(pawn);
		}

		// fuck this method
		public Cell NearestCell(Cell startCell, Predicate<Cell> predicate) {
			Cell[] matches = cells.Cast<Cell>().Where(c => predicate(c)).ToArray();
			if (matches.Length == 0) {
				return null;
			}
			return matches.OrderBy(c => c.DistanceToCell(startCell)).FirstOrDefault();
		}

		public void DropSoftObject(Pawn pawn, Cell target, int amount) {
			SpawnSoftObject(pawn.Carrying, target, amount);
			pawn.Carrying = null;
		}

		public void SpawnSoftObject(SoftObject softObject, Cell target, int amount) {
			if (softObject == null) {
				return;
			}

			List<SoftObject> split = softObject.Split();

			for (int i = 0; i < split.Count; i++) {
				SoftObject so = split[i];
				Cell nextCell = NearestCell(target, ValidSoftObjectPlacementPredicate(so));

				if (nextCell.SoftObject != null) {
					int remainder = nextCell.SoftObject.IncreaseCount(so.Count, true);
					if (remainder > 0) {
						split.Add(so.Clone(remainder));
					}
				} else {
					nextCell.SoftObject = so;
				}

			}
		}

		public void PickupSoftObject(Pawn pawn, Cell target, int amount) {
			if (target.SoftObject == null || pawn.Carrying != null) {
				return;
			}

			// Transfer correct amount
			pawn.Carrying = target.SoftObject.Clone();
			target.SoftObject.ReduceCount(amount);
			int remainder = target.SoftObject.Count;
			pawn.Carrying.ReduceCount(remainder);

			// Remove from cell if pawn picked up all available
			if (target.SoftObject.Count == 0) {
				target.SoftObject = null;
			}
		}

		public PathfinderAStar PathForNearestSoftObject(SoftObjectType type, Cell startCell) {
			Cell endCell = NearestCell(startCell, (cell) => {
				return cell.SoftObject != null && cell.SoftObject.Type == type;
			});
			if (endCell == null) {
				return null;
			}
			return new PathfinderAStar(this, startCell, endCell);
		}
	}
}

using Cartel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Pathfinder {
	public class PathfinderCellGraph {
		public Dictionary<Cell, PathfinderNode<Cell>> nodes;
		World world;

		public void UpdateCell(Cell cell, bool shouldUpdateNeighbors = true) {
			if (!nodes.ContainsKey(cell)) {
				return;
			}

			PathfinderNode<Cell> node = nodes[cell];

			List<PathfinderEdge<Cell>> edges = new List<PathfinderEdge<Cell>>();

			List<Cell> neighbors = cell.GetNeighbors(includeDiagonal: true);

			foreach (Cell neighbor in neighbors) {
				if (neighbor == null) {
					continue;
				}
				bool isTraversible = neighbor.MovementCost > 0 && !IsClippingCorner(cell, neighbor);
				if (isTraversible) {
					PathfinderEdge<Cell> edge = new PathfinderEdge<Cell>();
					edge.cost = neighbor.MovementCost;
					edge.node = nodes[neighbor];
					edges.Add(edge);
				}

				if (shouldUpdateNeighbors) {
					UpdateCell(neighbor, false);
				}
			}

			node.edges = edges.ToArray();
		}

		public PathfinderCellGraph(World world) {
			this.world = world;
			Console.WriteLine("Constructin world graph");

			// Construct nodes dictionary
			nodes = new Dictionary<Cell, PathfinderNode<Cell>>();

			for (int x = 0; x < world.Width; x++) {
				for (int y = 0; y < world.Height; y++) {
					Cell cell = world.GetCellAt(x, y);
					PathfinderNode<Cell> node = new PathfinderNode<Cell>();
					node.data = cell;
					nodes.Add(cell, node);
				}
			}
			
			// Construct edges
			int edgeCount = 0;

			foreach (Cell cell in nodes.Keys) {
				PathfinderNode<Cell> node = nodes[cell];

				List<PathfinderEdge<Cell>> edges = new List<PathfinderEdge<Cell>>();

				List<Cell> neighbors = cell.GetNeighbors(includeDiagonal: true);

				foreach(Cell neighbor in neighbors) {
					if (neighbor == null) {
						continue;
					}

					if (neighbor.MovementCost > 0 && !IsClippingCorner(cell, neighbor)) {
						PathfinderEdge<Cell> edge = new PathfinderEdge<Cell>();
						edge.cost = neighbor.MovementCost;
						edge.node = nodes[neighbor];
						edges.Add(edge);
						edgeCount++;
					}
				}

				node.edges = edges.ToArray();
			}
		}

		bool IsClippingCorner(Cell cell, Cell neighbor) {
			int dX = cell.X - neighbor.X;
			int dY = cell.Y - neighbor.Y;

			if (Math.Abs((float)dX) + Math.Abs((float)dY) == 2) { // Must be diagonal as both dY and dX == 1
				if (world.GetCellAt(cell.X - dX, cell.Y).MovementCost == 0) {
					return true;
				}

				if (world.GetCellAt(cell.X, cell.Y - dY).MovementCost == 0) {
					return true;
				}
			}

			return false;
		}
	}
}

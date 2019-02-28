using Cartel.Models;
using Cartel.Pathfinder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Priority_Queue;

namespace Cartel {
	public class PathfinderAStar {
		Queue<Cell> path;

		public Queue<Cell> Path {
			get { return path; }
		}

		public PathfinderAStar(World world, Cell startCell, Cell endCell, Predicate<Cell> endCellPredicate = null) {
			if (startCell == null) {
				Console.Error.WriteLine("[Pathfinder Error] Start cell is null");
				return;
			}

			if (endCell == null && endCellPredicate == null) {
				Console.Error.WriteLine("[Pathfinder Error] There is no end predicate or goal cell to reach");
				return;
			}

			if (world.cellGraph == null) {
				world.cellGraph = new PathfinderCellGraph(world);
			}

			Dictionary<Cell, PathfinderNode<Cell>> nodes = world.cellGraph.nodes;

			if (!nodes.ContainsKey(startCell)) {
				Console.Error.WriteLine("[Pathfinder Error] Start cell not in list of nodes");
				return;
			}

			if (endCell != null) {
				if (!nodes.ContainsKey(endCell)) {
					Console.Error.WriteLine("[Pathfinder Error] End cell not in list of nodes");
					return;
				}
			}

			PathfinderNode<Cell> startNode = nodes[startCell];
			PathfinderNode<Cell> goalNode = endCell == null ? null : nodes[endCell];

			if (goalNode != null && (goalNode.edges.Length == 0 || goalNode.data.MovementCost == 0)) {
				Console.Error.WriteLine("[Pathfinder Error] Goal node is unreachable: no edges or movement cost is 0");
				return;
			}

			List<PathfinderNode<Cell>> ClosedSet = new List<PathfinderNode<Cell>>();
			SimplePriorityQueue<PathfinderNode<Cell>> OpenSet = new SimplePriorityQueue<PathfinderNode<Cell>>();
			OpenSet.Enqueue(startNode, 0);

			Dictionary<PathfinderNode<Cell>, PathfinderNode<Cell>> from = new Dictionary<PathfinderNode<Cell>, PathfinderNode<Cell>>();

			// Create gScore
			Dictionary<PathfinderNode<Cell>, float> gScore = new Dictionary<PathfinderNode<Cell>, float>();
			foreach (PathfinderNode<Cell> node in nodes.Values) {
				gScore[node] = float.PositiveInfinity;
			}
			gScore[startNode] = 0;

			// Create fScore
			Dictionary<PathfinderNode<Cell>, float> fScore = new Dictionary<PathfinderNode<Cell>, float>();
			foreach (PathfinderNode<Cell> node in nodes.Values) {
				fScore[node] = float.PositiveInfinity;
			}
			fScore[startNode] = HeuristicCostEstimate(startNode, goalNode);

			while (OpenSet.Count > 0) {
				PathfinderNode<Cell> current = OpenSet.Dequeue();

				if (goalNode == null) {
					if (endCellPredicate != null && endCellPredicate(current.data)) {
						ReconstructPath(from, current);
						return;
					}
				} else if (current == goalNode) {
					ReconstructPath(from, current);
					return;
				}

				ClosedSet.Add(current);

				foreach (PathfinderEdge<Cell> edgeNeighbor in current.edges) {
					PathfinderNode<Cell> neighbor = edgeNeighbor.node;

					if (ClosedSet.Contains(neighbor)) {
						continue; // Already completed
					}

					float movementCostToNeighbor = neighbor.data.MovementCost * DistanceBetween(current, neighbor);
					float tentativeGScore = gScore[current] + movementCostToNeighbor;

					if (OpenSet.Contains(neighbor) && tentativeGScore >= gScore[neighbor]) {
						continue;
					}

					from[neighbor] = current;
					gScore[neighbor] = tentativeGScore;
					fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goalNode);

					if (OpenSet.Contains(neighbor)) {
						OpenSet.UpdatePriority(neighbor, fScore[neighbor]);
					} else {
						OpenSet.Enqueue(neighbor, fScore[neighbor]);
					}
				}
			}
		}

		private float HeuristicCostEstimate(PathfinderNode<Cell> a, PathfinderNode<Cell> b) {
			if (a == null || b == null) {
				return 0f;
			}

			float dX = a.data.X - b.data.X;
			float dY = a.data.Y - b.data.Y;

			return (float)Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2));
		}

		private float DistanceBetween(PathfinderNode<Cell> a, PathfinderNode<Cell> b) {
			if (a == null || b == null) {
				return 0f;
			}

			float dX = a.data.X - b.data.X;
			float dY = a.data.Y - b.data.Y;

			if (Math.Abs(dX) + Math.Abs(dY) == 1) {
				return 1f;
			}

			if (Math.Abs(dX) == 1 && Math.Abs(dY) == 1) {
				return (float)Math.Sqrt(2);
			}

			return (float)Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2));
		}

		void ReconstructPath(Dictionary<PathfinderNode<Cell>, PathfinderNode<Cell>> from, PathfinderNode<Cell> current) {
			Queue<Cell> totalPath = new Queue<Cell>();
			totalPath.Enqueue(current.data);

			while (from.ContainsKey(current)) {
				current = from[current];
				totalPath.Enqueue(current.data);
			}

			path = new Queue<Cell>(totalPath.Reverse());
		}

		public Cell Dequeue() {
			if (path == null) {
				Console.Error.WriteLine("[Pathfinder Error] No path to dequeue cell from");
				return null;
			}

			return path.Dequeue();
		}

		public int Length {
			get {
				if (path == null) {
					return 0;
				}

				return path.Count;
			}
		}

		public Cell LastCell() {
			if (path == null || path.Count == 0) {
				Console.Error.WriteLine("[Pathfinder Error] Path is null or empty");
				return null;
			}

			return path.Last();
		}

	}
}
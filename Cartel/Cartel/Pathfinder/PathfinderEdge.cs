using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Pathfinder {
	public class PathfinderEdge<T> {
		public float cost;
		public PathfinderNode<T> node;
	}
}

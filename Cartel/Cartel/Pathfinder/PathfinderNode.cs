using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Pathfinder {
	public class PathfinderNode<T> {
		public T data;
		public PathfinderEdge<T>[] edges;
	}
}

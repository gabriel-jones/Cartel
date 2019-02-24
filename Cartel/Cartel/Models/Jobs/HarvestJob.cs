using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Models.Jobs {
	public class HarvestJob : Job {
		public override float WorkAmount {
			get { return 1.0f; }
		}

		public HarvestJob(Cell cell) : base(cell) {

		}

		public override bool CanPawnComplete(Pawn pawn) {
			return pawn.Type == PawnType.Gardener;
		}

		protected override void CancelJob() {
			
		}

		protected override void CompleteJob() {
			if (!(cell.Structure is Plant)) {
				throw new InvalidCastException();
			}

			Plant target = (Plant)cell.Structure;
			SoftObject harvest = target.Harvest;
			if (harvest == null) {
				Console.WriteLine("Attempted to harvest plant with no harvest!");
				return;
			}
			cell.World.SpawnSoftObject(harvest, cell, harvest.Count);

			cell.Structure = null;
		}
	}
}

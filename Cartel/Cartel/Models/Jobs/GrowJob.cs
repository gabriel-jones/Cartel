using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Models.Jobs {
	// It's not gay if it's a...
	class GrowJob : Job {
		PlantType plantType;

		public PlantType PlantType {
			get { return plantType; }
		}

		public override float WorkAmount {
			get { return 1.0f; }
		}

		public GrowJob(Cell cell, PlantType plantType) : base(cell) {
			this.plantType = plantType;
		}

		public override bool CanPawnComplete(Pawn pawn) {
			return pawn.Type == PawnType.Gardener;
		}

		protected override void CancelJob() {
			
		}

		protected override void CompleteJob() {
			cell.Structure = new Plant(cell, plantType);
		}
	}
}

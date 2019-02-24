using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Models.Jobs {
	class DestroyJob : Job {
		// Fields
		Constructable target;

		// Properties
		public override float WorkAmount {
			get { return 0.2f; }
		}

		public Constructable Target {
			get { return target; }
		}

		// Constructor
		public DestroyJob(Cell cell, Constructable target) : base(cell) {
			this.target = target;
		}

		// Methods
		public override bool CanPawnComplete(Pawn pawn) {
			// TODO: destroy jobs for different items can be done by different pawns
			return pawn.Type == PawnType.Worker;
		}

		protected override void CancelJob() {
			
		}

		protected override void CompleteJob() {
			if (target is Floor) {
				cell.Floor = null;
			} else if (target is Structure) {
				cell.Structure = null;
			}
		}
	}
}

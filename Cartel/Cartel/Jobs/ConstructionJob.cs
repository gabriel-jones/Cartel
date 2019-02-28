using System;

namespace Cartel.Models.Jobs {
	public class ConstructionJob: Job {
		// Fields
		Blueprint constructionBlueprint;

		// Properties
		public override float WorkAmount {
			get { return 1.0f; } // TODO: dynamic value?
		}

		// Constructor
		public ConstructionJob(Cell cell, Blueprint constructionBlueprint) : base(cell) {
			this.constructionBlueprint = constructionBlueprint;
			this.buildRequirements = constructionBlueprint.Construct.BuildRequirements;
		}

		// Methods
		protected override void CancelJob() {
			if (cell.FloorBlueprint == constructionBlueprint) {
				cell.FloorBlueprint = null;
			} else if (cell.StructureBlueprint == constructionBlueprint) {
				cell.StructureBlueprint = null;
			}
		}

		protected override void CompleteJob(Pawn worker) {
			if (cell.FloorBlueprint == constructionBlueprint) {
				cell.FloorBlueprint = null;
				cell.Floor = (Floor)constructionBlueprint.Construct;
			} else if (cell.StructureBlueprint == constructionBlueprint) {
				cell.StructureBlueprint = null;
				cell.Structure = (Structure)constructionBlueprint.Construct;
			}
		}

		public override bool CanPawnComplete(Pawn pawn) {
			if (pawn.Type == PawnType.Worker) {
				return true;
			}
			return false;
		}
	}
}

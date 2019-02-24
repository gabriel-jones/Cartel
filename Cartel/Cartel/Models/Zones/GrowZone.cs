using Cartel.Models.Jobs;
using System.Linq;

namespace Cartel.Models.Zones {
	class GrowZone : Zone {
		PlantType plantType;

		public PlantType PlantType {
			get { return plantType; }
		}

		public GrowZone(PlantType plantType) : base() {
			this.plantType = plantType;
		}

		public override void Update(float deltaTime) {
			foreach(Cell cell in Cells) {
				if (cell.CanPlant && cell.World.JobManager.JobsForCell(cell).FirstOrDefault(j => j is GrowJob) == null) {
					GrowJob growJob = new GrowJob(cell, plantType);
					cell.World.JobManager.AddJob(growJob);
				}
			}
			base.Update(deltaTime);
		}
	}
}

using Cartel.Models;
using Cartel.Models.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Managers {
	public class JobManager {
		// Fields
		List<Job> jobs;
		World world;

		// Properties


		// Constructor
		public JobManager(World world) {
			jobs = new List<Job>();
			this.world = world;
		}

		// Methods
		public void AddJob(Job job) {
			if (jobs.Contains(job)) {
				Console.Error.WriteLine("Job list already contains this job!");
				return;
			}
			job.SetJobManager(this);
			jobs.Insert(0, job);
		}

		public void RemoveJob(Job job) {
			jobs.Remove(job);
		}

		public Job FindJobForPawn(Pawn pawn) {
			/*
			if (pawn.Type == PawnType.Worker) {
				return FindNearestJobForPawn(pawn);
			}
			return jobs.FirstOrDefault(job => job.CanPawnComplete(pawn) && !job.IsReserved && job.IsReachable);
			*/
			return FindNearestJobForPawn(pawn); //TODO: Is this the best approach?
		}

		public Job FindNearestJobForPawn(Pawn pawn) {
			return jobs.OrderBy(j => j.cell.DistanceToCell(pawn.CurrentCell)).FirstOrDefault(j => j.CanPawnComplete(pawn) && !j.IsReserved && j.IsReachable);
		}

		public void RemovePathfindingFailures() {
			foreach (Job job in jobs) {
				job.SetReachable(true);
			}
		}

		public Job[] JobsForCell(Cell target) {
			return jobs.Where(job => job.cell == target).ToArray();
		}
	}
}

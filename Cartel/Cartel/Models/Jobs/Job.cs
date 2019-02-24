using Cartel.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Models {
	public abstract class Job {
		// Fields
		public Cell cell;
		JobManager jobManager;
		float workAmountRemaining;
		bool isReserved = false;
		bool isReachable = true;
		List<SoftObject> deposit;
		protected List<SoftObject> buildRequirements;

		// Properties
		public abstract float WorkAmount { get; }

		public bool IsReserved {
			get { return isReserved; }
		}

		public bool IsReachable {
			get { return isReachable; }
		}

		// Constructor
		public Job(Cell cell, List<SoftObject> buildRequirements = null) {
			this.cell = cell;
			workAmountRemaining = WorkAmount;
			deposit = new List<SoftObject>();
			this.buildRequirements = buildRequirements;
		}

		// Methods
		public void DepositSoftObject(SoftObject softObject) {
			SoftObject requirement = buildRequirements.FirstOrDefault((obj) => { return obj.Type == softObject.Type; });
			SoftObject existing = deposit.FirstOrDefault((obj) => { return obj.Type == softObject.Type; });

			if (requirement == null || (existing != null && requirement.Count - existing.Count >= softObject.Count)) {
				Console.WriteLine("[Inventory Leak] Depositing object when none is needed!");
				return;
			}

			if (existing != null) {
				existing.IncreaseCount(softObject.Count);
			} else {
				deposit.Add(softObject.Clone());
			}
		}

		public void SetJobManager(JobManager jobManager) {
			this.jobManager = jobManager;
		}

		public void SetReserved(bool isReserved) {
			this.isReserved = isReserved;
		}

		public void SetReachable(bool isReachable) {
			this.isReachable = isReachable;
		}

		public void Progress(float workTime, Pawn worker) {
			workAmountRemaining -= workTime;

			if (workAmountRemaining <= 0) { // Job finished
				Complete(worker);
			}
		}

		public void Complete(Pawn worker) {
			jobManager.RemoveJob(this);
			CompleteJob(worker);
			jobCompletedCallback(this);
		}

		public void Cancel() {
			jobManager.RemoveJob(this);
			CancelJob();
		}

		public bool MeetsBuildRequirements() {
			if (buildRequirements == null) {
				return true;
			}

			return NextRequirement() == null;
		}

		public SoftObject NextRequirement() {
			return buildRequirements.FirstOrDefault((requirement) => {
				SoftObject match = deposit.Find((SoftObject softObject) => {
					return softObject.Type == requirement.Type;
				});
				if (match == null || match.Count < requirement.Count) {
					return true;
				}
				return false;
			});
		}

		// Callbacks
		Action<Job> jobCompletedCallback;

		public void RegisterJobCompletedCallback(Action<Job> callback) {
			jobCompletedCallback += callback;
		}

		public void UnregisterJobCompletedCallback(Action<Job> callback) {
			jobCompletedCallback -= callback;
		}

		// Abstract Methods
		protected abstract void CancelJob();
		protected abstract void CompleteJob(Pawn worker);
		public abstract bool CanPawnComplete(Pawn pawn);
	}
}

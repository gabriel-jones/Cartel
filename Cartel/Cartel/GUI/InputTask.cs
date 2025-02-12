﻿using Cartel.Interfaces;
using Cartel.Models;
using Cartel.Models.Jobs;
using Cartel.Models.Zones;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cartel.GUI.Tasks {
	public enum InputMode {
		Single, Line, Box, Area
	}
	public class InputTask {
		// Properties
		public InputMode Mode { get; protected set; }
		public Action<List<Cell>, List<ISelectable>> Action { get; protected set; }
		public Action<List<Cell>> CancelAction { get; protected set; }
		public Predicate<Cell> CanCancel { get; protected set; }
		public Action<Cell, SpriteBatch> Preview { get; protected set; }

		// Constructor
		public InputTask(InputMode mode, Action<List<Cell>, List<ISelectable>> action, Action<Cell, SpriteBatch> preview, Action<List<Cell>> cancelAction = null, Predicate<Cell> canCancel = null) {
			Mode = mode;
			Action = action;
			Preview = preview;
			CancelAction = cancelAction;
			CanCancel = canCancel;
		}

		// Static Methods
		public static InputTask PawnFactory(PawnType type) {
			return new InputTask(InputMode.Single, (cells, selected) => {
				foreach (Cell cell in cells) {
					cell.World.AddPawn(new Pawn(cell.World, type, cell));
				}
			}, (cell, spriteBatch) => {
				Pawn preview = new Pawn(cell.World, type, cell);
				preview.Draw(spriteBatch, cell.X * World.BlockSize, cell.Y * World.BlockSize);
			});
		}

		public static InputTask BlueprintFactory(InputMode mode, Func<Cell, Constructable> construct) {
			return new InputTask(mode, (cells, selected) => {
				foreach (Cell cell in cells) {
					Blueprint blueprint = new Blueprint(cell.World, construct(cell));
					if (blueprint != null && cell.World.ValidatePlacement(cell.X, cell.Y, blueprint)) {
						if (blueprint.Construct is Structure) {
							cell.StructureBlueprint = blueprint;
						} else if (blueprint.Construct is Floor) {
							cell.FloorBlueprint = blueprint;
						}
						ConstructionJob job = new ConstructionJob(cell, blueprint);
						cell.World.JobManager.AddJob(job);
						blueprint.job = job;
					}
				}
			}, (cell, spriteBatch) => {
				Blueprint preview = new Blueprint(cell.World, construct(cell));
				bool validPlacement = cell.World.ValidatePlacement(cell.X, cell.Y, preview);
				preview.Draw(spriteBatch, cell.X, cell.Y, validPlacement ? Blueprint.BlueprintState.Preview : Blueprint.BlueprintState.Invalid);
			}, (cells) => {
				foreach(Cell cell in cells) {
					cell.CancelJobs();
				}
			}, (cell) => {
				return cell.HasBlueprints;
			});
		}

		public static InputTask SoftObjectFactory(SoftObjectType type, int amount) {
			return new InputTask(InputMode.Single, (cells, selected) => {
				foreach (Cell cell in cells) {
					SoftObject softObject = new SoftObject(type, amount);
					cell.World.SpawnSoftObject(softObject, cell, softObject.Count);
				}
			}, null);
		}

		public static InputTask BulldozeFactory() {
			return new InputTask(InputMode.Area, (cells, selected) => {
				foreach (Cell cell in cells) {
					if (cell.Floor != null) {
						DestroyJob job = new DestroyJob(cell, cell.Floor);
						cell.World.JobManager.AddJob(job);
					} else if (cell.Structure != null) {
						DestroyJob job = new DestroyJob(cell, cell.Structure);
						cell.World.JobManager.AddJob(job);
					}
				}
			}, (cell, spriteBatch) => {
				Blueprint preview = new Blueprint(cell.World, new Floor(cell, FloorType.Concrete));
				preview.Draw(spriteBatch, cell.X, cell.Y, Blueprint.BlueprintState.Invalid);
			});
		}

		public static InputTask ZoneFactory(Func<Zone> factory) {
			return new InputTask(InputMode.Area, (cells, selected) => {
				if (cells.Count == 0) {
					return;
				}
				List<Zone> selectedZones = new List<Zone>();
				foreach (ISelectable selectedObject in selected) {
					if (selectedObject is Zone) {
						selectedZones.Add((Zone)selectedObject);
					}
				}

				Zone target = factory();
				foreach (Cell cell in cells) {
					if (cell.MovementCost == 0) {
						continue;
					}
					List<Cell> neighbors = cell.GetNeighbors(false);
					neighbors.Add(cell);
					foreach(Cell neighbor in neighbors) {
						if (neighbor != null && neighbor.Zone != null && selectedZones.Contains(neighbor.Zone)) { // This cell has a neighbor that has a selected zone in it
							target = neighbor.Zone;
						}
					}
				}
				foreach (Cell cell in cells) {
					if (cell.MovementCost == 0) {
						continue;
					}
					target.AddCell(cell);
				}
				if (!cells.First().World.Zones.Contains(target)) {
					cells.First().World.AddZone(target);
				}
			}, (cell, spriteBatch) => {
				Zone copy = factory();
				copy.DrawAtCell(spriteBatch, cell, 1.0f, false);
			}, (cells) => {
				foreach (Cell cell in cells) {
					if (cell.Zone != null) {
						cell.Zone.RemoveCell(cell);
					}
				}
			}, (cell) => {
				return cell.Zone != null;
			});
		}
	}
}

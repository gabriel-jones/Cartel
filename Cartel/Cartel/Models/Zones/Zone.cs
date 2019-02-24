using Cartel.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Models.Zones {
	public class Zone : Interfaces.ISelectable {
		// Fields
		List<Cell> cells;

		// Properties
		public bool IsSelected { get; set; }

		public List<Cell> Cells {
			get {
				return cells;
			}
		}

		// Constructor
		public Zone() {
			cells = new List<Cell>();
		}

		// Methods
		public void AddCell(Cell target) {
			cells.Add(target);
			if (target.Zone != null) {
				target.Zone.RemoveCell(target);
			}
			target.Zone = this;
		}

		public void RemoveCell(Cell target) {
			cells.Remove(target);
			target.Zone = null;
		}

		public virtual void Update(float deltaTime) { }

		public void Draw(SpriteBatch spriteBatch) {
			foreach (Cell cell in cells) {
				Draw(spriteBatch, cell);
			}
		}

		public void Draw(SpriteBatch spriteBatch, Cell cell, bool withBorder = true) {
			List<Cell> neighbors = cell.GetNeighbors(false);
			bool north = neighbors[0] == null || neighbors[0].Zone != this;
			bool east = neighbors[1] == null || neighbors[1].Zone != this;
			bool south = neighbors[2] == null || neighbors[2].Zone != this;
			bool west = neighbors[3] == null || neighbors[3].Zone != this;
			BitArray mask = new BitArray(new bool[] { north, east, south, west });
			Rectangle area = new Rectangle(cell.X * World.BlockSize, cell.Y * World.BlockSize, World.BlockSize, World.BlockSize);
			if (!withBorder) {
				mask = new BitArray(4, false);
			}
			ShapeManager.DrawRectangle(spriteBatch, area, new Color(1f, 0, 0, 0.25f), new Color(0, 0, 1f, 0.25f), mask);
		}
	}
}

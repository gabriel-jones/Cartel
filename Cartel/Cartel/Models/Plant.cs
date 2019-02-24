using Cartel.Managers;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Cartel.Models.Jobs;

namespace Cartel.Models {
	public enum PlantType {
		Sativa, Indica, Poppy, Coca, MorningGlory, Ayahuasca
	}

	public class Plant : Structure {
		// Fields
		PlantType type;
		float growRemaining;

		// Properties
		public PlantType Type {
			get { return type; }
		}

		public override float MovementCost {
			get { return 0.95f; }
		}

		public override Texture2D Texture {
			get {
				switch (type) {
					case PlantType.Sativa:
						return AssetManager.GetAsset<Texture2D>("plant_sativa");
					case PlantType.Indica:
						return AssetManager.GetAsset<Texture2D>("plant_indica");
					case PlantType.Poppy:
						return AssetManager.GetAsset<Texture2D>("plant_poppy");
					case PlantType.Coca:
						return AssetManager.GetAsset<Texture2D>("plant_coca");
				}
				return null;
			}
		}

		public SoftObject Harvest {
			get {
				switch (type) {
					case PlantType.Sativa:
						return new SoftObject(SoftObjectType.Concrete, 5);
					case PlantType.Indica:
						return new SoftObject(SoftObjectType.Concrete, 5);
					case PlantType.Poppy:
						return new SoftObject(SoftObjectType.Concrete, 5);
					case PlantType.Coca:
						return new SoftObject(SoftObjectType.Concrete, 5);
				}
				return null;
			}
		}

		public float GrowTime {
			get {
				Dictionary<PlantType, int> days = new Dictionary<PlantType, int> {
					[PlantType.Indica] = 1,
					[PlantType.Sativa] = 1,
					[PlantType.Poppy] = 4,
					[PlantType.Coca] = 4,
					[PlantType.MorningGlory] = 4,
					[PlantType.Ayahuasca] = 4
				};
				return TimeHelper.GameTimeToReal(TimeUnit.Day, days[type]);
			}
		}

		public float GrowProgress {
			get { return Math.Min(1, (GrowTime - growRemaining) / GrowTime); }
		}

		public bool IsMature {
			get { return growRemaining <= 0; }
		}

		// Constructor
		public Plant(Cell cell, PlantType plantType) : base(cell) {
			growRemaining = GrowTime;
			type = plantType;
		}

		// Methods
		public override void Update(float deltaTime) {
			if (growRemaining > 0) {
				growRemaining -= deltaTime;
			} else {
				if (cell.World.JobManager.JobsForCell(cell).FirstOrDefault(job => job is HarvestJob) == null) {
					cell.World.JobManager.AddJob(new HarvestJob(cell));
				}
			}
		}

		public override Rectangle CalculateSourceRectangle() {
			int index = (int)(GrowProgress * 4);
			if (index == 4) {
				index = 3;
			}

			int width = Texture.Width / 2;
			int height = Texture.Height / 2;
			int row = (int)((float)index / 2.0f);
			int column = index % 2;

			return new Rectangle(width * column, height * row, width, height);
		}

		public override void Draw(SpriteBatch spriteBatch, int x, int y) {
			base.Draw(spriteBatch, x, y);
		}
	}
}

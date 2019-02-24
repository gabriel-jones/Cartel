using Cartel.Managers;
using Cartel.Models.Jobs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel.Models {
	public class Blueprint {
		// Enums
		public enum BlueprintState {
			Invalid, Preview, Default
		}

		// Fields
		World world;
		public ConstructionJob job;

		// Properties
		public Constructable Construct { get; protected set; }

		// Constructor
		public Blueprint(World world, Constructable construct) {
			this.world = world;
			this.Construct = construct;
		}

		// Methods
		public void Draw(SpriteBatch spriteBatch, int x, int y, BlueprintState state = BlueprintState.Default) {
			if (Construct.Texture == null) {
				return;
			}
			spriteBatch.Draw(Construct.Texture,
				new Rectangle(x * World.BlockSize, y * World.BlockSize, World.BlockSize, World.BlockSize),
				CalculateSourceRectangle(),
				(job != null && job.IsReserved) ? Color.Orange : TintForState(state),
				0.0f,
				new Vector2(0),
				SpriteEffects.None,
				DrawLayer.Blueprint
			);
		}

		public Rectangle CalculateSourceRectangle() {
			return Construct.CalculateSourceRectangle();
		}

		private Color TintForState(BlueprintState state) {
			if (job != null && !job.IsReachable) {
				return new Color(1.0f, 0.0f, 0.0f, 0.8f);
			}

			switch (state) {
				case BlueprintState.Default:
					return new Color(0.0f, 0.0f, 1.0f, 0.8f);
				case BlueprintState.Invalid:
					return new Color(1.0f, 0.0f, 0.0f, 0.8f);
				case BlueprintState.Preview:
					return new Color(0.0f, 1.0f, 0.0f, 0.8f);
				default:
					return Color.White;
			}
		}
	}
}

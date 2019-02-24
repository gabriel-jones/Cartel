using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cartel.Models {
	public abstract class Structure: Constructable {
		// Fields

		// Properties
		public override abstract Texture2D Texture { get; }

		public virtual float MovementCost {
			get { return IsTraversible ? 1 : 0; }
		}

		public virtual bool IsTraversible {
			get { return true; }
		}

		// Constructors
		public Structure(Cell cell) : base(cell) { }

		// Methods
		override public Rectangle CalculateSourceRectangle() {
			return new Rectangle(0, 0, Texture.Width, Texture.Height);
		}

		public override void Draw(SpriteBatch spriteBatch, int x, int y) {
			base.Draw(spriteBatch, x, y, DrawLayer.Structure);
		}

		public virtual void Update(float deltaTime) { }
	}
}
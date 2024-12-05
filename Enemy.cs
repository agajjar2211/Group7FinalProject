using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;


namespace FinalProjectGame
{
	internal class Enemy
	{
		public Vector2 Position; // Enemy position
		public float Speed; // Movement speed
		public Rectangle MovementBounds; // Movement boundaries for the enemy

		public Enemy(Vector2 position, float speed, Rectangle movementBounds)
		{
			Position = position;
			Speed = speed;
			MovementBounds = movementBounds;
		}
	}
}

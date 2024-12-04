using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FinalProjectGame
{
	internal class Player
	{
		public Vector2 Position; // Player position
		public Vector2 Velocity; // Movement velocity
		public bool IsJumping; // Tracks jumping state

		public Player(Vector2 startPosition)
		{
			Position = startPosition;
			Velocity = Vector2.Zero;
			IsJumping = false;
		}
	}
}

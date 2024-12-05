using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FinalProjectGame
{
	internal class Coin
	{
		public Vector2 Position; // Coin position
		public bool Collected; // Whether the coin has been collected

		public Coin(Vector2 position)
		{
			Position = position;
			Collected = false;
		}
	}
}

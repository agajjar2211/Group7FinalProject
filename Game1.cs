using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProjectGame
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		private Player _player;
		private Platform[] _platforms;
		private Coin[] _coins;
		private Enemy[] _enemies;

		private Texture2D _playerTexture, _platformTexture, _coinTexture, _enemyTexture, _backgroundTexture;
		private SpriteFont _font;

		private Vector2 _backgroundOffset;
		private int _score;
		private bool _gameOver;
		private bool _gameOverAtEnd;
		private bool _isGameFrozen;
		private const int Gravity = 1;
		private const int JumpStrength = -10;

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			_graphics.PreferredBackBufferWidth = 800;
			_graphics.PreferredBackBufferHeight = 600;
			_graphics.ApplyChanges();
			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
			_playerTexture = Content.Load<Texture2D>("guidoPlayer");
			_platformTexture = Content.Load<Texture2D>("platform");
			_coinTexture = Content.Load<Texture2D>("coin");
			_enemyTexture = Content.Load<Texture2D>("deathcapEnemy");
			_backgroundTexture = Content.Load<Texture2D>("background");
			_font = Content.Load<SpriteFont>("DefaultFont");

			// Initialize game elements
			_player = new Player(new Vector2(100, 500 )  ); // Fixed X position for player


			_platforms = new[]
			{
				new Platform(new Rectangle(0, 580, 800, 20)),      // Ground platform
				new Platform(new Rectangle(150, 450, 200, 20)),   // First elevated platform
				new Platform(new Rectangle(400, 350, 200, 20)),   // Second elevated platform
				new Platform(new Rectangle(650, 250, 150, 20))    // Third elevated platform
			};


			_coins = new[]
			{
				new Coin(new Vector2(150 + 200 / 2 - 10, 450 - 20)),
				new Coin(new Vector2(400 + 200 / 2 - 10, 350 - 20)),
				new Coin(new Vector2(650 + 150 / 2 - 10, 250 - 20))
			};


			
			_enemies = new[]
{
	new Enemy(new Vector2(180, 430 - _enemyTexture.Height), 2, _platforms[1].Rectangle), // First elevated platform
    new Enemy(new Vector2(470, 330 - _enemyTexture.Height), -2, _platforms[2].Rectangle), // Second elevated platform
    new Enemy(new Vector2(720, 230 - _enemyTexture.Height), 2, _platforms[3].Rectangle)   // Third elevated platform
};
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			// TODO: Add your update logic here

			// Prevent updates if the game is frozen
			if (_isGameFrozen)
				return;

			var keyboardState = Keyboard.GetState();

			if (keyboardState.IsKeyDown(Keys.Left))
			{
				_player.Position.X -= 3;
			}


			if (keyboardState.IsKeyDown(Keys.Right))
			{
				_player.Position.X += 3;
			}
			// Prevent the player from moving out of the window boundaries
			_player.Position.X = MathHelper.Clamp(_player.Position.X, 0, _graphics.PreferredBackBufferWidth - _playerTexture.Width);


			// Gravity applied only if player is above the lower bound
			if (_player.Position.Y < 489)
				_player.Velocity.Y += Gravity; 
			else
			{
				_player.Velocity.Y = 0;       
				_player.IsJumping = false;   
			}

			
			_player.Position += _player.Velocity;

			
			_player.Position.Y = MathHelper.Clamp(_player.Position.Y, 0, 489);




			
			if (_player.Position.X < 0)
			{
				_player.Position.X = 0;
			}

			// Prevent moving past the right edge
			if (_player.Position.X > 2000 - _playerTexture.Width)
			{
				_player.Position.X = 2000 - _playerTexture.Width;
			}

			// Check if player reaches the right edge of the screen
			if (_player.Position.X >= _graphics.PreferredBackBufferWidth - _playerTexture.Width)
			{
				_gameOverAtEnd = true;
			}

		


			// Boosted jump logic with directional movement
			if (keyboardState.IsKeyDown(Keys.Space) && keyboardState.IsKeyDown(Keys.Enter))
			{
				if (!_player.IsJumping)
				{
					_player.Velocity.Y = -15; // Higher jump strength for boosted jump
					_player.IsJumping = true;
				}

				// Handle directional movement during boosted jump
				if (keyboardState.IsKeyDown(Keys.Right))
				{
					_player.Position.X += 5; // Boosted jump with movement to the right
				}
				else if (keyboardState.IsKeyDown(Keys.Left))
				{
					_player.Position.X -= 5; // Boosted jump with movement to the left
				}
			}

			// Normal jump logic
			if (keyboardState.IsKeyDown(Keys.Space) && !keyboardState.IsKeyDown(Keys.Enter) && !_player.IsJumping)
			{
				_player.Velocity.Y = -10; // Regular jump strength
				_player.IsJumping = true;
			}

			// Apply gravity and update player's position
			_player.Velocity.Y += Gravity;
			_player.Position += _player.Velocity;

			// Clamp the player's X position within screen boundaries
			_player.Position.X = MathHelper.Clamp(_player.Position.X, 0, _graphics.PreferredBackBufferWidth - _playerTexture.Width);



			
			foreach (var platform in _platforms)
			{
				// Check if the player is colliding with the platform
				if (_player.Position.Y + _playerTexture.Height > platform.Rectangle.Y &&
					_player.Position.Y < platform.Rectangle.Y &&
					_player.Position.X + _playerTexture.Width > platform.Rectangle.X &&
					_player.Position.X < platform.Rectangle.X + platform.Rectangle.Width)
				{
					// Allow collision only if the player is falling (moving downward)
					if (_player.Velocity.Y >= 0) // Falling
					{
						_player.Position.Y = platform.Rectangle.Y - _playerTexture.Height;
						_player.Velocity.Y = 0;
						_player.IsJumping = false;
					}
					else if (_player.Velocity.Y < 0) // Jumping
					{
						// Prevent collision ONLY IF the player is directly under the solid portion of the platform
						if (_player.Position.X + (_playerTexture.Width / 2) > platform.Rectangle.X &&
							_player.Position.X + (_playerTexture.Width / 2) < platform.Rectangle.X + platform.Rectangle.Width)
						{
							// Adjust the player's position to stay below the platform
							_player.Position.Y = platform.Rectangle.Y + platform.Rectangle.Height;
							_player.Velocity.Y = 0; // Stop upward velocity
						}
					}
				}
			}



			// Handle ground collision (grass level)
			if (_player.Position.Y + _playerTexture.Height > 580) 
			{
				_player.Position.Y = 580 - _playerTexture.Height; 
				_player.Velocity.Y = 0; 
				_player.IsJumping = false; 
			}

			// Coin collection
			foreach (var coin in _coins)
			{
				if (!coin.Collected)
				{
					// Create bounding boxes for the player and the coin
					Rectangle playerBounds = new Rectangle(
						(int)_player.Position.X,
						(int)_player.Position.Y,
						_playerTexture.Width,
						_playerTexture.Height
					);

					Rectangle coinBounds = new Rectangle(
						(int)coin.Position.X,
						(int)coin.Position.Y,
						_coinTexture.Width,
						_coinTexture.Height
					);

					// Check if the player's bounding box intersects the coin's bounding box
					if (playerBounds.Intersects(coinBounds))
					{
						// Ensure the player is on the same platform as the coin
						foreach (var platform in _platforms)
						{
							if (_player.Position.Y + _playerTexture.Height == platform.Rectangle.Y &&
								coin.Position.X + _coinTexture.Width > platform.Rectangle.X &&
								coin.Position.X < platform.Rectangle.X + platform.Rectangle.Width)
							{
								coin.Collected = true; 
								_score++;              
							}
						}
					}
				}
			}

			// Move the background with the average position of the enemies
			_backgroundOffset.X = -(_enemies[0].Position.X + _enemies[1].Position.X + _enemies[2].Position.X) / 3 % _graphics.PreferredBackBufferWidth;
			// Enemy movement and collision
			foreach (var enemy in _enemies)
			{
				// If the game is frozen, stop enemy movement
				if (_isGameFrozen)
					continue;

				// Enemy movement logic
				enemy.Position.X += enemy.Speed;

				// Reverse enemy direction when hitting bounds
				if (enemy.Position.X < enemy.MovementBounds.X ||
					enemy.Position.X + _enemyTexture.Width > enemy.MovementBounds.X + enemy.MovementBounds.Width)
				{
					enemy.Speed *= -1;
				}

				// Check collision between player and enemy
				if (_player.Position.X < enemy.Position.X + _enemyTexture.Width &&
					_player.Position.X + _playerTexture.Width > enemy.Position.X &&
					_player.Position.Y < enemy.Position.Y + _enemyTexture.Height &&
					_player.Position.Y + _playerTexture.Height > enemy.Position.Y)
				{
					_isGameFrozen = true; // Freeze the game
					_player.Velocity = Vector2.Zero; // Stop player movement
					break; // Exit loop after collision
				}
			}

			// Stop player movement if the game is frozen
			if (_isGameFrozen)
			{
				_player.Velocity = Vector2.Zero; // Prevent player from moving
			}


			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// TODO: Add your drawing code here
			_spriteBatch.Begin();
			// Draw background
			_spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
			
			foreach (var platform in _platforms)
			{
				_spriteBatch.Draw(_platformTexture, new Rectangle(platform.Rectangle.X, platform.Rectangle.Y, platform.Rectangle.Width, platform.Rectangle.Height), Color.White);
			}

			//Draw coins
			foreach (var coin in _coins)
			{
				if (!coin.Collected)
				{
					var coinRectangle = new Rectangle((int)coin.Position.X, (int)coin.Position.Y, 20, 20); // Scale to 20x20
					_spriteBatch.Draw(_coinTexture, coinRectangle, Color.White);
				}
			}

			// Draw enemies
			foreach (var enemy in _enemies)
			{
				_spriteBatch.Draw(_enemyTexture, new Vector2(enemy.Position.X, enemy.Position.Y), Color.White);
			}

			// Draw player
			_spriteBatch.Draw(_playerTexture, _player.Position, Color.White);

			// Draw score
			_spriteBatch.DrawString(_font, $"Score: {_score} X = {_player.Position.X} , Y ={_player.Position.Y}", new Vector2(10, 10), Color.White);

			// Draw game-over message
			if (_gameOver)
			{
				var gameOverText = "Game Over!";
				var textSize = _font.MeasureString(gameOverText);
				var position = new Vector2(
					(_graphics.PreferredBackBufferWidth - textSize.X) / 2,
					(_graphics.PreferredBackBufferHeight - textSize.Y) / 2
				);
				_spriteBatch.DrawString(_font, gameOverText, position, Color.Red);
			}

			if (_isGameFrozen)
			{
				var freezeMessage = "Game Over! Collision Detected!";
				var textSize = _font.MeasureString(freezeMessage);
				var position = new Vector2(
					(_graphics.PreferredBackBufferWidth - textSize.X) / 2,
					(_graphics.PreferredBackBufferHeight - textSize.Y) / 2
				);

				_spriteBatch.DrawString(_font, freezeMessage, position, Color.Red);
			}

			if (_gameOverAtEnd)
			{
				var gameOverText = "You Win! Game Over";
				var textSize = _font.MeasureString(gameOverText);
				var position = new Vector2(
					(_graphics.PreferredBackBufferWidth - textSize.X) / 2,
					(_graphics.PreferredBackBufferHeight - textSize.Y) / 2
				);
				_spriteBatch.DrawString(_font, gameOverText, position, Color.Green);
				_spriteBatch.End();
				return; // Stop drawing further if game over
			}
			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}

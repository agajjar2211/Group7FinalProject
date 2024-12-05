using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

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
		private const int JumpStrength = -15;


        //for sound 
        private Song _menuMusic;
        private Song _gameplayMusic;
        private Song _collisionSound;
        private Song _gameOverSound;

        private bool _musicPlaying;


        private double _elapsedTime; // Time elapsed in seconds
		private string _endMessage; // Message to display on Game Over	
		private const double TimeLimit = 30.0; // 30 seconds time limit
		private bool _shouldRefresh = false;  // Flag for screen refresh

		// Added variables for menu handling
		private string _currentGameState = "StartMenu"; // "StartMenu", "Playing", "GameOverMenu"

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			_graphics.PreferredBackBufferWidth = 800;
			_graphics.PreferredBackBufferHeight = 600;
			_graphics.ApplyChanges();
			base.Initialize();
		}

		protected override void LoadContent()
		{

            // for song 

            _menuMusic = Content.Load<Song>("gameMenu");
            _gameplayMusic = Content.Load<Song>("inGame");
            _collisionSound = Content.Load<Song>("enemyHit");
            _gameOverSound = Content.Load<Song>("gameEnd");

            MediaPlayer.IsRepeating = true;
            //initalising it 
            PlayMusic(_menuMusic);


            _spriteBatch = new SpriteBatch(GraphicsDevice);

			_playerTexture = Content.Load<Texture2D>("guidoPlayer");
			_platformTexture = Content.Load<Texture2D>("platform");
			_coinTexture = Content.Load<Texture2D>("coin");
			_enemyTexture = Content.Load<Texture2D>("deathcapEnemy");
			_backgroundTexture = Content.Load<Texture2D>("background");
			_font = Content.Load<SpriteFont>("DefaultFont");

			_player = new Player(new Vector2(100, 500));

			_platforms = new[]
			{
				new Platform(new Rectangle(0, 580, 800, 20)),
				new Platform(new Rectangle(150, 450, 200, 20)),
				new Platform(new Rectangle(400, 350, 200, 20)),
				new Platform(new Rectangle(650, 250, 150, 20))
			};

			_coins = new[]
			{
				new Coin(new Vector2(150 + 200 / 2 - 10, 450 - 20)),
				new Coin(new Vector2(400 + 200 / 2 - 10, 350 - 20)),
				new Coin(new Vector2(650 + 150 / 2 - 10, 250 - 20))
			};

			_enemies = new[]
			{
				new Enemy(new Vector2(180, 430 - _enemyTexture.Height), 1, _platforms[1].Rectangle),
				new Enemy(new Vector2(470, 330 - _enemyTexture.Height), -1, _platforms[2].Rectangle),
				new Enemy(new Vector2(720, 230 - _enemyTexture.Height), 1, _platforms[3].Rectangle)
			};
		}
        private void PlayMusic(Song song)
        {
            MediaPlayer.Stop();
            MediaPlayer.Play(song);
            _musicPlaying = true;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var keyboardState = Keyboard.GetState();

            // Handle game states
            if (_currentGameState == "StartMenu")
            {
                if (!_musicPlaying)
                {
                    PlayMusic(_menuMusic);
                }
                if (keyboardState.IsKeyDown(Keys.Enter))
                {
                    _currentGameState = "Playing";
                    _elapsedTime = 0;
                    PlayMusic(_gameplayMusic);
                }
                else if (keyboardState.IsKeyDown(Keys.Escape))
                {
                    Exit();
                }
                return; // Skip the rest of the update logic in StartMenu
            }

            if (_currentGameState == "GameOverMenu")
            {
                if (!_musicPlaying)
                {
                    PlayMusic(_gameOverSound); // Play end music
                }
                if (keyboardState.IsKeyDown(Keys.R))
                {
                    RestartGame();
                }
                else if (keyboardState.IsKeyDown(Keys.Escape))
                {
                    Exit();
                }
                return; // Skip the rest of the update logic in GameOverMenu
            }

            // Update elapsed time while playing
            if (_currentGameState == "Playing")
            {
                _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds; // Increment elapsed time
                                                                       // Check time limit
                if (_elapsedTime >= TimeLimit)
                {
                    _endMessage = "Time's up! Game over.";
                    _currentGameState = "GameOverMenu";
                    return;
                }
            }

            // Prevent updates if the game is frozen
            if (_isGameFrozen)
                return;

            // Movement and jumping logic
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                _player.Position.X -= 3;
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                _player.Position.X += 3;
            }

            // Clamp player's horizontal position
            _player.Position.X = MathHelper.Clamp(_player.Position.X, 0, _graphics.PreferredBackBufferWidth - _playerTexture.Width);

            // Jumping logic - consolidated
            bool canJump = !_player.IsJumping;
            if (keyboardState.IsKeyDown(Keys.Space) && canJump)
            {
                // Regular jump
                if (!keyboardState.IsKeyDown(Keys.Enter))
                {
                    _player.Velocity.Y = JumpStrength; // Use constant for consistent jump
                    _player.IsJumping = true;
                }
                // Boosted jump with directional movement
                else
                {
                    _player.Velocity.Y = JumpStrength - 5; // Slightly stronger jump
                    _player.IsJumping = true;

                    // Directional movement during boosted jump
                    if (keyboardState.IsKeyDown(Keys.Right))
                    {
                        _player.Position.X += 5;
                    }
                    else if (keyboardState.IsKeyDown(Keys.Left))
                    {
                        _player.Position.X -= 5;
                    }
                }
            }

            // Apply gravity consistently
            _player.Velocity.Y += Gravity;
            _player.Position += _player.Velocity;

            // Check if player reaches the right edge of the screen
            if (_player.Position.X >= _graphics.PreferredBackBufferWidth - _playerTexture.Width)
            {
                _shouldRefresh = true; // Trigger screen refresh
            }

            // Handle screen refresh
            if (_shouldRefresh)
            {
                RefreshGameElements();
                _shouldRefresh = false; // Reset the flag
            }

            // Platform collision with improved jump reset logic
            bool isOnGround = _player.Position.Y >= 580 - _playerTexture.Height; // Ground Level
            bool isOnPlatform = false;

            // Platform Collision Detection (Top Surface)

            foreach (var platform in _platforms)
            {
                
                if (_player.Position.Y + _playerTexture.Height >= platform.Rectangle.Y &&
                    _player.Position.Y + _playerTexture.Height <= platform.Rectangle.Y + platform.Rectangle.Height &&
                    _player.Position.X + _playerTexture.Width > platform.Rectangle.X &&
                    _player.Position.X < platform.Rectangle.X + platform.Rectangle.Width &&
                    _player.Velocity.Y >= 0) // Player must be falling
                {
                    _player.Position.Y = platform.Rectangle.Y - _playerTexture.Height;
                    _player.Velocity.Y = 0;
                    _player.IsJumping = false; // Allow jumping again
                    isOnPlatform = false;
                    break;
                }

            }

            // Ground collision
            if (_player.Position.Y + _playerTexture.Height > 580 || isOnPlatform)
            {
                _player.Position.Y = 580 - _playerTexture.Height;
                _player.Velocity.Y = 0;
                _player.IsJumping = false;
            }

            // Vertical position clamping
            _player.Position.Y = MathHelper.Clamp(_player.Position.Y, 0, 489);

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
                    _currentGameState = "GameOverMenu";
                    PlayMusic(_gameOverSound);
                    break; // Exit loop after collision
                }
            }

            // Stop player movement if the game is frozen
            if (_isGameFrozen)
            {
                _player.Velocity = Vector2.Zero; // Prevent player from moving
            }

            if (_isGameFrozen)
            {
                _endMessage = $"You collided with an enemy!";
                _currentGameState = "GameOverMenu";
            }
            else if (_gameOverAtEnd)
            {
                _endMessage = $"Good job! Game over.";
                _currentGameState = "GameOverMenu";
            }

            base.Update(gameTime);
        }
        
        private void RefreshGameElements()
		{
			var random = new Random();

			// Reset player position
			_player.Position = new Vector2(100, 500);

			// Generate random platforms
			_platforms = new Platform[4];
			for (int i = 0; i < _platforms.Length; i++)
			{
				int platformWidth = random.Next(150, 300); // Random width between 150 and 300
				int platformX = random.Next(50, _graphics.PreferredBackBufferWidth - platformWidth - 50); // Random X within screen bounds
				int platformY = 200 + i * 100; // Space platforms vertically to avoid overlap

				_platforms[i] = new Platform(new Rectangle(platformX, platformY, platformWidth, 20));
			}

			// Generate coins on platforms
			_coins = new Coin[_platforms.Length];
			for (int i = 0; i < _platforms.Length; i++)
			{
				var platform = _platforms[i];
				int coinX = platform.Rectangle.X + platform.Rectangle.Width / 2 - 10; // Center coin on the platform
				int coinY = platform.Rectangle.Y - 20; // Place coin slightly above the platform

				_coins[i] = new Coin(new Vector2(coinX, coinY));
			}

			// Generate enemies on platforms
			_enemies = new Enemy[_platforms.Length];
			for (int i = 0; i < _platforms.Length; i++)
			{
				var platform = _platforms[i];
				int enemyX = random.Next(platform.Rectangle.X, platform.Rectangle.X + platform.Rectangle.Width - 50); // Random X within platform bounds
				int enemyY = platform.Rectangle.Y - _enemyTexture.Height; // Place enemy on top of the platform
				//int speed = random.Next(2, 5) * (random.Next(0, 2) == 0 ? -1 : 1); // Random speed, either positive or negative

				_enemies[i] = new Enemy(new Vector2(enemyX, enemyY), 1, platform.Rectangle);
			}
		}

			private void RestartGame()
		{
			_player.Position = new Vector2(100, 500);
			_score = 0; // Reset score
			_elapsedTime = 0; // Reset elapsed time
			_isGameFrozen = false;
			_gameOver = false;
			_gameOverAtEnd = false;

			foreach (var coin in _coins)
				coin.Collected = false;

			_currentGameState = "StartMenu"; // Reset to Start Menu
            PlayMusic(_menuMusic);
        }

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			_spriteBatch.Begin();

			if (_currentGameState == "StartMenu")
			{
				DrawStartMenu();
			}
			else if (_currentGameState == "Playing")
			{
				DrawPlayingState();
			}
			else if (_currentGameState == "GameOverMenu")
			{
				DrawGameOverMenu();
			}

			_spriteBatch.End();

			base.Draw(gameTime);
		}

		private void DrawStartMenu()
		{
			// Draw a semi-transparent background for the menu
			_spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, 800, 600), new Color(0, 0, 0, 150));

			// Title text
			var title = "Welcome to Our Game!";
			var titleSize = _font.MeasureString(title);
			_spriteBatch.DrawString(_font, title, new Vector2(400 - titleSize.X / 2, 150), Color.Yellow);

			// Button-like rectangles and text
			var buttonWidth = 200;
			var buttonHeight = 50;
			var startButtonRect = new Rectangle(300, 250, buttonWidth, buttonHeight);
			var exitButtonRect = new Rectangle(300, 350, buttonWidth, buttonHeight);

			// Draw buttons
			_spriteBatch.Draw(_platformTexture, startButtonRect, Color.DarkGray);
			_spriteBatch.Draw(_platformTexture, exitButtonRect, Color.DarkGray);

			// Draw button text
			var startMessage = "Start Game (press enter)";
			var startTextSize = _font.MeasureString(startMessage);
			_spriteBatch.DrawString(_font, startMessage, new Vector2(
				startButtonRect.X + (buttonWidth - startTextSize.X) / 2,
				startButtonRect.Y + (buttonHeight - startTextSize.Y) / 2), Color.White);

			var exitMessage = "Exit (press esc)";
			var exitTextSize = _font.MeasureString(exitMessage);
			_spriteBatch.DrawString(_font, exitMessage, new Vector2(
				exitButtonRect.X + (buttonWidth - exitTextSize.X) / 2,
				exitButtonRect.Y + (buttonHeight - exitTextSize.Y) / 2), Color.White);

		}

		private void DrawPlayingState()
		{
			_spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, 800, 600), Color.White);

			foreach (var platform in _platforms)
			{
				_spriteBatch.Draw(_platformTexture, platform.Rectangle, Color.White);
			}

            foreach (var coin in _coins)
            {
                if (!coin.Collected)
                {
                    var coinRectangle = new Rectangle((int)coin.Position.X, (int)coin.Position.Y, 20, 20); // Scale to 20x20
                    _spriteBatch.Draw(_coinTexture, coinRectangle, Color.White);
                }
            }

            foreach (var enemy in _enemies)
			{
				_spriteBatch.Draw(_enemyTexture, enemy.Position, Color.White);
			}

			_spriteBatch.Draw(_playerTexture, _player.Position, Color.White);
			_spriteBatch.DrawString(_font, $"Score: {_score}", new Vector2(10, 10), Color.White);
			_spriteBatch.DrawString(_font, $"Time: {TimeLimit - _elapsedTime:F1} seconds", new Vector2(10, 40), Color.White);

		}

		private void DrawGameOverMenu()
		{
			// Draw a semi-transparent background for the menu
			_spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, 800, 600), new Color(0, 0, 0, 150));

			// Draw the game-over message
			var gameOverSize = _font.MeasureString(_endMessage);
			_spriteBatch.DrawString(_font, _endMessage, new Vector2(400 - gameOverSize.X / 2, 150), Color.Red);

			// Display the score
			var scoreMessage = $"Score: {_score} coins collected";
			var scoreSize = _font.MeasureString(scoreMessage);
			_spriteBatch.DrawString(_font, scoreMessage, new Vector2(400 - scoreSize.X / 2, 250), Color.White);

			// Display the elapsed time
			var timeMessage = $"Time Elapsed: {_elapsedTime:F1} seconds";
			var timeSize = _font.MeasureString(timeMessage);
			_spriteBatch.DrawString(_font, timeMessage, new Vector2(400 - timeSize.X / 2, 300), Color.White);

			// Draw buttons (Restart and Exit)
			var buttonWidth = 200;
			var buttonHeight = 50;
			var restartButtonRect = new Rectangle(300, 400, buttonWidth, buttonHeight);
			var exitButtonRect = new Rectangle(300, 500, buttonWidth, buttonHeight);

			_spriteBatch.Draw(_platformTexture, restartButtonRect, Color.DarkGray);
			_spriteBatch.Draw(_platformTexture, exitButtonRect, Color.DarkGray);

			var restartMessage = "Restart";
			var restartTextSize = _font.MeasureString(restartMessage);
			_spriteBatch.DrawString(_font, restartMessage, new Vector2(
				restartButtonRect.X + (buttonWidth - restartTextSize.X) / 2,
				restartButtonRect.Y + (buttonHeight - restartTextSize.Y) / 2), Color.White);

			var exitMessage = "Exit";
			var exitTextSize = _font.MeasureString(exitMessage);
			_spriteBatch.DrawString(_font, exitMessage, new Vector2(
				exitButtonRect.X + (buttonWidth - exitTextSize.X) / 2,
				exitButtonRect.Y + (buttonHeight - exitTextSize.Y) / 2), Color.White);

		}
	}
}


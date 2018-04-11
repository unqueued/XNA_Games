using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace CS3113Breakout
{
    /// <summary>
    /// The player will eliminate bricks on the screen
    /// When there are no more bricks, a new brickwall will be displayed
    /// 
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D paddleSprite;
        Paddle paddle;

        Texture2D ballSprite;
        Ball ball;

        Texture2D brickSprite;
        BrickWall brickWall;

        SpriteFont font;
        SpriteFont bigFont;
        SpriteFont smallFont;

        AudioEngine audioEngine;
        WaveBank waveBank;
        public static SoundBank soundBank;  // To be more accessable

        protected const int POINTSTOWIN = 60;
        protected const int BRICKSPERROW = 15;
        protected const int BRICKSPERCOL = 6;
        protected const int LIVESPERGAME = 3;

        KeyboardState lastState;


        public static Boolean paused = false;
        public static int points = 0;
        public static int lives = LIVESPERGAME;
        public static Boolean win = false;
        public static Boolean lose = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();


            // center should be fixed so that outerbound not need be set
            paddle.outerbound = new Rectangle(0,0, GraphicsDevice.Viewport.Width,GraphicsDevice.Viewport.Height);
            paddle.Center();

            ball.outerbound = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            ball.Center();
            ball.brickWall = brickWall;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Initialize paddle
            paddleSprite = Content.Load<Texture2D>(@"Sprites\paddle");
            paddle = new Paddle();
            paddle.sprite = paddleSprite;

            // Initialize ball
            ballSprite = Content.Load<Texture2D>(@"sprites\ball");
            ball = new Ball();
            ball.sprite = ballSprite;
            ball.paddle = paddle;

            // Initialize bricks
            brickSprite = Content.Load<Texture2D>(@"sprites\brick");
            brickWall = new BrickWall(brickSprite, BRICKSPERROW, BRICKSPERCOL,
                new Rectangle(
                        20, 60, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height
                        )
                        );

            // Load our fonts
            font = Content.Load<SpriteFont>(@"sprites\gamefont");
            bigFont = Content.Load<SpriteFont>(@"sprites\bigfont");
            smallFont = Content.Load<SpriteFont>(@"sprites\smallfont");

            // Load some stock audio effects
            audioEngine = new AudioEngine(@"Content\Audio\AnotherAudio.xgs");
            waveBank = new WaveBank(audioEngine, @"Content\Audio\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, @"Content\Audio\Sound Bank.xsb");

            // Make our cursor visible
            this.IsMouseVisible = true;

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyUp(Keys.R) && lastState.IsKeyDown(Keys.R))
            {
                Reset();
                Console.WriteLine("Reset has been called " + win + " " + lose);
            }

            if (!win && !lose)
            {
                if (keyboardState.IsKeyUp(Keys.P) && lastState.IsKeyDown(Keys.P))
                {
                    paused = !paused;
                }

                // If our game state allows it, then update the differnet objects
                if (!paused)
                {
                    paddle.Update();
                    ball.Update();
                }

                // Test for losing state
                if (lives < 1)
                    lose = true;
                // Test for winning case
                if (points >= POINTSTOWIN)
                {
                    win = true;
                }
            }

            lastState = keyboardState;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            if (!win && !lose)
            {
                paddle.Draw(spriteBatch);
                ball.Draw(spriteBatch);
                brickWall.Draw(spriteBatch);
            }
            else
            {
                if (win)
                {
                    CenterText(spriteBatch, bigFont, 
                        "Congratulations! You win!", Color.Pink, 
                        GraphicsDevice.Viewport.Width / 2, 220);
                    CenterText(spriteBatch, font, "Press R to reset the game",
                        Color.Pink, GraphicsDevice.Viewport.Width / 2, 300);
                }
                else
                {
                    CenterText(spriteBatch, bigFont, 
                        "Game Over", Color.Pink, 
                        GraphicsDevice.Viewport.Width / 2, 220);
                    CenterText(spriteBatch, font, "Press R to reset the game",
                        Color.Pink, GraphicsDevice.Viewport.Width / 2, 300);
                }
            }

            // Display pause text, if paused
            if (paused)
                CenterText(spriteBatch, bigFont, "Paused", Color.Pink, GraphicsDevice.Viewport.Width / 2, 200);

            // Draw status screen
            spriteBatch.DrawString(font, "Score: " + points + " out of " + POINTSTOWIN,
                new Vector2(0, 0), Color.Pink);
            spriteBatch.DrawString(font, "Lives: " + lives, new Vector2(0, 20), Color.Pink);

            // Draw help screen
            string instructions0 = "Instructions:";
            spriteBatch.DrawString(smallFont, instructions0,
                new Vector2(320, 0), 
                Color.Pink);
            string instructions1 = "Gain " + POINTSTOWIN +
                " points by hitting bricks without running out of lives";
            spriteBatch.DrawString(smallFont, instructions1,
                new Vector2(320, 12),
                Color.Pink);
            string instructions2 = "R key to reset game, p key to pause, arrows to move";
            spriteBatch.DrawString(smallFont, instructions2,
                new Vector2(320, 24), Color.Pink);

            
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void CenterText(SpriteBatch spriteBatch, SpriteFont font, string text, Color color, int x, int y) {
            spriteBatch.DrawString(font, text,
                new Vector2(
                    x - font.MeasureString(text).Length() / 2,
                    y
                    ), color);
        }

        public void Reset()
        {
            ball.Reset();
            paddle.Center();
            brickWall.Reset();
            win = false;
            lose = false;
            paused = false;
            points = 0;
            lives = LIVESPERGAME;
        }

        public class Entity
        {
            public Texture2D sprite { get; set; }
            public Color color { get; set; }

            protected Random random = new Random(); // a new random instance for each object

        }

        /*
         * 
         */
        public class Ball : Entity
        {
            protected const int DEFAULTSPEED = 2;
            private Vector2 direction;
            public Rectangle outerbound { get; set;}
            public Paddle paddle { get; set; }
            public float speed = DEFAULTSPEED;
            private Vector2 _position;
            private Vector2 _prev_position;
            protected float size = 15;             // the pixel size of the ball
            public BrickWall brickWall { get; set; }
            public Boolean Paddled { get; set; }

            public Point position_point
            {
                get
                {
                    return new Point( (int)_position.X, (int)_position.Y );
                }
            }

            public Point prev_position_point
            {
                get
                {
                    return new Point( (int)_prev_position.X, (int)_prev_position.Y );
                }
            }

            public Ball()
            {
                color = Color.CornflowerBlue;

                Center();
                SetRandomDirection();

                Paddled = false;
            }

            // Set our vector in a random direction
            public void SetRandomDirection()
            {
                int degreeAngle = random.Next(15, 45);
                direction = new Vector2(
                    (float)Math.Cos(MathHelper.ToRadians(degreeAngle)),
                    (float)Math.Sin(MathHelper.ToRadians(degreeAngle))
                    );
                direction.Normalize();
            }

            // This method resets the other values of the ball
            public void Reset()
            {
                Center();
                speed = DEFAULTSPEED;
                SetRandomDirection();
            }

            // This method centers the ball to its default position
            public void Center()
            {
                _position = new Vector2(outerbound.Width / 2, (outerbound.Height / 2) + 50);
            }

            // This method changes the direction of the ball when it comes into contact with the paddle.
            // If the ball hits the middle 50% of the paddle, it is reflected.
           //  If the ball hits the left or right 25% of the paddle, it is deflected and has 15 degrees
            // added or subtracted from its direction vector

            public void BouncePaddle()
            {
                soundBank.PlayCue("fire");
                // Dividing the paddle between left and right side
                Rectangle leftPaddleBounds = new Rectangle(paddle.bounds.Left, paddle.bounds.Top, 
                    paddle.bounds.Width / 3, paddle.bounds.Height);
                Rectangle rightPaddleBounds = new Rectangle(paddle.bounds.Left + (paddle.bounds.Width / 3) * 2,
                    paddle.bounds.Top, paddle.bounds.Width / 3, paddle.bounds.Height);

                float angle;

                angle = (int)MathHelper.ToDegrees((float)Math.Atan2(direction.Y, direction.X));
                
                // Modify the angle in some way
                if (leftPaddleBounds.Contains(position_point))
                {
                    angle = (int)MathHelper.ToDegrees((float)Math.Atan2(direction.X, direction.Y));
                    direction.Y = -direction.Y;
                    angle = (int)MathHelper.ToDegrees((float)Math.Atan2(direction.X, direction.Y));

                    angle += 15;

                    direction = new Vector2(
                        (float)Math.Sin(MathHelper.ToRadians(angle)),
                        (float)Math.Cos(MathHelper.ToRadians(angle))
                        );
                }
                else if (rightPaddleBounds.Contains(position_point))
                {
                    angle = (int)MathHelper.ToDegrees((float)Math.Atan2(direction.X, direction.Y));
                    direction.Y = -direction.Y;
                    angle = (int)MathHelper.ToDegrees((float)Math.Atan2(direction.X, direction.Y));

                    angle -= 15;

                    direction = new Vector2(
                        (float)Math.Sin(MathHelper.ToRadians(angle)),
                        (float)Math.Cos(MathHelper.ToRadians(angle))
                        );
                }
                else
                {
                    direction.Y = -direction.Y;
                }

                Paddled = true;
            }

            public void Draw(SpriteBatch s)
            {
                //s.Draw(sprite,new Rectangle((int)_position.X, (int)_position.Y, (int)size, (int)size), color);
                s.Draw(sprite, new Rectangle(
                        (int)(_position.X - size/2),
                        (int)(_position.Y - size/2),
                        (int)size,
                        (int)size),
                    color);
            }

            public void Update()
            {
                // Bounce the ball if we go outside the game window
                if(!outerbound.Contains(position_point)) {
                    if ((_position.X > outerbound.Width) || (_position.X < 0)) 
                        direction.X = -direction.X;
                    if (_position.Y < 0)
                        direction.Y = -direction.Y;
                    if (_position.Y > outerbound.Height)
                    {
                        lives--;
                        Reset();
                        paddle.Center();
                    }
                }

                // Detect if we've hit the paddle
                // The Paddled flag is used to make sure that the ball doesn't get detected until
                // it is clear of the paddle
                if (paddle.bounds.Contains(position_point))
                {
                    if (!Paddled)
                        BouncePaddle();
                }
                else
                {
                    Paddled = false;
                }

                // Detect if we've hit a brick
                Brick[,] brickList = this.brickWall.getBrickList;
                foreach(Brick i in brickList) {
                    if (i.Alive)
                    {
                        // Does the brick contain() the ball's point?
                        // If so, then immediately kill the brick and increment points.
                        // The reason the ball's point is checked is because a Point can't ever be within
                        // more than one non-overlapping Rectangle.
                        // If it does contain the ball, check whether the dimensions of the Rectangle of intersection
                        // to determine which direction to bounce the ball in.


                        // Increase the size of the ball's rectangle.s

                        if (i.Location.Contains(position_point))
                        {
                            // We have a hit
                            i.kill();
                            soundBank.PlayCue("Explo4");
                            points++;

                            // Simpler collision detection algorithm
                            Rectangle ballBounds = new Rectangle(position_point.X - 15, position_point.Y - 15,
                                30, 30
                                );
                            Rectangle collision = Rectangle.Intersect(i.Location, ballBounds);

                            // Alternate collision detection
                            /*
                            Rectangle prevRectangle =
                                new Rectangle(position_point.X, position_point.Y, prev_position_point.X, prev_position_point.Y);
                            Rectangle collision = Rectangle.Intersect(
                                i.Location, prevRectangle
                                );

                            */

                            if (collision.Width >= collision.Height)
                                direction.Y = -direction.Y;
                            else
                                direction.X = -direction.X;


                        }
                    }
                }

                _prev_position = _position;
                _position.X += (direction.X * speed);
                _position.Y += (direction.Y * speed);
            }
        }

        public class Paddle : Entity
        {
            public Rectangle outerbound { get; set; }
            public Rectangle bounds { get; set; }
            private Vector2 _velocty;

            public Paddle()
            {
                Center();
                color = Color.CornflowerBlue;
                _velocty = new Vector2();
            }

            public void Center()
            {   
                // Needs to be done from the outside
                this.bounds = new Rectangle(400, 420, 100, 50);
                bounds = new Rectangle(
                    (outerbound.Width / 2) - (bounds.Width/2),
                    bounds.Y,
                    bounds.Width,
                    bounds.Height
                    );
            }

            // Where paddle logic is processed
            // this includes input and collision detections
            public void Update()
            {
                KeyboardState keyboardState = Keyboard.GetState();

                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    bounds = new Rectangle(
                        (int)MathHelper.Clamp(bounds.X - 5, 0, outerbound.Width - bounds.Width),
                        bounds.Y, bounds.Width, bounds.Height);
                    _velocty = new Vector2(-2, 0);
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    bounds = new Rectangle(
                        (int)MathHelper.Clamp(bounds.X + 5, 0, outerbound.Width - bounds.Width),
                        bounds.Y, bounds.Width, bounds.Height);
                    _velocty = new Vector2(2, 0);
                }
                if (!keyboardState.IsKeyDown(Keys.Right) && !keyboardState.IsKeyDown(Keys.Left))
                {
                    _velocty = new Vector2();
                }

            }

            public Vector2 Velocity
            {
                get
                {
                    return _velocty;
                }
            }

            public void Draw(SpriteBatch s)
            {
                s.Draw(sprite, bounds, color);
            }
        }
    }
    /* 
     * The purpose of the BrickWall class is to assist with managing the bricks.
     * For example, some powerups may alter the properties of the ball, or the surrounding bricks
     * We may want to be able to query the matrix of bricks, and not have to worry about array management
     * or iteration.
     * This class provide methods so that a brick can return a list of pointers to surrounding bricks, 
     * which can then be acted upon.
     * 
     */
    public class BrickWall
    {
        private Brick[,] _bricks;
        protected Texture2D brickSprite;
        protected int wallWidth;
        protected int wallHeight;
        private Rectangle _brickWindow;

        public BrickWall(Texture2D brickSprite, int wallWidth, int wallHeight, Rectangle brickWindow)
        {
            this.brickSprite = brickSprite;
            this.wallWidth = wallWidth;
            this.wallHeight = wallHeight;
            _brickWindow = brickWindow;

            //int padding = 5;
            int padding = 1;
            int leftPadding = _brickWindow.X;
            int topPadding = _brickWindow.Y;

            // initialize our bricks
            _bricks = new Brick[wallWidth, wallHeight];
            for (int x = 0; x < wallWidth; x++)
            {
                for (int y = 0; y < wallHeight; y++)
                {
                    _bricks[x, y] = new Brick(brickSprite,
                        new Rectangle(
                               (x * brickSprite.Width) + (padding * x) + leftPadding,
                               (y * brickSprite.Height) + (padding * y) + topPadding,
                               brickSprite.Width,
                               brickSprite.Height
                            ),
                        Color.Red);
                }
            }

        }

        public void Reset()
        {
            foreach(Brick i in _bricks) {
                i.reset();
            }
        }

        public int BrickCount
        {
            get
            {
                int brickCount = 0;
                foreach (Brick i in _bricks)
                    if (i.Alive)
                        brickCount++;
                return brickCount;
            }
        }

        public Brick[,] getBrickList
        {
            get
            {
                return _bricks;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Brick i in _bricks)
            {
                i.Draw(spriteBatch);
            }
        }
    }

    public class Brick
    {
        //TODO change access for alive bakc
        private Texture2D _sprite;
        private Color _color;
        private Rectangle _location;
        private Boolean _alive;

        public Brick(Texture2D sprite, Rectangle location, Color color)
        {
            _sprite = sprite;
            _color = color;
            _location = location;
            _alive = true;
        }

        public void reset()
        {
            _alive = true;
        }

        public Texture2D Sprite
        {
            get
            {
                return _sprite;
            }
        }

        public Boolean Alive
        {
            get
            {
                return _alive;
            }
        }

        public void kill()
        {
            this._alive = false;
        }

        public Rectangle Location
        {
            get
            {
                return _location;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_alive == true)
            {
                spriteBatch.Draw(_sprite, _location, _color);
            }
        }
    }
}

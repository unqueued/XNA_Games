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

namespace Homework6
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static Random random = new Random();
        public static Viewport viewport;

        KeyboardState curKeyboardState;
        KeyboardState lastKeyBoardState;

        SpriteFont smallFont;
        SpriteFont medFont;
        SpriteFont largeFont;

        Ship ship;

        Texture2D missileSprite;
        Texture2D astroidSprite0;
        Texture2D astroidSprite1;
        Texture2D boomSprite;

        public static bool yellowAlert = false;
        public static bool redAlert = false;

        public static int lives = 4;
        public static bool gameOver = false;
        public static bool gameWon = false;
        public static bool gamePaused = false;

        public static TimeSpan lastHitTime;             // Tracks last time the ship was hit
        public static int lastHitCooldown = 2;

        public static TimeSpan lastTeleportTime;
        public static int lastTeleportCooldown = 5;

        string message = "";

        //public static List<Entity> entityList = new List<Entity>();
        public static List<Entity> deleteList = new List<Entity>();

        public static List<Projectile> projectileList = new List<Projectile>();
        public static List<Astroid> astroidList = new List<Astroid>();
        public static List<Boom> boomList = new List<Boom>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            this.IsMouseVisible = true;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            smallFont = Content.Load<SpriteFont>(@"smallfont");
            medFont = Content.Load<SpriteFont>(@"medfont");
            largeFont = Content.Load<SpriteFont>(@"largefont");

            Texture2D shipSprite = Content.Load<Texture2D>(@"ship0");

            missileSprite = Content.Load<Texture2D>(@"missile");
            astroidSprite0 = Content.Load<Texture2D>(@"astroid0");
            astroidSprite1 = Content.Load<Texture2D>(@"astroid1");
            boomSprite = Content.Load<Texture2D>(@"boom0");



            viewport = graphics.GraphicsDevice.Viewport;

            
            ship = new Ship(shipSprite);

            newGame();
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void newGame()
        {
            // Set up the initial game environment

            astroidList.Clear();

            for (int i = 0; i < 6; i++)
            {
                gamePaused = false;
                gameWon = false;
                gameOver = false;

                Astroid a = new Astroid(astroidSprite0);
                a.Position = new Vector2((float)(random.NextDouble() * viewport.Width), (float)(random.NextDouble() * viewport.Height));
                a.Rotation = (float)(random.NextDouble() * 6.2);
                astroidList.Add(a);
            }

            ship.Velocity = new Vector2();
            ship.teleport(true);
            lives = 4;
        }

        public void fire(GameTime gameTime)
        {
            Projectile p = new Projectile(missileSprite, ship, gameTime.TotalGameTime);
            projectileList.Add(p);
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
            curKeyboardState = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();


            // Input

            if (!gamePaused && !gameOver && !gameWon)
            {
                if (curKeyboardState.IsKeyDown(Keys.Left))
                {
                    ship.Rotation -= ship.RotationRate;
                }
                if (curKeyboardState.IsKeyDown(Keys.Right))
                {
                    ship.Rotation += ship.RotationRate;
                }
                if (curKeyboardState.IsKeyDown(Keys.Up))
                {
                    ship.thrust();
                }
                if (curKeyboardState.IsKeyUp(Keys.Space) && lastKeyBoardState.IsKeyDown(Keys.Space))
                {
                    fire(gameTime);
                }
                if (curKeyboardState.IsKeyUp(Keys.T) && lastKeyBoardState.IsKeyDown(Keys.T))
                {
                    ship.teleport(false);
                }
            }
            
            if(curKeyboardState.IsKeyUp(Keys.N) && lastKeyBoardState.IsKeyDown(Keys.N)) {
                newGame();
            }

            if (curKeyboardState.IsKeyUp(Keys.P) && lastKeyBoardState.IsKeyDown(Keys.P))
            {
                gamePaused = !gamePaused;
            }


            // Do logic for each entity

            if (!gamePaused)
            {
                // Check for winning and losing state

                // Winning will have priority over losing
                // If the winning conditions are satisfied for both winning and losing, the player will win
                if (astroidList.Count < 1)
                {
                    gameWon = true;
                }

                if (!gameWon)
                {
                    if (lives < 1)
                    {
                        gameOver = true;
                    }
                }

                // Ship
                if(!gameWon && !gameOver)
                    ship.Update();

                // Projectiles
                foreach (Projectile p in projectileList)
                {
                    p.Update();
                    if (p.expired(gameTime.TotalGameTime))
                    {
                        deleteList.Add(p);
                    }
                }

                foreach (Projectile p in deleteList)
                {
                    if (projectileList.Contains(p))
                    {
                        projectileList.Remove(p);
                        //boomList.Add(new Boom(boomSprite, p));
                    }

                }

                // Astroids
                List<Astroid> splitList = new List<Astroid>();
                List<Projectile> projectileDeleteList = new List<Projectile>();

                foreach (Astroid a in astroidList)
                {
                    a.Update();

                    if(RotatedSpritesCollide((Entity)ship, (Entity)a))
                    {
                        if (IntersectPixels(ship.getTransform(), ship.Sprite.Width, ship.Sprite.Height, ship.SpriteData,
                            a.getTransform(), a.Sprite.Width, a.Sprite.Height, a.SpriteData))
                        {
                            ship.Velocity += a.Velocity;
                            if ((lastHitTime.Seconds + 1) < (gameTime.TotalGameTime.Seconds))
                            {
                                lives--;
                                lastHitTime = gameTime.TotalGameTime;
                            }
                        }
                    }

                    foreach (Projectile p in projectileList)
                    {
                        if(RotatedSpritesCollide((Entity)p, (Entity)a))
                        {
                            if (IntersectPixels(p.getTransform(), p.Sprite.Width, p.Sprite.Height, p.SpriteData,
                                a.getTransform(), a.Sprite.Width, a.Sprite.Height, a.SpriteData))
                            {
                                splitList.Add(a);
                                a.Velocity = p.Velocity / 4;
                                projectileDeleteList.Add(p);
                            }

                        }

                    }

                }

                // Splitting astroids that are found to have collided
                foreach (Astroid a in splitList)
                {
                    if (astroidList.Contains(a))
                    {
                        // Remove big astroid
                        astroidList.Remove(a);

                        // Add two little ones
                        for (int i = 0; i < 2; i++)
                        {
                            if (a.canBreak)
                            {
                                Astroid newAstriod = new Astroid(astroidSprite1, a, gameTime.TotalGameTime);
                                newAstriod.canBreak = false;

                                newAstriod.Velocity = new Vector2(
                                    (float)(newAstriod.Velocity.X + (newAstriod.Velocity.X * (random.NextDouble() * 2))),
                                    (float)(newAstriod.Velocity.Y + (newAstriod.Velocity.Y * (random.NextDouble() * 2))));
                                astroidList.Add(newAstriod);
                            }
                        }
                    }
                }

                foreach (Projectile p in projectileDeleteList)
                {
                    if (projectileList.Contains(p))
                    {
                        projectileList.Remove(p);
                    }
                }
            }

            message = "";
            message += "CS3113 Astroids\n";
            message += "(N) New game\n";
            message += "(P) Pause\n";
            message += "(T) Teleport\n";

            lastKeyBoardState = curKeyboardState;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (yellowAlert)
                GraphicsDevice.Clear(Color.Yellow);


            spriteBatch.Begin();
            // TODO: Add your drawing code here

            //foreach (Entity e in entityList)
            //{
                //spriteBatch.Draw(e.Sprite, e.Location, Color.White);
                //spriteBatch.Draw(e.Sprite, e.Location, null, Color.White, 0.0f, e.Position, SpriteEffects.None, 0.0f);
                //spriteBatch.Draw(e.Sprite, e.Location, null, Color.White, e.Rotation, e.Origin, SpriteEffects.None, 0.0f);
            //}

            // Ship
            if(!gameOver && !gameWon)
                spriteBatch.Draw(ship.Sprite, ship.Location, null, Color.White, ship.Rotation, ship.Origin, SpriteEffects.None, 0.0f);

            // Projectiles
            foreach (Projectile p in projectileList)
            {
                spriteBatch.Draw(p.Sprite, p.Location, null, Color.White, p.Rotation, p.Origin, SpriteEffects.None, 0.0f);
            }

            // Explosions
            foreach (Boom b in boomList)
            {
                spriteBatch.Draw(b.Sprite, b.Location, Color.White);
            }

            // Astroids
            foreach (Astroid a in astroidList)
            {
                spriteBatch.Draw(a.Sprite, a.Location, null, Color.White, a.Rotation, a.Origin, SpriteEffects.None, 0.0f);
            }

            // Display messages
            spriteBatch.DrawString(medFont, message, new Vector2(0, 0), Color.White);

            spriteBatch.DrawString(medFont, "Lives: ", new Vector2(600, 10), Color.White);
            for (int i = 0; i < lives; i++)
            {
                spriteBatch.Draw(ship.Sprite,
                    new Rectangle(680 + (ship.Sprite.Width * i), ship.Sprite.Height / 2 + 10, ship.Sprite.Width, ship.Sprite.Height),
                    null, Color.White, MathHelper.ToRadians(270), ship.Origin, SpriteEffects.None, 0.0f);
            }

            if (gamePaused)
            {
                CenterText(spriteBatch, largeFont, "Paused", Color.White, viewport.Width / 2, viewport.Height / 2);
            }
            else if (gameWon)
            {
                CenterText(spriteBatch, largeFont, "You won!", Color.White, viewport.Width / 2, viewport.Height / 2);
                CenterText(spriteBatch, medFont, "Press N for new game", Color.White, (int)viewport.Width / 2, 10);
            }
            else if (gameOver)
            {
                CenterText(spriteBatch, largeFont, "Game over!", Color.White, viewport.Width / 2, viewport.Height / 2);
                CenterText(spriteBatch, medFont, "Press N for new game", Color.White, (int)viewport.Width / 2, 10);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        // Used for centering text
        public void CenterText(SpriteBatch spriteBatch, SpriteFont font, string text, Color color, int x, int y)
        {
            spriteBatch.DrawString(font, text,
                new Vector2(
                    x - font.MeasureString(text).Length() / 2,
                    y
                    ), color);
        }

        public static bool RotatedSpritesCollide(Entity first, Entity second)
        {
            Rectangle firstAdjustedRectangle = CalculateBoundingRectangle(
                new Rectangle(0, 0, (int)first.Sprite.Width, (int)first.Sprite.Height),
                first.getTransform()
                );

            Rectangle secondAdjustedRectangle = CalculateBoundingRectangle(
                new Rectangle(0, 0, (int)second.Sprite.Width, (int)second.Sprite.Height),
                second.getTransform()
                );

            if (firstAdjustedRectangle.Intersects(secondAdjustedRectangle))
            {
                return true;
            }
            return false;
        }


        public static Rectangle CalculateBoundingRectangle(Rectangle rectangle,
                                                           Matrix transform)
        {
            Vector2 leftTop = new Vector2(rectangle.Left, rectangle.Top);
            Vector2 rightTop = new Vector2(rectangle.Right, rectangle.Top);
            Vector2 leftBottom = new Vector2(rectangle.Left, rectangle.Bottom);
            Vector2 rightBottom = new Vector2(rectangle.Right, rectangle.Bottom);

            Vector2.Transform(ref leftTop, ref transform, out leftTop);
            Vector2.Transform(ref rightTop, ref transform, out rightTop);
            Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
            Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                      Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                      Vector2.Max(leftBottom, rightBottom));

            return new Rectangle((int)min.X, (int)min.Y,
                                 (int)(max.X - min.X), (int)(max.Y - min.Y));
        }


        public static bool IntersectPixels(
                            Matrix transformA, int widthA, int heightA, Color[] dataA,
                            Matrix transformB, int widthB, int heightB, Color[] dataB)
        {
            Matrix transformAToB = transformA * Matrix.Invert(transformB);

            Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transformAToB);
            Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transformAToB);

            Vector2 yPosInB = Vector2.Transform(Vector2.Zero, transformAToB);

            for (int yA = 0; yA < heightA; yA++)
            {
                Vector2 posInB = yPosInB;

                for (int xA = 0; xA < widthA; xA++)
                {
                    int xB = (int)Math.Round(posInB.X);
                    int yB = (int)Math.Round(posInB.Y);

                    if (0 <= xB && xB < widthB &&
                        0 <= yB && yB < heightB)
                    {
                        Color colorA = dataA[xA + yA * widthA];
                        Color colorB = dataB[xB + yB * widthB];

                        if (colorA.A != 0 && colorB.A != 0)
                        {
                            return true;
                        }
                    }

                    posInB += stepX;
                }

                yPosInB += stepY;
            }

            return false;
        }

        // This class will provide most of the abstract properties of all of the entities used in the game
        public class Entity
        {
            protected Texture2D _sprite;
            protected Vector2 _position;
            protected Vector2 _heading;
            protected Vector2 _velocity;
            protected float _rotation = 0.0f;
            protected float _rotationRate = 0.04f;
            protected TimeSpan createdTime;
            protected float lifeTimeSeconds = 2.3f;
            protected Color[] spriteData;

            public Entity()
            {
                
            }

            public Entity(Texture2D sprite) : this()
            {
                _sprite = sprite;

                spriteData = new Color[sprite.Width * sprite.Height];
                sprite.GetData(spriteData);
            }

            // This constructor extracts movement information from its parent and
            // uses it
            public Entity(Texture2D sprite, Entity parent) : this(sprite)
            {
                if (parent != null)
                {
                    Velocity = parent.Velocity;
                    Position = parent.Position;
                }
            }

            public Entity(Texture2D sprite, Entity parent, TimeSpan gameTime)
                : this(sprite, parent)
            {
                this.createdTime = gameTime;
            }

            public virtual void Update()
            {
                Position += Velocity;
            }

            public Color[] SpriteData { get { return spriteData; } }

            public Rectangle Location
            {
                get
                {
                    return new Rectangle((int)Position.X, (int)Position.Y, Sprite.Width, Sprite.Height);
                }
            }

            public Texture2D Sprite {
                get
                {
                    return _sprite;
                }
                set
                {
                    _sprite = value;
                }
            }

            // This property will make sure that the position of the entity is on screen
            // at all times
            public Vector2 Position {
                get
                {
                    return _position;
                }
                set
                {
                    if (value.X > viewport.Width)
                        _position.X = 0;
                    else if (value.X < 0)
                        _position.X = viewport.Width;
                    else
                        _position.X = value.X;

                    if (value.Y > viewport.Height)
                        _position.Y = 0;
                    else if (value.Y < 0)
                        _position.Y = viewport.Height;
                    else
                        _position.Y = value.Y;

                }
            }

            public Vector2 Heading
            {
                get
                {
                    return new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation));
                }
                set
                {
                    _heading = value;
                }
            }

            public virtual Vector2 Velocity
            {
                get
                {
                    return _velocity;
                }
                set
                {
                    _velocity.X = MathHelper.Clamp(value.X, -3, 3);
                    _velocity.Y = MathHelper.Clamp(value.Y, -3, 3);
                }
            }

            // This property will make sure that rotation is never
            // represented outside of 0-360 degrees
            public float Rotation
            {
                get
                {
                    return _rotation;
                }
                set
                {
                    if (value > Math.PI * 2)
                    {
                        _rotation = (float)value % ((float)Math.PI * 2.0f);
                    }
                    else if (value < 0)
                    {
                        _rotation = (float)Math.Abs(value) + (float)Math.PI * 2.0f;
                    }
                    else
                    {
                        _rotation = value;
                    }
                }
            }


            public float RotationRate
            {
                get
                {
                    return _rotationRate;
                }
                set
                {
                    _rotationRate = value;
                }
            }

            public Vector2 Origin
            {
                get
                {
                    return new Vector2(_sprite.Width / 2, _sprite.Height / 2);
                }
            }

            public virtual void die() {
                deleteList.Add(this);
            }

            public virtual bool expired(TimeSpan currentTime)
            {
                return false;
            }

            // Get the transformation matrix, for collision detection
            public Matrix getTransform()
            {
                Matrix newTransform;
                newTransform =
                    Matrix.CreateTranslation(new Vector3(-Origin, 0.0f));
                newTransform *=
                    Matrix.CreateRotationZ(Rotation);
                newTransform *=
                    Matrix.CreateTranslation(new Vector3(Position, 0.0f));

                return newTransform;
            }
        }

        public class Boom : Entity
        {
            public Boom(Texture2D sprite, Entity parent)
                : base(sprite, parent)
            {
                Velocity = new Vector2(0f);
            }

            public override bool expired(TimeSpan currentTime)
            {
                if (currentTime.Seconds > (createdTime.Seconds + lifeTimeSeconds))
                {
                    return true;
                }
                return false;
            }
        }


        public class Ship : Entity
        {
            private float acceleration = .02f;
            
            public Ship(Texture2D sprite) : base(sprite) { }

            // Places the ship somewhere on the gameboard.
            // If safe flag is set, will make sure that there are no collisions before placing
            public bool teleport(bool safe)
            {

                _position.X = (float)random.Next(0, viewport.Width);
                _position.Y = (float)random.Next(0, viewport.Height);
                Rotation = (float)(random.NextDouble() * (Math.PI * 2));

                return true;
            }

            // Transfer some of the heading vector into the velocity vector
            public void thrust()
            {
                Velocity += (Heading * acceleration);
            }
        }


        public class Astroid : Entity
        {
            public Astroid(Texture2D sprite, Entity parent, TimeSpan createdTime)
                : base(sprite, parent, createdTime)
            {
                
            }

            public Astroid(Texture2D sprite, TimeSpan createdTime) : this(sprite)
            {
                this.createdTime = createdTime;
            }

            public Astroid(Texture2D sprite) : base(sprite)
            {
                _rotationRate = (float)(random.NextDouble() * 0.04f);
                Velocity = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
            }

            // Whether this astroid will split, or be obliterated
            public bool canBreak = true;

            public override void Update()
            {
                base.Update();

                _rotation += _rotationRate;
            }
        }

        // This is the only entity that expires after a certain number of seconds in game
        // so, it needs to be checked in on with its expired() method every so often
        public class Projectile : Entity
        {
            private float totalProjectiles = 5;
            private float lifetime = 2.3f; // Total distance a missile travels before dying
            private Vector2 _startingPosition;

            public Projectile(Texture2D sprite, Entity parent, TimeSpan createdTime) : base(sprite)
            {
                if (projectileList.Count > totalProjectiles)
                {
                    return;
                }

                base.lifeTimeSeconds = lifetime;
                this.createdTime = createdTime;

                Position = parent.Position;

                Rotation = parent.Rotation;

                Velocity = parent.Velocity;
                Velocity += (parent.Heading * 3);

                _startingPosition = parent.Position;
           }

            public override void Update()
            {
                base.Update();
            }

            public override void die()
            {
                base.die();
            }

            // Removing speed limits for projective velocity
            public override Vector2 Velocity
            {
                get
                {
                    return _velocity;
                }
                set
                {
                    _velocity = value;
                }
            }

            // This checks to see if the time that the projectile was fired has "expired"
            public override bool expired(TimeSpan currentTime)
            {
                if (currentTime.Seconds > (createdTime.Seconds + lifeTimeSeconds))
                {
                    return true;
                }
                return false;
            }

        }
    }
}

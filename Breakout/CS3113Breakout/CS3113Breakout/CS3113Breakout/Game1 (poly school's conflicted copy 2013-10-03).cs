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
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D paddleSprite;

        Paddle paddle;

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

            // TODO: use this.Content to load your game content here
            paddleSprite = Content.Load<Texture2D>(@"Sprites\paddle");
            paddle = new Paddle();
            paddle.Sprite = paddleSprite;

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

            // If our game state allows it, then update the differnet objects
            paddle.Update();

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
            //spriteBatch.Draw(paddle.Sprite, paddle.Size, paddle.Color);
            spriteBatch.Draw(paddle.Sprite, paddle.Size, paddle.Color);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public class Entity
        {
            // TODO make these a bit more restrictive
            public Texture2D Sprite { get; set; }
            public Rectangle Size { get; set; }
            public Point Location { get; set; } // Make this be the center of the rectangle
            public Color Color { get; set; }

        }
        public class Ball : Entity
        {
            
        }
        public class Paddle : Entity
        {

            public Paddle()
            {
                Size = new Rectangle(200, 300, 100, 100);
                Color = Color.Red;
            }

            public void Center()
            {   
                // Needs to be done from the outside
                Size = new Rectangle(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width/2, Size.Y, 100, 100);
            }

            // Where paddle logic is processed
            // this includes input and collision detections
            public void Update()
            {
                KeyboardState keyboardState = Keyboard.GetState();

                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    Center();
                }
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    Size = new Rectangle(Size.X - 5, Size.Y, Size.Width, Size.Height);
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    Size = new Rectangle(Size.X + 5, Size.Y, Size.Width, Size.Height);
                }
            }
        }
    }
}

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

namespace Infector
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KeyboardState lastKeyboardState;
        KeyboardState curKeyboardState;
        MouseState curMouseState;

        SpriteFont smallFont;
        SpriteFont medFont;
        SpriteFont largeFont;

        Texture2D placeHolderSprite;

        // Probably this is temporary
        private List<Entity> allEntities;

        DocileHuman loneHuman;

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
            Console.WriteLine("Loading content...");
            this.IsMouseVisible = true;


            allEntities = new List<Entity>();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            smallFont = Content.Load<SpriteFont>(@"smallfont");
            medFont = Content.Load<SpriteFont>(@"medfont");
            largeFont = Content.Load<SpriteFont>(@"largefont");

            placeHolderSprite = Content.Load<Texture2D>(@"placeholder");
            
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
            curMouseState = Mouse.GetState();

            if (loneHuman == null)
            {
                loneHuman = new DocileHuman(placeHolderSprite);
                loneHuman.Position = new Vector2(300, 300);
                allEntities.Add(loneHuman);
                loneHuman.Entities = allEntities;
            }

            if (curMouseState.LeftButton == ButtonState.Pressed)
            {

                //Console.WriteLine("Clicked");
                //Entity e = new Entity(placeHolderSprite);
                //e.Position = new Vector2(mouse.X, mouse.Y);
                //allEntities.Add(e);
                foreach (Entity e in allEntities)
                {
                    if (e is Zombie)
                    {
                        Zombie z = (Zombie)e;
                        Console.WriteLine(z.findClosestNonZombie());
                    }
                }
            }

            if (curKeyboardState.IsKeyUp(Keys.H) && lastKeyboardState.IsKeyDown(Keys.H))
            {
                DocileHuman human = new DocileHuman(placeHolderSprite);
                human.Position = new Vector2(curMouseState.X, curMouseState.Y);
                human.debugFont = smallFont;
                human.Entities = allEntities;
                allEntities.Add(human);
            }

            if (curKeyboardState.IsKeyUp(Keys.Z) && lastKeyboardState.IsKeyDown(Keys.Z))
            {
                Zombie z = new Zombie(placeHolderSprite);
                z.Position = new Vector2(curMouseState.X , curMouseState.Y);
                z.debugFont = smallFont;
                z.Entities = allEntities;
                allEntities.Add(z);
            }

            foreach (var e in allEntities)
            {
                e.update();
            }

            lastKeyboardState = Keyboard.GetState();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            spriteBatch.Begin();
            foreach (var e in allEntities)
            {
                //spriteBatch.Draw(e.Sprite, e.Location, Color.Red);
                e.draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

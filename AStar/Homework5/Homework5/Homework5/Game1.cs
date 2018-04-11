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

namespace Homework5
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static SpriteFont font;
        public static SpriteFont bigFont;
        public static SpriteFont smallFont;

        Texture2D tileSprite;

        KeyboardState oldKeyboard;

        public const int DISPLAYPADDING = 10;
        public const int INTERFACEHEIGHT = 200;
        public const int BUTTONMARGIN = 2;

        // These are the logical width and height of the game board
        public const int BOARDWIDTH = 10;
        public const int BOARDHEIGHT = 10;

        TileButton[,] tileButtons;
        TileNode[,] tileNodes;

        public static TileNode startNode = null;
        public static TileNode destinationNode = null;

        public static List<TileNode> openSet = new List<TileNode>();
        public static List<TileNode> closedSet = new List<TileNode>();
        public static List<TileNode> errorSet = new List<TileNode>();   // used for debugging
        public static List<TileNode> winSet;                            // the list to store nodes in path

        public static Boolean interactive = true;
        public static Boolean debug = false;
        public static Boolean pathFound = false;
        public static Boolean allPathsFound = false;
        public static Boolean unsolvable = false;
        public static Boolean won = false;          // Stop searching we found a path

        public static Boolean error = false;

        Rectangle playArea;         // This is the area in which the game is played
        Rectangle interfaceArea;    // This is the area in which messages are displayed

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 750;
            graphics.PreferredBackBufferHeight = 750 + INTERFACEHEIGHT;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization 
            base.Initialize();

            this.IsMouseVisible = true;
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
            font = Content.Load<SpriteFont>(@"sprites\gamefont");
            bigFont = Content.Load<SpriteFont>(@"sprites\bigfont");
            smallFont = Content.Load<SpriteFont>(@"sprites\smallfont");

            tileSprite = Content.Load<Texture2D>(@"sprites\tile");

            interfaceArea = new Rectangle(
                0 + DISPLAYPADDING,
                0 + DISPLAYPADDING,
                GraphicsDevice.Viewport.Width - (DISPLAYPADDING * 2),
                INTERFACEHEIGHT - (DISPLAYPADDING * 2));

            playArea = new Rectangle(
                0 + DISPLAYPADDING,
                INTERFACEHEIGHT + DISPLAYPADDING,
                GraphicsDevice.Viewport.Width - (DISPLAYPADDING * 2),
                GraphicsDevice.Viewport.Height - (interfaceArea.Height) - (DISPLAYPADDING  * 4)
                );


            // Create our clickable tile board
            int tileWidth = playArea.Width / BOARDWIDTH;
            int tileHeight = playArea.Height / BOARDHEIGHT;

            tileNodes = new TileNode[BOARDWIDTH, BOARDHEIGHT];
            tileButtons = new TileButton[BOARDWIDTH, BOARDHEIGHT];

            for (int yi = 0; yi < tileButtons.GetLength(1); yi++)
                for (int xi = 0; xi < tileButtons.GetLength(0); xi++)
                {
                    // Create our tilenodes
                    tileNodes[xi, yi] = new TileNode(xi, yi);

                    // And create TileButtons
                    tileButtons[xi, yi] = new TileButton(
                        new Rectangle(
                            (xi * tileWidth) + playArea.Left + BUTTONMARGIN,
                            (yi * tileHeight) + playArea.Top + BUTTONMARGIN,
                            tileWidth - BUTTONMARGIN,
                            tileHeight - BUTTONMARGIN
                            ),
                        tileSprite,
                        tileNodes[xi, yi]
                        );
                }

            // Now connect all of our tileNodes to each other
            for (int yi = 0; yi < tileNodes.GetLength(1); yi++)
            {
                for (int xi = 0; xi < tileNodes.GetLength(0); xi++)
                {
                    if(yi > 0)
                        tileNodes[xi, yi].Up = tileNodes[xi, yi - 1];
                    if(yi < tileNodes.GetLength(1) - 1)
                        tileNodes[xi, yi].Down = tileNodes[xi, yi + 1];
                    if(xi > 0)
                        tileNodes[xi, yi].Left = tileNodes[xi - 1, yi];
                    if(xi < tileNodes.GetLength(0) - 1)
                        tileNodes[xi, yi].Right = tileNodes[xi + 1, yi];
                }
            }
            Console.WriteLine();

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void ResetGame()
        {
            startNode = null;
            destinationNode = null;
            interactive = true;
            unsolvable = false;
            if(openSet != null)
                openSet.Clear();
            if(closedSet != null)
                closedSet.Clear();
            if(winSet != null)
                winSet.Clear();
            pathFound = false;
            foreach (TileNode i in tileNodes)
            {
                i.passable = true;
                i.DisplayStats = false;
            }
        }

        public void StartSearch()
        {
            if (startNode == null || destinationNode == null)
                return;

            interactive = false;

            // Check if won

            if (openSet.Count == 0)
            {
                unsolvable = true;
                return;
            }

            //Console.WriteLine("\n\nSearching...-----------");

            //printNodeList(openSet);

            TileNode current = getSmallNext(openSet);

            //Console.WriteLine("Extracted smallest: " + current);

            closedSet.Add(current);
            openSet.Remove(current);

            List<TileNode> neighbors = current.getNeighbors();

            
            foreach (TileNode i in neighbors)
            {
                if (!openSet.Contains(i))
                {
                    i.updateStats(current);
                    i.parent = current;
                    openSet.Add(i);
                }
                else
                {
                    //Console.Write("" + i + "Already in openlist ");
                    //Console.WriteLine("previous g() = " + i.g + " current g() = " + i.getManhattanDistance(current));
                    if(i.getManhattanDistance(current) < i.g) {
                        i.updateStats(current);
                        i.parent = current;
                    }
                    //Console.WriteLine("Updated, now g() = " + i.g);
                }
            }

            //Console.WriteLine("Removed currentnode, added neighbors");

            //printNodeList(openSet);
            //printNodeList(closedSet);

            // only try to find a path the first time around
            // Need to be done manually every time after that
            if (closedSet.Contains(destinationNode))
            {
                pathFound = true;
                winSet = buildPath();
            }
        }

        // Returns the next element, from smallest value
        // for f()
        // It is the users's responsibility to make sure that stats are up to date
        public TileNode getSmallNext(List<TileNode> l)
        {
            if (l.Count < 1)
                return null;
            
            // Set the first popped element to be returned, if we can't find
            // any elements who's value of f is smaller than it
            TileNode smallest = l[0];

            foreach (TileNode i in l)
                if (smallest.f > i.f)
                    smallest = i;

            return smallest;
        }

        // Prints out a list of TileNode elements in an array, for debugging purposes
        public void printNodeList(List<TileNode> l)
        {
            if (l == null)
            {
                Console.WriteLine("null nodelist detected");
                return;
            }
            Console.WriteLine("There are " + l.Count() + " elements:");
            foreach (TileNode i in l)
            {
                Console.Write("(" + i.x + ", " + i.y + ") : ");
                Console.Write("f() = " + i.f + " g() = " + i.g + " h() = " + i.h + " ");
                Console.Write("Parent: " + i.parent);
                Console.WriteLine();
            }
        }

        // Assemble a list of nodes that are a path, using most recent values
        public List<TileNode> buildPath()
        {
            if (!closedSet.Contains(destinationNode))
            {
                Console.WriteLine("No paths found");
                return null;
            }
            List<TileNode> path = new List<TileNode>();
            TileNode cursor = destinationNode;

            if (error)
                return null;

            int pathCount = 0;  // shouldn't need this
            while (cursor.parent != null && pathCount < 200)
            {
                pathCount++;
                path.Add(cursor.parent);
                cursor = cursor.parent;
            }
            return path;
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

            KeyboardState keyboard = Keyboard.GetState();

            keyboard = Keyboard.GetState();

            if (keyboard.IsKeyUp(Keys.D) && oldKeyboard.IsKeyDown(Keys.D))
            {
                //debug = !debug;
                Console.WriteLine("Destination node: " + destinationNode);
                printNodeList(openSet);
                printNodeList(closedSet);
            }

            //if (keyboard.IsKeyUp(Keys.W) && oldKeyboard.IsKeyDown(Keys.W))
            //{
                // manually detect winstate
                //winSet = buildPath();
            //}

            if (keyboard.IsKeyUp(Keys.Space) && oldKeyboard.IsKeyDown(Keys.Space))
            {
                interactive = false;
                while (!unsolvable && !pathFound)
                {
                    StartSearch();
                }
                interactive = true;
            }

            if (keyboard.IsKeyUp(Keys.R) && oldKeyboard.IsKeyDown(Keys.R))
            {
                ResetGame();
            }

            //if (keyboard.IsKeyUp(Keys.S) && oldKeyboard.IsKeyDown(Keys.S))
            if (keyboard.IsKeyDown(Keys.S))
            {
                StartSearch();
            }

            if (interactive)
            {
                foreach (TileButton i in tileButtons)
                    i.Update();
            }

            oldKeyboard = keyboard;

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

            drawInterface(spriteBatch);
            //spriteBatch.Draw(tileSprite, interfaceArea, Color.Beige);
            //spriteBatch.Draw(tileSprite, playArea, Color.Beige);

            foreach(TileButton i in tileButtons) {
                i.Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        void drawInterface(SpriteBatch spriteBatch)
        {
            string instructionsString = "";

            if (interactive)
            {
                instructionsString += "Instructions\n";

                instructionsString +=
                    "Left click: set start and end\n" +
                    "Right click: toggle barrier\n" +
                    "(R) Reset\n" +
                    "(S) Step\n" +
                    "(SPACE) Find path\n";

                spriteBatch.DrawString(font, instructionsString,
                    new Vector2(
                        interfaceArea.Left + 5,
                        interfaceArea.Top + 5),
                        Color.Beige);


                string statusString = "";

                statusString += "Starting Node:";
                if (startNode != null)
                    statusString += "(" + startNode.x + ", " + startNode.y + ")\n";
                else
                    statusString += "[None selected]\n";

                statusString += "Destination Node:";
                if (destinationNode != null)
                    statusString += "(" + destinationNode.x + ", " + destinationNode.y + ")\n";
                else
                    statusString += "[None selected]\n";

                if (unsolvable)
                    statusString +=
                        "Unsolvable, please\n hit (R) to reset\n";

                if (!pathFound)
                {
                    if (startNode != null && destinationNode != null)
                        statusString += "Press SPACE to find path\n";
                    else
                        statusString += "Start and end nodes not selected\n";
                } else {
                    statusString +=
                        "Path found!\n" +
                        "Hit S to continue searching\n" +
                        "for other paths";
            }

                spriteBatch.DrawString(font, statusString,
                    new Vector2(
                        interfaceArea.Left + 350,
                        interfaceArea.Top + 5),
                        Color.Beige);
            }
            else
            {
                instructionsString += "Instructions\n";

                instructionsString +=
                    "(R) Reset\n" +
                    "(S) Step\n";

                spriteBatch.DrawString(font, instructionsString,
                    new Vector2(
                        interfaceArea.Left + 5,
                        interfaceArea.Top + 5),
                        Color.Beige);

                string statusString = "";

                statusString += "Open List: " +
                    openSet.Count + "\n";

                statusString += "Closed List: " +
                    closedSet.Count + "\n";

                if (pathFound)
                    statusString +=
                        "Path found!\n" +
                        "Hit S to continue searching\n" +
                        "for other paths";
                if (unsolvable)
                    statusString +=
                        "Unsolvable,\n please hit (R) to reset\n";

                spriteBatch.DrawString(font, statusString,
                    new Vector2(
                        interfaceArea.Left + 350,
                        interfaceArea.Top + 5),
                        Color.Beige);

            }
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

        // TODO make these safer

        // The nodegraph which implements helps implement the logic of our program
        public class TileNode
        {
            private int _x;
            private int _y;
            private int _g;
            public TileNode previous { get; set; }
            public Boolean passable { get; set; }

            public Boolean Toggled { get; set; }       // Flag that will be removed later

            public TileNode Left { get; set; }
            public TileNode Right { get; set; }
            public TileNode Up { get; set; }
            public TileNode Down { get; set; }

            public TileNode parent = null;

            public int f
            {
                get
                {
                    return Math.Abs(g + h);
                }
            }

            public int g
            {
                get
                {
                    return _g;
                }
            }
            public int h
            {
                get
                {
                    if (destinationNode == null)
                        return 0;
                    else
                        return getManhattanDistance(destinationNode);
                }
            }

            public Boolean DisplayStats { get; set; }

            public int x { get { return _x; } }
            public int y { get { return _y; } }

            public TileNode(int x, int y)
            {
                this._x = x;
                this._y = y;
                previous = null;
                passable = true;
            }

            public override string ToString()
            {
                string output = "";
                output = output + "Node (" + _x + ", " + _y + ")\n"; 
                return base.ToString() + " : " + output;
            }

            // Returns a list of possible neighbors (excludes edges, non passable, and 
            // members of the closed set)
            public List<TileNode> getNeighbors()
            {
                List<TileNode> neighbors = new List<TileNode>();

                if (Left != null && Left.passable)
                    neighbors.Add(Left);

                if (Right != null && Right.passable)
                    neighbors.Add(Right);

                if (Up != null && Up.passable)
                    neighbors.Add(Up);
                
                if (Down != null && Down.passable)
                    neighbors.Add(Down);

                foreach (TileNode i in closedSet)
                    neighbors.Remove(i);

                return neighbors;
            }


            // Update our g() value, since it may change during the run
            public void updateStats(TileNode src)
            {
                DisplayStats = true;
                _g = getManhattanDistance(src);
            }

            public int getManhattanDistance(TileNode dest)
            {
                return getManhattanDistance(dest, this);
            }

            public int getManhattanDistance(TileNode dest, TileNode src)
            {
                int distance = 0;
                if (dest != null && src != null)
                {
                    distance = Math.Abs(src.x - dest.x);
                    distance += Math.Abs(src.y - dest.y);
                }
                return distance;
            }
        }


        // Tilebuttons are passive, and are only for capturing input, and displaying
        // the state of their associated tile.
        // They only get their texture, screen coordinates, and associated tile address at
        // creation.
        // After that, their other properties, such as tint, are controlled externally.
        public class TileButton
        {
            private Rectangle _screenLocation;
            private Texture2D _tile;

            MouseState mouse;
            MouseState oldMouse;
            Boolean clicked = false;
            //Boolean toggled = true;
            TileNode _tileNode;

            public TileButton(Rectangle _screenLocation, Texture2D _tile, TileNode _tileNode)
            {
                this._screenLocation = _screenLocation;
                this._tile = _tile;
                this._tileNode = _tileNode;
            }

            public void Update()
            {
                mouse = Mouse.GetState();


                // TODO: This could probably be made more efficient...
                if (_screenLocation.Contains(mouse.X, mouse.Y))
                {
                    if (!clicked)
                    {
                        if (mouse.LeftButton == ButtonState.Pressed)
                        {
                            clicked = !clicked;
                            
                            //Console.WriteLine("Toggled:" + _tileNode);
                            //_tileNode.Toggled = !_tileNode.Toggled;
                            //toggled = true;
                            if (startNode == null)
                            {
                                startNode = _tileNode;
                                startNode.passable = true;
                                return;
                            }
                            else if (destinationNode == null && destinationNode != startNode)
                            {
                                destinationNode = _tileNode;
                                _tileNode.passable = true;
                                openSet.Clear();
                                openSet.Add(startNode);
                                return;
                            }
                            if (startNode != null && destinationNode != null)
                            {
                                startNode = destinationNode;
                                destinationNode = _tileNode;
                                destinationNode.passable = true;
                                openSet.Clear();
                                openSet.Add(startNode);
                            }

                        }
                        if (mouse.RightButton == ButtonState.Pressed)
                        {
                            clicked = !clicked;
                            
                            if (_tileNode == destinationNode)
                                destinationNode = null;
                            if (_tileNode == startNode)
                                startNode = null;

                            _tileNode.passable = !_tileNode.passable;
                            //toggled = false;
                        }
                    }
                }
                else
                {
                    if (clicked)
                        clicked = !clicked;
                }
                oldMouse = mouse;
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                Color tileColor = Color.Gray;

                if (!_tileNode.passable)
                    tileColor = Color.Black;

                if (closedSet.Contains(_tileNode))
                    tileColor = Color.DarkBlue;

                if (openSet.Contains(_tileNode))
                    tileColor = Color.LightGray;

                if (_tileNode == destinationNode)
                    tileColor = Color.Green;

                if (_tileNode == startNode)
                    tileColor = Color.Brown;

                if (winSet != null && winSet.Contains(_tileNode))
                    tileColor = Color.LimeGreen;

                if (errorSet != null && errorSet.Contains(_tileNode))
                    tileColor = Color.Yellow;

                spriteBatch.Draw(_tile, _screenLocation, tileColor);

                if (_tileNode.DisplayStats)
                {
                    string fs = "f " + _tileNode.f;
                    string gs = "g " + _tileNode.g;
                    string hs = "h " + _tileNode.h;

                    string current = "(" + _tileNode.x + "," + _tileNode.y + ")";
                    string parents = "(" + _tileNode.parent.x + "," + _tileNode.parent.y + ")";

                    spriteBatch.DrawString(smallFont, fs, new Vector2(_screenLocation.Left, _screenLocation.Top), Color.White);
                    spriteBatch.DrawString(
                        smallFont, gs,
                        new Vector2(
                            _screenLocation.Left,
                            _screenLocation.Top + smallFont.MeasureString(fs).Y
                            ),
                        Color.White
                        );
                    spriteBatch.DrawString(
                        smallFont, hs,
                        new Vector2(
                            _screenLocation.Left,
                            _screenLocation.Top + smallFont.MeasureString(fs).Y + smallFont.MeasureString(gs).Y
                            ),
                        Color.White
                        );
                    /*
                    spriteBatch.DrawString(
                        smallFont, current,
                        new Vector2(
                            _screenLocation.Left + 20,
                            _screenLocation.Top
                            ),
                        Color.White
                        );
                    spriteBatch.DrawString(
                        smallFont, parents,
                        new Vector2(
                            _screenLocation.Left + 20,
                            _screenLocation.Top + smallFont.MeasureString(current).Y
                            ),
                        Color.White
                        );
                     */ 
                }

                /*
                if (_tileNode.DisplayStats)
                {
                    int g = _tileNode.FromStart;
                    int h = _tileNode.ToDestination;
                    int f = Math.Abs(g+h);

                    string fs = "f() = " + f;
                    string gs = "g() = " + g;
                    string hs = "h() = " + h;

                    spriteBatch.DrawString(smallFont, fs, new Vector2(_screenLocation.Left, _screenLocation.Top), Color.White);
                    spriteBatch.DrawString(
                        smallFont, gs,
                        new Vector2(
                            _screenLocation.Left,
                            _screenLocation.Top + smallFont.MeasureString(fs).Y
                            ),
                        Color.White
                        );
                    spriteBatch.DrawString(
                        smallFont, hs,
                        new Vector2(
                            _screenLocation.Left,
                            _screenLocation.Top + smallFont.MeasureString(fs).Y + smallFont.MeasureString(gs).Y
                            ),
                        Color.White
                        );
                }
                 */

                //if(!_tileNode.Toggled)
                //    spriteBatch.Draw(_tile, _screenLocation, Color.White);
                //else
                //    spriteBatch.Draw(_tile, _screenLocation, Color.Blue);
                //if (toggled)
                //{
                //    spriteBatch.Draw(_tile, _screenLocation, Color.Blue);
                //}
            }
        }
    }
}

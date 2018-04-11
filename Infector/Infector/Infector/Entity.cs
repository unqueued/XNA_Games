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

// For now, AI logic is hardcoded into subclasses.
// May implment possesor pattern

namespace Infector
{
    class Entity
    {
        private Texture2D _sprite;
        private Vector2 _position;
        private float _rotation;
        private Color[] spriteData;
        
        private const float SPEED = 5.0f;        // the actual speed of our Entity
                                                 // will be based on this.

        public Entity()
        {
            debugShowEntityStats = true;
        }

        public Entity(Texture2D sprite) : this()
        {
            _sprite = sprite;

            spriteData = new Color[sprite.Width * sprite.Height];
            sprite.GetData(spriteData);
        }

        // This constructor extracts movement information from its ancestor
        // and uses it
        public Entity(Texture2D sprite, Entity ancestor)
            : this(sprite)
        {
            if (ancestor != null)
            {
                Position = ancestor.Position;
            }
        }

        public virtual Color color { get; set; }

        public virtual Boolean debugShowScanArea {get; set; }

        public virtual Boolean debugShowEntityStats { get; set; }

        public virtual Boolean Clicked { get; set; }

        public virtual Entity Target { get; set; }

        public virtual List<Entity> Entities { get; set; }

        protected virtual float speed { get { return SPEED; } }

        public virtual List<Entity> OtherEntities
        {
            get
            {
                List<Entity> newList = new List<Entity>();
                foreach (Entity e in Entities)
                {
                    if (e != this)
                        newList.Add(e);
                }
                
                return (List<Entity>)newList;
            }
        }

        // This only needs to be set if you're outputting Entity text
        public virtual SpriteFont debugFont { get; set; }

        public Texture2D Sprite
        {
            get
            {
                return _sprite;
            }
            set
            {
                _sprite = value;
            }
        }

        // This should be the center of Location
        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        public float X { get { return _position.X; } }
        public float Y { get { return _position.Y; } }

        public Color[] SpriteData { get { return spriteData; } }

        // This needs to be improved.
        // It needs to include the actual visible location
        public Rectangle Location
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Sprite.Width, Sprite.Height);
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

        public override string ToString()
        {
            string newString = "";
            newString += base.ToString() + " ";
            newString += "Position: " + Position.ToString() + " ";
            newString += "Location: " + Location.ToString() + " ";
            return newString;
        }

        public float? distanceTo(Entity entity)
        {
            if (entity == null)
                return null;

            return vectorTo(entity).Length();
        }

        public Vector2 vectorTo(Entity entity)
        {
            return Vector2.Subtract(entity.Position, Position);
        }

        public virtual void update()
        {

        }

        public virtual void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, Location, color);
            if (debugShowEntityStats)
            {
                if (debugFont != null)
                {
                    Vector2 messagePosition;

                    string positionMessage = Position.ToString();
                    messagePosition = new Vector2(X, Y - debugFont.MeasureString(positionMessage).Y);
                    spriteBatch.DrawString(debugFont, positionMessage, messagePosition, Color.White);

                    string targetingMessage = "" + distanceTo(Target);
                    messagePosition = new Vector2(X, Y);
                    spriteBatch.DrawString(debugFont, targetingMessage, messagePosition, Color.White);

                    /*
                    Vector2 targetDistance;
                    targetDistance = Vector2.Subtract(Target.Position, Position);
                    string targetingMessage = Vector2.Distance(new Vector2(0, 0), targetDistance).ToString();
                    messagePosition = new Vector2(X, Y);
                    spriteBatch.DrawString(debugFont, targetingMessage, messagePosition, Color.White);
                    */

                }
            }
        }

    }
}

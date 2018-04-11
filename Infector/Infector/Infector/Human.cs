using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Infector
{
    class Human : Entity
    {
        private const float SPEED = 2.0f;

        public Human(Texture2D sprite) : base(sprite) {
            
        }

        public Human() { }

        protected override float speed
        {
            get
            {
                return base.speed + SPEED;
            }
        }

    }
}

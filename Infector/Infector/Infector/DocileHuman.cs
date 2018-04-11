using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Infector
{
    class DocileHuman : Human
    {
        public DocileHuman() { }

        public DocileHuman(Texture2D sprite)
            : base(sprite)
        {
            color = Color.Blue;
        }
    }
}

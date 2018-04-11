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
    class Zombie : Human
    {
        private const float SPEED = -1;

        public Zombie(Texture2D sprite)
            : base(sprite)
        {
            color = Color.Red;
        }

        public override void update()
        {
            base.update();


        }

        protected override float speed
        {
            get
            {
                return base.speed + SPEED;
            }
        }

        public Entity findClosestNonZombie()
        {
            var nonZombies = from e in OtherEntities
                             where !(e is Zombie)
                             select e;

            var closestEntities = nonZombies.OrderByDescending(d => d.distanceTo(this)).Last();

            return (Entity)closestEntities;
        }
    }
}

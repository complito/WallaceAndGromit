using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallaceAndGromit
{
    class Player
    {
        public int x, y;
        public Size size;
        public int currentFrame = 0;
        public AnimationDirection currentAnimation = AnimationDirection.None;
        public AnimationDirection previousAnimation = AnimationDirection.Right;
        public int speed;
        public Player(Size size, int x, int y, int speed)
        {
            this.size = size;
            this.x = x;
            this.y = y;
            this.speed = speed;
        }
        public void Left()
        {
            x -= speed;
        }

        public void Right()
        {
            x += speed;
        }

        public void Up()
        {
            y -= speed;
        }

        public void Down()
        {
            y += speed;
        }
    }
}

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
        public int X, Y;
        public Size Size;
        public int CurrentFrame = 0;
        public AnimationDirection CurrentAnimation = AnimationDirection.None;
        public AnimationDirection PreviousAnimation = AnimationDirection.Right;
        public int Speed;
        public Player(Size size, int x, int y, int speed)
        {
            Size = size;
            X = x;
            Y = y;
            Speed = speed;
        }
        public void Left()
        {
            X -= Speed;
        }

        public void Right()
        {
            X += Speed;
        }

        public void Up()
        {
            Y -= Speed;
        }

        public void Down()
        {
            Y += Speed;
        }
    }
}

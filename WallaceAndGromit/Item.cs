using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallaceAndGromit
{
    class Item
    {
        public int X;
        public int Y;
        public Bitmap Image;
        public Point PointOnMap;
        public bool IsRescued = false;
        public int Speed = 2;
        public AnimationDirection CurrentAnimation = AnimationDirection.Right;
        public int CurrentFrame = 0;

        public Item(int x, int y, Bitmap image, Point pointOnMap)
        {
            X = x;
            Y = y;
            Image = image;
            PointOnMap = pointOnMap;
        }
    }
}

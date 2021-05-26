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

        public Item(int x, int y, Bitmap image)
        {
            X = x;
            Y = y;
            Image = image;
        }
    }
}

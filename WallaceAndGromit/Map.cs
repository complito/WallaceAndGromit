using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallaceAndGromit
{
    class Map
    {
        public int[,] MapLayout;
        public int TextureWidth;
        public int TextureHeight;
        public Image GrassImage;
        public Image WallImage;

        public Map(int[,] mapLayout, int textureWidth,
            int textureHeight, Image grassImage, Image wallImage)
        {
            MapLayout = mapLayout;
            TextureWidth = textureWidth;
            TextureHeight = textureHeight;
            GrassImage = grassImage;
            WallImage = wallImage;
        }
    }
}

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
        public int[,] map;
        public int textureWidth;
        public int textureHeight;
        public Image grassImage;
        public Image wallImage;

        public Map(int[,] map, int textureWidth,
            int textureHeight, Image grassImage, Image wallImage)
        {
            this.map = map;
            this.textureWidth = textureWidth;
            this.textureHeight = textureHeight;
            this.grassImage = grassImage;
            this.wallImage = wallImage;
        }
    }
}

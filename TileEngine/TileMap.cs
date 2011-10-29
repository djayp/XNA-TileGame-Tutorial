using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileEngine
{
    public class TileMap
    {
        public List<TileLayer> layers = new List<TileLayer>();
        //TileMaps only have 1 collision layer
        public CollisionLayer CollisionLayer;

        public int GetWidthInPixel()
        {
            return GetWidth() * Engine.TileWidth;
        }

        public int GetHeightInPixel()
        {
            return GetHeight() * Engine.TileHeight;
        }


        public int GetWidth()
        {
            int width = -10000;

            //Gets largest width
            foreach (TileLayer layer in layers)
                width = (int)Math.Max(width, layer.Width);

            return width;
        }

        public int GetHeight()
        {
            int Height = -10000;

            //Gets largest width
            foreach (TileLayer layer in layers)
                Height = (int)Math.Max(Height, layer.Height);

            return Height;
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            foreach (TileLayer layer in layers)
            {
                layer.Draw(spriteBatch, camera);
            }
        }

    }
}

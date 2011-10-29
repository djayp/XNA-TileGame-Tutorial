using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace TileEngine
{
    public static class Engine
    {
        public const int TileWidth = 32;
        public const int TileHeight = 32;

        //Takes character position and gives you the cell he is standing inside of
        public static Point ConvertPositionToCell(Vector2 position)
        {
            return new Point((int)(position.X / (float)TileWidth), (int)(position.Y / (float)TileHeight));
        }

        public static Rectangle CreateRectForCell(Point cell)
        {
            return new Rectangle(
                cell.X * TileWidth, cell.Y * TileHeight, TileWidth, TileHeight
            );
        }


    }
}

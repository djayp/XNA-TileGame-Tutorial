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

//for FromFile method
using System.IO;

namespace TileEngine
{
    public class CollisionLayer
    {
        
        int[,] map;
        //transparancy = between 0 and 1
        float alpha = 1f;


        public int Width
        {
            get { return map.GetLength(1); }
        }

        public int Height
        {
            get { return map.GetLength(0); }
        }

        //creates a tile layer of all 0s
        public CollisionLayer(int width, int height)
        {
            map = new int[height, width];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    map[y, x] = -1;
        }


        public void Save(string fileName, string[] textureNames)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {

                writer.WriteLine("[Layout]");

                for (int y = 0; y < Height; y++)
                {
                    string line = String.Empty;

                    for (int x = 0; x < Width; x++)
                    {
                        line += map[y, x].ToString() + " ";
                    }

                    writer.WriteLine(line);
                }
            }
        }

    

        public static CollisionLayer FromFile(string filename)
        {
            CollisionLayer tileLayer;
            List<List<int>> tempLayout = new List<List<int>>();

            //using statement disposes of everything after the brackets, always use with file IO
            using (StreamReader reader = new StreamReader(filename))
            {
                bool readingLayout = false;

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().Trim();

                    if (string.IsNullOrEmpty(line))
                        continue;
                    
                    if (line.Contains("[Layout]"))
                    {
                        readingLayout = true;
                    }
                    else if (readingLayout)
                    {
                        List<int> row = new List<int>();

                        string[] cells = line.Split(' ');

                        foreach (string c in cells)
                        {
                            if (!string.IsNullOrEmpty(c))
                                row.Add(int.Parse(c));
                        }

                        tempLayout.Add(row);
                    }
                }
            }

            //width = Cells in first row
            int width = tempLayout[0].Count;
            //number of rows
            int height = tempLayout.Count;

            tileLayer = new CollisionLayer(width, height);


            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tileLayer.SetCellIndex(x, y, tempLayout[y][x]);
                }
            }
            return tileLayer;
        }

        public int GetCellIndex(int x, int y)
        {
            return map[y, x];
        }

        //Point is like a vector 2 but uses Ints instead of floats
        public int GetCellIndex(Point point)
        {
            return map[point.Y, point.X];
        }


        //Sets a cell index to whatever
        public void SetCellIndex(int x, int y, int cellIndex)
        {
            map[y, x] = cellIndex;
        }

        public void SetCellIndex(Point point, int cellIndex)
        {
            map[point.Y, point.X] = cellIndex;
        }

        public void RemoveIndex(int existingIndex)
        {
            //int tileMapWidth = map.GetLength(1);
            //int tileMapHeight = map.GetLength(0);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (map[y, x] == existingIndex)
                    {
                        map[y, x] = -1;
                    }
                    else if (map[y, x] > existingIndex)
                    {
                        map[y, x]--;
                    }

                }
            }

        }

        public void ReplaceIndex(int existingIndex, int newIndex)
        {
            //int tileMapWidth = map.GetLength(1);
            //int tileMapHeight = map.GetLength(0);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (map[y, x] == existingIndex)
                        map[y, x] = newIndex;
                }
            }

        }
        
    }//end class
}

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
    public class TileLayer
    {
        List<Texture2D> tileTextures = new List<Texture2D>();
        int[,] map;
        //transparancy = between 0 and 1
        float alpha = 1f;

        /// <summary>
        /// Set between 0 and 1, closer to 0, the more transparent
        /// </summary>
        public float Alpha
        {
            get { return alpha; }
            set
            {
                alpha = MathHelper.Clamp(value, 0f, 1f);
            }
        }

        public int WidthInPixels
        {
            get
            {
                return Width * Engine.TileWidth;
            }
                
        }

        public int HeightInPixels
        {
            get
            {
                return Height * Engine.TileHeight;
            }
        }

        public int Width
        {
            get { return map.GetLength(1); }
        }

        public int Height
        {
            get { return map.GetLength(0); }
        }

        //creates a tile layer of all 0s
        public TileLayer(int width, int height)
        {
            map = new int[height, width];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    map[y, x] = -1;
        }

        public TileLayer(int[,] existingMap)
        {
            //array is reference type, so you use clone to create a copy of it
            map = (int[,])existingMap.Clone();
        }

        //returns index if the TileLayer is using passed in texture, else -1
        public int IsUsingTexture(Texture2D texture)
        {
            if (tileTextures.Contains(texture))
            {
                return tileTextures.IndexOf(texture);
            }
            return -1;
        }

        public void Save(string fileName, string[] textureNames)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.WriteLine("[Textures]");
                foreach (string t in textureNames)
                {
                    writer.WriteLine(t);
                }


                writer.WriteLine();

                writer.WriteLine("[Properties]");
                writer.WriteLine("Alpha = " + Alpha.ToString());

                writer.WriteLine();

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

        public static TileLayer FromFile(string filename, out string[] textureNameArray)
        {
            TileLayer tileLayer;

            List<string> textureNames = new List<string>();
            //Grid(Row)
            tileLayer = ProcessFile(filename, textureNames);

            textureNameArray = textureNames.ToArray();

            return tileLayer;
        }

        public static TileLayer FromFile(ContentManager content, string filename)
        {
            TileLayer tileLayer;
            List<string> textureNames = new List<string>();

            tileLayer = ProcessFile(filename, textureNames);

            tileLayer.LoadTileTextures(content, textureNames.ToArray());

            return tileLayer;
        }

        private static TileLayer ProcessFile(string filename, List<string> textureNames)
        {
            TileLayer tileLayer;
            List<List<int>> tempLayout = new List<List<int>>();
            Dictionary<string, string> properties = new Dictionary<string, string>();

            //using statement disposes of everything after the brackets, always use with file IO
            using (StreamReader reader = new StreamReader(filename))
            {
                bool readingTextures = false;
                bool readingLayout = false;
                bool readingProperties = false;

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().Trim();

                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (line.Contains("[Textures]"))
                    {
                        readingTextures = true;
                        readingLayout = false;
                        readingProperties = false;
                    }
                    else if (line.Contains("[Layout]"))
                    {
                        readingTextures = false;
                        readingLayout = true;
                        readingProperties = false;
                    }
                    else if (line.Contains("[Properties]"))
                    {
                        readingProperties = true;
                        readingLayout = false;
                        readingTextures = false;
                    }
                    else if (readingTextures)
                    {
                        textureNames.Add(line);
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
                    else if (readingProperties)
                    {
                        string[] pair = line.Split('=');
                        string key = pair[0].Trim();
                        string value = pair[1].Trim();

                        properties.Add(key, value);
                    }
                }
            }

            //width = Cells in first row
            int width = tempLayout[0].Count;
            //number of rows
            int height = tempLayout.Count;

            tileLayer = new TileLayer(width, height);

            foreach (KeyValuePair<string, string> property in properties)
            {
                switch (property.Key)
                {
                    case "Alpha":
                        tileLayer.Alpha = float.Parse(property.Value);
                        break;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tileLayer.SetCellIndex(x, y, tempLayout[y][x]);
                }
            }
            return tileLayer;
        }



        public void LoadTileTextures(ContentManager content, params string[] textureNames)
        {
            Texture2D texture;

            foreach (string textureName in textureNames)
            {
                texture = content.Load<Texture2D>(textureName);
                tileTextures.Add(texture);
            }
        }

        //TileEditor Method
        public void AddTexture(Texture2D texture)
        {
            tileTextures.Add(texture);
        }

        public void RemoveTexture(Texture2D texture)
        {
            RemoveIndex(tileTextures.IndexOf(texture));
            tileTextures.Remove(texture);
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

        public void Draw(SpriteBatch batch, Camera camera)
        {
            batch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, null, null, null, null, camera.TransformMatrix);
       
            int tileMapWidth = map.GetLength(1);
            int tileMapHeight = map.GetLength(0);

            for (int x = 0; x < tileMapWidth; x++)
            {
                for (int y = 0; y < tileMapHeight; y++)
                {
                    int tileTextureIndex = map[y, x];

                    if (tileTextureIndex == -1)
                        continue;

                    Texture2D texture = tileTextures[tileTextureIndex];

                    batch.Draw(texture
                                    , new Rectangle(
                                        x * Engine.TileWidth,
                                        y * Engine.TileHeight,
                                        Engine.TileWidth,
                                        Engine.TileHeight)
                                    , new Color(new Vector4(1f, 1f, 1f, Alpha)));//R, G, B, Alpha
                }
            }
            batch.End();

        }

    }//end class
}

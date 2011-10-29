using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

using TileEngine;

namespace TileEditor
{
    using Image = System.Drawing.Image;

    public partial class Form1 : Form
    {
        string[] imageExtensions = new string[]
        {
            ".jpg", ".png", ".tga",
        };

        int maxWidth = 0, maxHeight = 0;

        SpriteBatch spriteBatch;

        Texture2D tileTexture;

        Camera camera = new Camera();
        TileLayer currentLayer;
        int cellX, cellY;

        TileMap tileMap = new TileMap();

        //fill tool
        const int maxFillCount = 1000;
        int fillCounter = maxFillCount;

        Dictionary<string, TileLayer> layerDict = new Dictionary<string, TileLayer>();
        Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();
        Dictionary<string, Image> previewDict = new Dictionary<string, Image>();


        public GraphicsDevice GraphicsDevice
        {
            get { return tileDisplay1.GraphicsDevice; }
        }

        public Form1()
        {
            InitializeComponent();

            tileDisplay1.OnInitialize += new EventHandler(tileDisplay1_OnInitialize);
            tileDisplay1.OnDraw += new EventHandler(tileDisplay1_OnDraw);

            //If application idles, render tileDisplay. Windows forms doesn't repaint or draw often
            Application.Idle += delegate { tileDisplay1.Invalidate(); };

                                   //Name of filter(in dropdown), type of file accepted

            saveFileDialog1.Filter = "Layer File|*.layer";

            Mouse.WindowHandle = tileDisplay1.Handle;

            while (string.IsNullOrEmpty(contentPathTextBox.Text))
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    contentPathTextBox.Text = folderBrowserDialog1.SelectedPath;
                }
                else
                {
                    MessageBox.Show("Please choose a content directory.");
                }
            }
        }

        void tileDisplay1_OnInitialize(object sender, EventArgs e)
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //this.contentPathTextBox.Text = Directory.GetCurrentDirectory();
            //C:\Users\Danny\Documents\Visual Studio 2010\Projects\TileGame\TileGame\TileGameContent
            //tileTexture﻿ = Texture2D.FromStream(GraphicsDevice, new StreamReader(@"C:\Users\Danny\documents\visual studio 2010\Projects\TileGame\TileEditor\Content\tile.png").BaseStream);
            tileTexture﻿ = Texture2D.FromStream(GraphicsDevice, new StreamReader(@"Content\tile.png").BaseStream);
            //tileTexture﻿ = Texture2D.FromStream(GraphicsDevice, new StreamReader("Content\tile.png").BaseStream);
        }

        void tileDisplay1_OnDraw(object sender, EventArgs e)
        {
            Logic();
            Render();
        }

        #region Draw Tool Method

        //fill tool
        public void FillCell(int x, int y, int desiredIndex)
        {
            int oldIndex = currentLayer.GetCellIndex(x, y);
            currentLayer.SetCellIndex(x, y, desiredIndex);

            if (desiredIndex == oldIndex || fillCounter ==0)
                return;

            fillCounter--;

            if (x > 0 && currentLayer.GetCellIndex(x - 1, y) == oldIndex)
            {
                FillCell(x - 1, y, desiredIndex);
            }
            if (x < currentLayer.Width - 1 && currentLayer.GetCellIndex(x + 1, y) == oldIndex)
            {
                FillCell(x + 1, y, desiredIndex);
            }
            if (y > 0 && currentLayer.GetCellIndex(x , y - 1) == oldIndex)
            {
                FillCell(x, y - 1, desiredIndex);
            }
            if (y < currentLayer.Height - 1 && currentLayer.GetCellIndex(x, y + 1) == oldIndex)
            {
                FillCell(x, y + 1, desiredIndex);
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (currentLayer != null)
                currentLayer.Alpha = (float)alphaSlider.Value / 100f;
        }

        #endregion


        /// <summary>
        /// Updates
        /// </summary>
        private void Logic()
        {
            camera.Position.X = hScrollBar1.Value * Engine.TileWidth;
            camera.Position.Y = vScrollBar1.Value * Engine.TileHeight;

            int mx = Mouse.GetState().X;
            int my = Mouse.GetState().Y;

            if (currentLayer != null)
            {

                if (mx >= 0 && mx < tileDisplay1.Width &&
                    my >= 0 && my < tileDisplay1.Height)
                {
                    //cells
                    cellX = mx / Engine.TileWidth;
                    cellY = my / Engine.TileHeight;

                    cellX += hScrollBar1.Value;
                    cellY += vScrollBar1.Value;

                    cellX = (int)MathHelper.Clamp(cellX, 0, currentLayer.Width - 1);
                    cellY = (int)MathHelper.Clamp(cellY, 0, currentLayer.Height -1);

                    if (Mouse.GetState().LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                    {
                        if (drawRadioButton.Checked && textureListBox.SelectedItem != null)
                        {
                            Texture2D texture = textureDict[textureListBox.SelectedItem as string];
                            int index = currentLayer.IsUsingTexture(texture);

                            if (index == -1)
                            {
                                currentLayer.AddTexture(texture);
                                index = currentLayer.IsUsingTexture(texture);
                            }

                            if (fillCheckBox.Checked)
                            {
                                fillCounter = maxFillCount;
                                FillCell(cellX, cellY, index);
                            }
                            else
                                currentLayer.SetCellIndex(cellX, cellY, index);
                        }
                        else if (eraseRadioButton.Checked)
                        {
                            if (fillCheckBox.Checked)
                            {
                                fillCounter = maxFillCount;
                                FillCell(cellX, cellY, -1);
                            }
                            else
                                currentLayer.SetCellIndex(cellX, cellY, -1);
                        }
                    }
                }
                else
                {
                    cellX = cellY = -1;
                }


            } //end currentLayer != null
        }

        /// <summary>
        /// Draws
        /// </summary>
        private void Render()
        {
            GraphicsDevice.Clear(Color.Black);

            foreach (TileLayer layer in tileMap.layers)
            {
                layer.Draw(spriteBatch, camera);

                spriteBatch.Begin();
                for (int y = 0; y < layer.Height; y++)
                {
                    for (int x = 0; x < layer.Width; x++)
                    {

                        if (layer.GetCellIndex(x, y) == -1)
                        {
                            spriteBatch.Draw(tileTexture
                                            , new Rectangle(
                                                x * Engine.TileWidth - (int)camera.Position.X,
                                                y * Engine.TileHeight - (int)camera.Position.Y,
                                                Engine.TileWidth,
                                                Engine.TileHeight)
                                            , Color.White);
                        }
                    }
                }
                spriteBatch.End();

                if (layer == currentLayer)
                    break;
            } 

                if (currentLayer != null)
                {
                   
                    if (cellX != -1 && cellY != -1)
                    {
                        spriteBatch.Begin();
                        spriteBatch.Draw(tileTexture
                                            , new Rectangle(
                                                cellX * Engine.TileWidth - (int)camera.Position.X,
                                                cellY * Engine.TileHeight - (int)camera.Position.Y,
                                                Engine.TileWidth,
                                                Engine.TileHeight)
                                            , Color.Red);

                        spriteBatch.End();

                    }
                }
            
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Layer File|*.layer";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                string[] textureNames;

                TileLayer layer = TileLayer.FromFile(fileName, out textureNames);

                //Sets to key as the filename from the directory path
                layerDict.Add(Path.GetFileName(fileName), layer);
                //add the layer to the tileMap
                tileMap.layers.Add(layer);
                layerListBox.Items.Add(Path.GetFileName(fileName));

                foreach (string textureName in textureNames)
                {


                    if (textureDict.ContainsKey(textureName))
                    {
                        layer.AddTexture(textureDict[textureName]);
                        continue;
                    }

                    string fullPath = contentPathTextBox.Text + "/" + textureName;

                    foreach (string ext in imageExtensions)
                    {
                        if (File.Exists(fullPath + ext))
                        {
                            fullPath += ext;
                            break;
                        }
                    }

                    Texture2D﻿ tex = Texture2D.FromStream(GraphicsDevice, new StreamReader(fullPath).BaseStream);
                    Image image = Image.FromFile(fullPath);
                    textureDict.Add(textureName, tex);
                    previewDict.Add(textureName, image);

                    textureListBox.Items.Add(textureName);
                    layer.AddTexture(tex);
                }
                AdjustScrollBars();
            }
        }

        private void AdjustScrollBars()
        {
            if (tileMap.GetWidthInPixel() > tileDisplay1.Width)
            {
                maxWidth = (int)Math.Max(tileMap.GetWidth(), maxWidth);

                hScrollBar1.Visible = true;
                hScrollBar1.Minimum = 0;
                hScrollBar1.Maximum = maxWidth;
            }
            else
            {
                maxWidth = 0;
                hScrollBar1.Visible = false;
            }

            if (tileMap.GetHeightInPixel() > tileDisplay1.Height)
            {
                maxHeight = (int)Math.Max(tileMap.GetHeight(), maxHeight);

                vScrollBar1.Visible = true;
                vScrollBar1.Minimum = 0;
                vScrollBar1.Maximum = maxHeight;
            }
            else
            {
                maxHeight = 0;
                vScrollBar1.Visible = false;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        { 
            if (layerListBox.SelectedItem != null)
            {
                string fileName = layerListBox.SelectedItem as string;
                saveFileDialog1.FileName = fileName;

                TileLayer tileLayer = layerDict[fileName];

                Dictionary<int, string> utilizedTextures = new Dictionary<int, string>();

                foreach (string textureName in textureListBox.Items)
                {
                    int index = tileLayer.IsUsingTexture(textureDict[textureName]);

                    if (index != -1)
                    {
                        utilizedTextures.Add(index,textureName);
                    }
                }

                List<string> utilizedTextureList = new List<string>();

                for (int i = 0; i < utilizedTextures.Count; i++)
                {
                    utilizedTextureList.Add(utilizedTextures[i]);
                }

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    tileLayer.Save(saveFileDialog1.FileName, utilizedTextureList.ToArray());
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textureListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (textureListBox.SelectedItem != null)
            {
                texturePreviewBox.Image = previewDict[textureListBox.SelectedItem as string];
            }
        }

        private void layerListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (layerListBox.SelectedItem != null)
            {
                currentLayer = layerDict[layerListBox.SelectedItem as string];
                alphaSlider.Value = (int)(currentLayer.Alpha * 100);
            }
        }

        private void addLayerButton_Click(object sender, EventArgs e)
        {
            NewLayerForm form = new NewLayerForm();
            form.ShowDialog();

            if (form.OKPressed)
            {
                TileLayer tileLayer = new TileLayer(
                    int.Parse(form.width.Text),
                    int.Parse(form.height.Text));

                layerDict.Add(form.name.Text, tileLayer);
                tileMap.layers.Add(tileLayer);
                layerListBox.Items.Add(form.name.Text);

                AdjustScrollBars();
            }
        }

        private void removeLayerButton_Click(object sender, EventArgs e)
        {
            if (currentLayer != null)
            {
                string fileName = layerListBox.SelectedItem as string;

                
                tileMap.layers.Remove(currentLayer);
                layerDict.Remove(fileName);
                layerListBox.Items.Remove(layerListBox.SelectedItem);

                currentLayer = null;

                AdjustScrollBars();
            }
        }

        private void addTextureButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "JPG File|*.jpg|PNG Image|*.png|TGA Image|*.tga";

            openFileDialog1.InitialDirectory = contentPathTextBox.Text;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;

                Texture2D texture = Texture2D.FromStream(GraphicsDevice, new StreamReader(fileName).BaseStream);
                Image image = Image.FromFile(fileName);

                fileName = fileName.Replace(contentPathTextBox.Text + "\\", "");
                fileName = fileName.Remove(fileName.LastIndexOf("."));

                textureListBox.Items.Add(fileName);
                textureDict.Add(fileName, texture);
                previewDict.Add(fileName, image);

            }
        }

        private void removeTextureButton_Click(object sender, EventArgs e)
        {
            if (textureListBox.SelectedItem != null)
            {
                string textureName = textureListBox.SelectedItem as string;

                foreach (TileLayer layer in tileMap.layers)
                {
                    if (layer.IsUsingTexture(textureDict[textureName]) != -1)
                    {
                        layer.RemoveTexture(textureDict[textureName]);
                    }
                }

                textureDict.Remove(textureName);
                previewDict.Remove(textureName);
                textureListBox.Items.Remove(textureListBox.SelectedItem);

                texturePreviewBox.Image = null;
            }
        }

            
    }
}

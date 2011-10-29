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

using TileEngine;

namespace TileGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        TileMap tileMap = new TileMap();
        Camera camera = new Camera();

        List<AnimatedSprite> npcs = new List<AnimatedSprite>();

        AnimatedSprite sprite;
       

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            //needs to be after base.Initialize because that creates sprite
            FrameAnimation up = new FrameAnimation(2, 32, 32, 0, 0);
            up.FramesPerSecond = 4;
            sprite.Animations.Add("Up", up);

            FrameAnimation down = new FrameAnimation(2, 32, 32, 64, 0);
            down.FramesPerSecond = 4;
            sprite.Animations.Add("Down", down);

            FrameAnimation left = new FrameAnimation(2, 32, 32, 128, 0);
            left.FramesPerSecond = 4;
            sprite.Animations.Add("Left", left);

            FrameAnimation right = new FrameAnimation(2, 32, 32, 192, 0);
            right.FramesPerSecond = 4;
            sprite.Animations.Add("Right", right);

            Random rand = new Random();

            foreach (AnimatedSprite s in npcs)
            {
                s.Animations.Add("Up", (FrameAnimation)up.Clone());
                s.Animations.Add("Down", (FrameAnimation)down.Clone());
                s.Animations.Add("Left", (FrameAnimation)left.Clone());
                s.Animations.Add("Right", (FrameAnimation)right.Clone());

                int animation = rand.Next(3);

                switch(animation)
                {
                    case 0:
                        s.CurrentAnimationName = "Up";
                        break;
                    case 1:
                        s.CurrentAnimationName = "Down";
                        break;
                    case 2:
                        s.CurrentAnimationName = "Left";
                        break;
                    case 3:
                        s.CurrentAnimationName = "Right";
                        break;
                }
                //position of NPCs
                s.Position = new Vector2(
                    //use tileMap.GetWidthInPixel() - 32 to set size to full map
                    rand.Next(600), 
                    rand.Next(400));
            }

            sprite.CurrentAnimationName = "Down";


        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            tileMap.layers.Add( TileLayer.FromFile(Content, "Content/Layers/Layer1.layer"));
            tileMap.CollisionLayer = CollisionLayer.FromFile("Content/Layers/Collision.layer");


            sprite = new AnimatedSprite(Content.Load<Texture2D>("Sprites/thf4"));

            //Because all sprites are 32, 32. 16 is half the sprite width, 32 is the height, where the feet are.
            sprite.OriginOffset = new Vector2(16, 32);

            npcs.Add(new AnimatedSprite(Content.Load<Texture2D>("Sprites/amg1")));
            npcs.Add(new AnimatedSprite(Content.Load<Texture2D>("Sprites/man1")));
            npcs.Add(new AnimatedSprite(Content.Load<Texture2D>("Sprites/npc3")));
            npcs.Add(new AnimatedSprite(Content.Load<Texture2D>("Sprites/wmg1")));
            npcs.Add(new AnimatedSprite(Content.Load<Texture2D>("Sprites/ygr1")));
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState keyState = Keyboard.GetState();

            //Used with cameraSpeed
            Vector2 motion = Vector2.Zero;

            if (keyState.IsKeyDown(Keys.Up))
                motion.Y--;
            if (keyState.IsKeyDown(Keys.Down))
                motion.Y++;
            if (keyState.IsKeyDown(Keys.Right))
                motion.X++;
            if (keyState.IsKeyDown(Keys.Left))
                motion.X--;

            //So moving diagonally isn't faster
            if (motion != Vector2.Zero)
            {
                motion.Normalize();

                //for Collision Tiles
                motion = CheckCollisionForMotion(motion, sprite);

                sprite.Position += motion * sprite.Speed;
                UpdateSpriteAnimation(motion);
                sprite.IsAnimating = true;

                CheckForUnwalkableTiles(sprite);

            }
            else
            {
                sprite.IsAnimating = false;
            }

            //Prevents sprite from moving outside area
            sprite.ClampToArea(tileMap.GetWidthInPixel(), tileMap.GetHeightInPixel());

            sprite.Update(gameTime);    

            foreach (AnimatedSprite s in npcs)
            {
                s.Update(gameTime);

                if(AnimatedSprite.AreColliding(sprite, s))
                {
                    //direction vector
                    Vector2 d = Vector2.Normalize(s.Position - sprite.Position);

                    //push player character, sets player positon to the position of the npc - direction vection multiplied by the 2 radius
                    sprite.Position = s.Position - (d * (sprite.CollisionRadius + s.CollisionRadius));
                }
            }

            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;

            camera.LockToTarget(sprite, screenWidth, screenHeight);

            camera.ClampToArea(tileMap.GetWidthInPixel() - screenWidth, tileMap.GetHeightInPixel() - screenHeight);

            base.Update(gameTime);
        }

        private void CheckForUnwalkableTiles(AnimatedSprite sprite)
        {
            Point spriteCell = Engine.ConvertPositionToCell(sprite.Center);

            //nullable type
            Point? upLeft, up, upRight,
                left, right,
                downLeft, down, downRight;

            //left = down = right = up = new Point();

            //left.Y = right.Y = spriteCell.Y;
            //up.X = down.X = spriteCell.X;

            //if (spriteCell.X > 0 && spriteCell.Y > 0)
            //{
            //    upLeft.X = spriteCell.X - 1;
            //    left.X = spriteCell.X - 1;
            //    downLeft.X = spriteCell.X - 1;
            //}
            
            //if(spriteCell.X < tileMap.CollisionLayer.Width - 1)
            //{
            //    upRight.X = spriteCell.X + 1;
            //    right.X = spriteCell.X + 1;
            //    downRight.X = spriteCell.X + 1;
            //}

            //if (spriteCell.Y > 0)
            //{
            //    upLeft.Y = spriteCell.Y - 1;
            //    up.Y = spriteCell.Y - 1;
            //    downLeft.Y = spriteCell.Y - 1;
            //}
            
            //if (spriteCell.Y < tileMap.CollisionLayer.Height - 1)
            //{
            //    upRight.Y = spriteCell.Y + 1;
            //    down.Y = spriteCell.Y + 1;
            //    downRight.Y = spriteCell.Y + 1;
            //}
        }

        private void UpdateSpriteAnimation(Vector2 motion)
        {
            //Gives the angle in which the character is moving to change the animation depending on it.
            float motionAngle = (float)Math.Atan2(motion.Y, motion.X);

            //Set to right animation if character is moving that way
            if (motionAngle >= -MathHelper.PiOver4 && motionAngle <= MathHelper.PiOver4)
            {
                sprite.CurrentAnimationName = "Right";
            }
            //Down > PiOver4 and <= 3PiOver4
            else if (motionAngle >= MathHelper.PiOver4 && motionAngle <= 3f * MathHelper.PiOver4)
            {
                sprite.CurrentAnimationName = "Down";
            }
            //Up - less than -PiOver4  and >=  - 3PiOver4
            else if (motionAngle <= -MathHelper.PiOver4 && motionAngle >= -3f * MathHelper.PiOver4)
            {
                sprite.CurrentAnimationName = "Up";
            }
            else
            {
                sprite.CurrentAnimationName = "Left";
            }
        }

        private Vector2 CheckCollisionForMotion(Vector2 motion, AnimatedSprite sprite)
        {
            Point cell = Engine.ConvertPositionToCell(sprite.Origin);
            cell.X = (int)MathHelper.Clamp(cell.X, 0f, tileMap.GetWidth() - 1);
            cell.Y = (int)MathHelper.Clamp(cell.Y, 0f, tileMap.GetHeight() - 1);

            int collIndex;
            
            collIndex = tileMap.CollisionLayer.GetCellIndex(cell);

            //If player is standing on tile index 2 (Mud)
            if (collIndex == 2)
                return motion * .2f;

            return motion;
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            tileMap.Draw(spriteBatch, camera);

            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, null, null, null, null, camera.TransformMatrix);

            sprite.Draw(spriteBatch);

            foreach (AnimatedSprite s in npcs)
                s.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

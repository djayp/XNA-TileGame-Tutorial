using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace TileEngine
{
    public class Camera
    {
        //www.youtube.com/watch?v=GwccAvY7eyQ&feature=related 
        //using cameraSpeed to fix the problem with the camera moving faster diagonally than horizontally and vertically

        public Vector2 Position = Vector2.Zero;


        public Matrix TransformMatrix
        {
            get
            {
                //Position needs to be negative
                return Matrix.CreateTranslation(new Vector3(-Position, 0f));
            }
        }

        //Lock's camera to the player sprite
        public void LockToTarget(AnimatedSprite sprite, int screenWidth, int screenHeight)
        {
            //sets camera position to the sprite's position. Middle of sprite, -(screenWidth / 2) puts sprite in center of screen when camera moves
            this.Position.X = sprite.Position.X + (sprite.CurrentAnimation.CurrentRect.Width / 2) - (screenWidth / 2);

            this.Position.Y = sprite.Position.Y + (sprite.CurrentAnimation.CurrentRect.Height / 2) - (screenHeight / 2);
        }

        /// <summary>
        /// Clamps Camera
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ClampToArea(int width, int height)
        {
            if (this.Position.X > width)
                this.Position.X = width;

            if (this.Position.Y > height)
                this.Position.Y = height;


            // Clamping camera
            if (this.Position.X < 0)
                this.Position.X = 0;
            if (this.Position.Y < 0)
                this.Position.Y = 0;
        }

    }
}

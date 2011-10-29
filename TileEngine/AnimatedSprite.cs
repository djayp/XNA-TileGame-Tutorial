using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileEngine
{
    public class AnimatedSprite
    {
        public Dictionary<string, FrameAnimation> Animations = new Dictionary<string, FrameAnimation>();

        string currentAnimation = null;
        bool animating = true;
        Texture2D texture;

        public Vector2 Position = Vector2.Zero;

        //Helps determines tile the sprite is stepping on
        public Vector2 OriginOffset = Vector2.Zero;

        public Vector2 Origin
        {
            get { return Position + OriginOffset;  }
        }

        public Vector2 Center
        {
            get
            {
                return Position + new Vector2(CurrentAnimation.CurrentRect.Width / 2, CurrentAnimation.CurrentRect.Height / 2);
            }
        }

        //for collision detection using circles
        float radius = 16f;

        float speed = 2f;

        public float Speed
        {
            get { return speed; }
            set
            {
                //doesn't allow the speed to be set below 1
                speed = (float)Math.Max(value, .1f);
            }
        }

        public float CollisionRadius
        {
            get { return radius; }
            set { radius = (float)Math.Max(value, 1);   }
        }

        public bool IsAnimating
        {
            get { return animating;  }
            set { animating = value; }
        }

        public FrameAnimation CurrentAnimation
        {
            get
            {
                if (!string.IsNullOrEmpty(currentAnimation))
                    return Animations[currentAnimation];
                else return null;
            }
        }

        public string CurrentAnimationName
        {
            get { return currentAnimation;  }
            set
            {
                if (Animations.ContainsKey(value))
                    currentAnimation = value;
            }
        }

        public AnimatedSprite(Texture2D texture)
        {
            this.texture = texture;
        }

        //Collision Detection
        public static bool AreColliding(AnimatedSprite a, AnimatedSprite b)
        {
            Vector2 d = b.Position - a.Position;

            //Returns true if colliding
            return (d.Length() < b.CollisionRadius + a.CollisionRadius);
        }

        public void ClampToArea(int width, int height)
        {

            if (Position.X < 0)
                Position.X = 0;
            if (Position.Y < 0)
                Position.Y = 0;

            if (Position.X > width - CurrentAnimation.CurrentRect.Width)
                Position.X = width - CurrentAnimation.CurrentRect.Width;
            if (Position.Y > height - CurrentAnimation.CurrentRect.Height)
                Position.Y = height - CurrentAnimation.CurrentRect.Height;
        }

        public void Update(GameTime gameTime)
        {
            if (!IsAnimating)
                return;

            FrameAnimation animation = CurrentAnimation;

            if (animation == null)
            {
                if (Animations.Count > 0)
                {
                    string[] keys = new string[Animations.Count];
                    Animations.Keys.CopyTo(keys, 0);

                    currentAnimation = keys[0];

                    animation = CurrentAnimation;
                }
                else
                    return;
            }

            animation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            FrameAnimation animation = CurrentAnimation;

            if (animation != null)
                spriteBatch.Draw(texture,
                                    Position,
                                    animation.CurrentRect,
                                    Color.White);
        }

    }
}

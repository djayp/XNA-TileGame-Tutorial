using System;
using Microsoft.Xna.Framework;

namespace TileEngine
{
    public class FrameAnimation : ICloneable
    {
        Rectangle[] frames;
        int currentFrame = 0;

        //Number of seconds to wait before switching frames
        float frameLength = .5f;
        float timer = 0;

        public int FramesPerSecond
        {
            get
            {
                return (int)(1f / frameLength);
            }
            set
            {
                frameLength = (float)Math.Max(1f / (float)value, .001f);
            }
        }

        public Rectangle CurrentRect
        {
            get { return frames[currentFrame]; }
        }

        public int CurrentFrame
        {
            get { return currentFrame; }
            set
            {
                currentFrame = (int)MathHelper.Clamp(value, 0, frames.Length - 1);
            }
        }

        public FrameAnimation(int numberOfFrames, int frameWidth, int frameHeight,int xOffSet, int yOffSet)
        {
            frames = new Rectangle[numberOfFrames];

            for (int i = 0; i < numberOfFrames; i++)
            {
                Rectangle rect = new Rectangle();
                rect.Width = frameWidth;
                rect.Height = frameHeight;
                rect.X = xOffSet + (i * frameWidth);
                rect.Y = yOffSet;

                frames[i] = rect;
            }
        }

        //for clone
        private FrameAnimation()
        {

        }

        public void Update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(timer >= frameLength)
            {
                timer = 0f;

                currentFrame++;
                if(currentFrame >= frames.Length)
                    currentFrame = 0;
            }
        }



        public object Clone()
        {
            FrameAnimation anim = new FrameAnimation();
            anim.frameLength = frameLength;
            anim.frames = frames;

            return anim;
        }

    }
}

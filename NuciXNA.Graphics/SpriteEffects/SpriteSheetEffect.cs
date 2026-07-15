using NuciXNA.Primitives;

using NuciXNA.Graphics.Drawing;

namespace NuciXNA.Graphics.SpriteEffects
{
    public abstract class SpriteSheetEffect : NuciSpriteEffect<TextureSprite>
    {
        /// <summary>
        /// Gets or sets the current frame counter in milliseconds.
        /// </summary>
        public int FrameCounter { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds between frame switches.
        /// </summary>
        public int SwitchFrame { get; set; }

        /// <summary>
        /// Gets or sets the current sprite sheet frame coordinates.
        /// </summary>
        public Point2D CurrentFrame { get; set; }

        /// <summary>
        /// Gets or sets the number of frames in each dimension of the sprite sheet.
        /// </summary>
        public Size2D FrameAmount { get; set; }

        /// <summary>
        /// Gets the size of a single frame in the sprite sheet.
        /// </summary>
        public Size2D FrameSize { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="SpriteSheetEffect"/> class.
        /// </summary>
        public SpriteSheetEffect()
        {
            SwitchFrame = 100;
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected override void DoLoadContent() => FrameSize = Sprite.TextureSize / FrameAmount;

        /// <summary>
        /// Unloads the content.
        /// </summary>
        protected override void DoUnloadContent() { }
    }
}

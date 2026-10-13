using Microsoft.Xna.Framework;

namespace NuciXNA.Graphics.SpriteEffects
{
    /// <summary>
    /// Oscillation sprite effect.
    /// </summary>
    public class OscillationEffect : RotationEffect
    {
        /// <summary>
        /// Gets or sets the minimum rotation multiplier.
        /// </summary>
        public float MinimumMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the maximum rotation multiplier.
        /// </summary>
        public float MaximumMultiplier { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OscillationEffect"/> class.
        /// </summary>
        public OscillationEffect() : base()
        {
            MinimumMultiplier = 0.5f;
            MaximumMultiplier = 1.5f;
        }

        /// <summary>
        /// Updates the rotation multiplier.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        protected override void DoUpdate(GameTime gameTime)
        {
            float delta = Speed * ((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (IsIncreasing)
            {
                CurrentMultiplier += delta;

                if (CurrentMultiplier >= MaximumMultiplier)
                {
                    CurrentMultiplier = MaximumMultiplier;
                    IsIncreasing = false;
                }
            }
            else
            {
                CurrentMultiplier -= delta;

                if (CurrentMultiplier <= MinimumMultiplier)
                {
                    CurrentMultiplier = MinimumMultiplier;
                    IsIncreasing = true;
                }
            }
        }
    }
}

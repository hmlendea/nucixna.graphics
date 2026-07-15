using System;

using Microsoft.Xna.Framework;

using NuciXNA.Primitives;

using NuciXNA.Graphics.Drawing;

namespace NuciXNA.Graphics.SpriteEffects
{
    /// <summary>
    /// Zoom sprite effect.
    /// </summary>
    public class MovementEffect : NuciSpriteEffect<Sprite>
    {
        /// <summary>
        /// Gets or sets the movement speed in units per second.
        /// </summary>
        public float Speed { get; set; } = 7.5f;

        /// <summary>
        /// Gets or sets the target location to move towards.
        /// </summary>
        public Point2D TargetLocation { get; set; }

        /// <summary>
        /// Gets the current location offset from the sprite's base location.
        /// </summary>
        public Point2D LocationOffset { get; private set; } = Point2D.Empty;

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected override void DoLoadContent()
        {
            Activated += OnActivated;
            Deactivated += OnDeactivated;
        }

        /// <summary>
        /// Unloads the content.
        /// </summary>
        protected override void DoUnloadContent()
        {
            Activated -= OnActivated;
            Deactivated -= OnDeactivated;
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        protected override void DoUpdate(GameTime gameTime)
        {
            Point2D currentLocation = Sprite.Location + LocationOffset;
            Point2D direction = TargetLocation - currentLocation;

            double distance = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

            if (distance <= Speed)
            {
                LocationOffset = TargetLocation - Sprite.Location;
                Deactivate();
                return;
            }

            LocationOffset += new Point2D(
                (int)Math.Round((float)(direction.X / distance * Speed)),
                (int)Math.Round((float)(direction.Y / distance * Speed)));
        }

        private void OnActivated(object sender, EventArgs e)
            => LocationOffset = Point2D.Empty;

        private void OnDeactivated(object sender, EventArgs e)
            => LocationOffset = Point2D.Empty;
    }
}

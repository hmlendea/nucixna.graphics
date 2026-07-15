using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NuciXNA.Primitives;
using NuciXNA.Primitives.Mapping;

using XnaGraphicsSpriteEffects = Microsoft.Xna.Framework.Graphics.SpriteEffects;

namespace NuciXNA.Graphics.Drawing
{
    /// <summary>
    /// Draws textures to a sprite batch, managing sampler state transitions.
    /// </summary>
    public static class TextureDrawer
    {
        private static SpriteSortMode DefaultSpriteSortMode => SpriteSortMode.Deferred;

        private static SpriteSortMode currentSpriteSortMode = SpriteSortMode.Deferred;
        private static SamplerState currentSamplerState;

        /// <summary>
        /// Draws a texture using the graphics manager's default sampler state for Stretch layout.
        /// </summary>
        public static void Draw(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Point2D location,
            Rectangle2D sourceRectangle,
            Colour tint,
            float opacity,
            float rotation,
            Point2D origin,
            Scale2D scale,
            TextureLayout textureLayout)
        {
            Draw(
                spriteBatch,
                texture,
                location,
                sourceRectangle,
                tint,
                opacity,
                rotation,
                origin,
                scale,
                textureLayout,
                GraphicsManager.Instance.DefaultSamplerState);
        }

        /// <summary>
        /// Draws a texture with an explicit sampler state for Stretch layout.
        /// </summary>
        public static void Draw(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Point2D location,
            Rectangle2D sourceRectangle,
            Colour tint,
            float opacity,
            float rotation,
            Point2D origin,
            Scale2D scale,
            TextureLayout textureLayout,
            SamplerState samplerState)
        {
            Color textureColour = tint.ToXnaColor();
            float layerDepth = 0.0f;

            textureColour.A = (byte)(textureColour.A * opacity);

            if (textureLayout == TextureLayout.Tile)
            {
                SetSpriteBatchProperties(spriteBatch, SpriteSortMode.Deferred, SamplerState.PointWrap);

                sourceRectangle = new Rectangle2D(
                    sourceRectangle.Location.X,
                    sourceRectangle.Location.Y,
                    (int)(sourceRectangle.Size.Width * scale.Horizontal),
                    (int)(sourceRectangle.Size.Height * scale.Vertical));

                scale = Scale2D.One;
            }
            else if (textureLayout == TextureLayout.Stretch)
            {
                Size2D targetSize = sourceRectangle.Size * scale;

                SetSpriteBatchProperties(spriteBatch, DefaultSpriteSortMode, samplerState);

                location += new Point2D(
                    targetSize.Width / 2 - (int)Math.Round(scale.Horizontal / 2),
                    targetSize.Height / 2 - (int)Math.Round(scale.Vertical / 2));
            }

            spriteBatch.Draw(
                texture,
                location.ToXnaVector2(),
                sourceRectangle.ToXnaRectangle(),
                textureColour,
                rotation,
                origin.ToXnaVector2(),
                scale.ToXnaVector2(),
                XnaGraphicsSpriteEffects.None,
                layerDepth);
        }

        private static void SetSpriteBatchProperties(
            SpriteBatch spriteBatch,
            SpriteSortMode spriteSortMode,
            SamplerState samplerState)
        {
            bool isBatchRestartNecessary = false;

            if (spriteSortMode != currentSpriteSortMode)
            {
                currentSpriteSortMode = spriteSortMode;
                isBatchRestartNecessary = true;
            }

            if (samplerState != currentSamplerState)
            {
                currentSamplerState = samplerState;
                isBatchRestartNecessary = true;
            }

            // TODO: Is it ok to End and Begin again? Does it affect performance? It most probably does.
            if (isBatchRestartNecessary)
            {
                spriteBatch.End();
                spriteBatch.Begin(spriteSortMode, null, samplerState);
            }
        }
    }
}


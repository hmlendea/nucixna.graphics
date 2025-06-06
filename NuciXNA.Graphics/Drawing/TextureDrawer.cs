using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NuciXNA.Primitives;
using NuciXNA.Primitives.Mapping;

namespace NuciXNA.Graphics.Drawing
{
    public class TextureDrawer
    {
        private static readonly SpriteSortMode DefaultSpriteSortMode = SpriteSortMode.Deferred;
        private static readonly SamplerState DefaultSamplerState = new()
        {
            AddressU = SamplerState.LinearClamp.AddressU,
            AddressV = SamplerState.LinearClamp.AddressV,
            AddressW = SamplerState.LinearClamp.AddressW,
            BorderColor = SamplerState.LinearClamp.BorderColor,
            ComparisonFunction = SamplerState.LinearClamp.ComparisonFunction,
            Filter = SamplerState.LinearClamp.Filter,
            FilterMode = SamplerState.LinearClamp.FilterMode,
            MaxAnisotropy = SamplerState.LinearClamp.MaxAnisotropy,
            MaxMipLevel = SamplerState.LinearClamp.MaxMipLevel,
            MipMapLevelOfDetailBias = -1f
        };

        private static SpriteSortMode currentSpriteSortMode = DefaultSpriteSortMode;
        private static SamplerState currentSamplerState = DefaultSamplerState;

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
            Color textureColour = tint.ToXnaColor();
            float layerDepth = 0.0f;

            textureColour.A = (byte)(textureColour.A * opacity);

            if (textureLayout.Equals(TextureLayout.Tile))
            {
                SetSpriteBatchProperties(spriteBatch, SpriteSortMode.Deferred, SamplerState.PointWrap);

                sourceRectangle = new Rectangle2D(
                    sourceRectangle.Location.X,
                    sourceRectangle.Location.Y,
                    (int)(sourceRectangle.Size.Width * scale.Horizontal),
                    (int)(sourceRectangle.Size.Height * scale.Vertical));

                scale = Scale2D.One;
            }
            else if (textureLayout.Equals(TextureLayout.Stretch))
            {
                Size2D targetSize = sourceRectangle.Size * scale;

                SetSpriteBatchProperties(spriteBatch, DefaultSpriteSortMode, DefaultSamplerState);

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
                Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
                layerDepth);
        }

        static void SetSpriteBatchProperties(
            SpriteBatch spriteBatch,
            SpriteSortMode spriteSortMode,
            SamplerState samplerState)
        {
            bool beginBatchAgain = false;

            if (!spriteSortMode.Equals(currentSpriteSortMode))
            {
                currentSpriteSortMode = spriteSortMode;
                beginBatchAgain = true;
            }

            if (!samplerState.Equals(currentSamplerState))
            {
                currentSamplerState = samplerState;
                beginBatchAgain = true;
            }

            // TODO: Is it ok to End and Begin again? Does it affect performance? It most probably does.
            if (beginBatchAgain)
            {
                spriteBatch.End();
                spriteBatch.Begin(spriteSortMode, null, samplerState);
            }
        }
    }
}

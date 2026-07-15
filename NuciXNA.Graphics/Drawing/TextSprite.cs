using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NuciXNA.DataAccess.Content;
using NuciXNA.Primitives;
using NuciXNA.Primitives.Mapping;

namespace NuciXNA.Graphics.Drawing
{
    public sealed class TextSprite : Sprite
    {
        private SpriteFont font;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the name of the font.
        /// </summary>
        /// <value>The name of the font.</value>
        public string FontName { get; set; }

        /// <summary>
        /// Gets or sets the outline colour of the text.
        /// </summary>
        /// <value>The outline colour.</value>
        public Colour OutlineColour { get; set; }

        // TODO: Make this a number (Outline size)
        /// <summary>
        /// Gets or sets a value indicating whether the text of the <see cref="Sprite"/> will be outlined.
        /// </summary>
        /// <value><c>true</c> if the text is outlined; otherwise, <c>false</c>.</value>
        public FontOutline FontOutline { get; set; }

        /// <summary>
        /// Gets or sets the horizontal alignment of the text.
        /// </summary>
        /// <value>The horizontal alignment.</value>
        public Alignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment of the text.
        /// </summary>
        /// <value>The vertical alignment.</value>
        public Alignment VerticalAlignment { get; set; }

        /// <summary>
        /// Gets the covered screen area.
        /// </summary>
        /// <value>The covered screen area.</value>
        public override Rectangle2D ClientRectangle
            => new(ClientLocation, SpriteSize);

        /// <summary>
        /// Initializes a new instance of the <see cref="Sprite"/> class.
        /// </summary>
        public TextSprite() : base()
        {
            OutlineColour = Colour.Black;
            Text = string.Empty;
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected override void DoLoadContent()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                Text = string.Empty;
            }

            if (!string.IsNullOrEmpty(FontName))
            {
                font = NuciContentManager.Instance.LoadSpriteFont("Fonts/" + FontName);
            }

            if (SpriteSize == Size2D.Empty)
            {
                Size2D size;

                if (!string.IsNullOrEmpty(Text))
                {
                    Vector2D measuredSize = font.MeasureString(Text).ToVector2D();

                    size = new Size2D(
                        (int)measuredSize.X,
                        (int)measuredSize.Y);
                }
                else
                {
                    size = new Size2D(1, 1);
                }

                SpriteSize = size;
            }
        }

        /// <summary>
        /// Unloads the content.
        /// </summary>
        protected override void DoUnloadContent()
        {
            Text = string.Empty;
            font = null;
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        protected override void DoUpdate(GameTime gameTime) { }

        /// <summary>
        /// Draws the content.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch.</param>
        protected override void DoDraw(SpriteBatch spriteBatch)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return;
            }

            StringDrawer.Draw(
                spriteBatch,
                font,
                StringDrawer.WrapText(font, Text, SpriteSize.Width),
                ClientRectangle,
                Tint,
                OutlineColour,
                Opacity,
                HorizontalAlignment,
                VerticalAlignment,
                FontOutline);
        }
    }
}

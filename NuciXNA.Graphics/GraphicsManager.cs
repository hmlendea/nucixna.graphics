using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NuciXNA.Primitives;

namespace NuciXNA.Graphics
{
    /// <summary>
    /// Graphics Manager.
    /// </summary>
    public class GraphicsManager
    {
        static volatile GraphicsManager instance;
        static readonly Lock syncRoot = new();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static GraphicsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        instance ??= new GraphicsManager();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets or sets the graphics device manager.
        /// </summary>
        /// <value>The graphics device manager.</value>
        public GraphicsDeviceManager Graphics { get; set; }

        /// <summary>
        /// Gets or sets the sprite batch.
        /// </summary>
        /// <value>The sprite batch.</value>
        public SpriteBatch SpriteBatch { get; set; }

        public Size2D BackBufferSize => new(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
    }
}

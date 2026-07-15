using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NuciXNA.Primitives;

namespace NuciXNA.Graphics
{
    /// <summary>
    /// Graphics Manager.
    /// </summary>
    public sealed class GraphicsManager
    {
        private static volatile GraphicsManager instance;
        private static readonly Lock syncRoot = new();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static GraphicsManager Instance
        {
            get
            {
                if (instance is null)
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

        /// <summary>
        /// Gets or sets the default sampler state used by TextureDrawer for Stretch-layout sprites.
        /// Override this in your game's LoadContent to change the global texture filtering.
        /// Defaults to AnisotropicClamp for smooth scaling.
        /// </summary>
        public SamplerState DefaultSamplerState { get; set; } = SamplerState.AnisotropicClamp;

        /// <summary>
        /// Gets the back buffer size.
        /// </summary>
        /// <value>The back buffer size.</value>
        public Size2D BackBufferSize => new(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);

        private GraphicsManager() { }
    }
}

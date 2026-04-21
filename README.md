[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/fund.html) [![Latest Release](https://img.shields.io/github/v/release/hmlendea/nucixna.graphics)](https://github.com/hmlendea/nucixna.graphics/releases/latest) [![Build Status](https://github.com/hmlendea/nucixna.graphics/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nucixna.graphics/actions/workflows/dotnet.yml) [![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://gnu.org/licenses/gpl-3.0)

# NuciXNA.Graphics

Graphics helpers and sprite abstractions for NuciXNA, built on top of MonoGame/XNA.

It provides:

- a central graphics context manager (`GraphicsManager`)
- drawable sprite primitives (`TextSprite`, `TextureSprite`)
- text drawing helpers with alignment and outlines
- reusable sprite effects (movement, animation, opacity, scale, rotation)

## Installation

[![Get it from NuGet](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/nuget.png)](https://nuget.org/packages/NuciXNA.Graphics)

### .NET CLI

```bash
dotnet add package NuciXNA.Graphics
```

### Package Manager
```powershell
Install-Package NuciXNA.Graphics
```

## Requirements

- .NET: `net10.0`
- MonoGame DesktopGL (or compatible MonoGame runtime)
- `NuciXNA.DataAccess` and `NuciXNA.Primitives` (restored automatically via NuGet)

## Quick Start

### 1. Initialize the graphics context

In your game setup (after creating your `GraphicsDeviceManager` and `SpriteBatch`), assign them to the shared manager:

```csharp
using Microsoft.Xna.Framework.Graphics;
using NuciXNA.Graphics;

GraphicsManager.Instance.Graphics = graphics;
GraphicsManager.Instance.SpriteBatch = spriteBatch;
```

### 2. Create and load a sprite

```csharp
using NuciXNA.Graphics.Drawing;
using NuciXNA.Primitives;

var label = new TextSprite
{
	FontName = "Default",
	Text = "Hello NuciXNA.Graphics",
	Location = new Point2D(32, 24),
	SpriteSize = new Size2D(320, 64),
	HorizontalAlignment = Alignment.Beginning,
	VerticalAlignment = Alignment.Beginning,
	Tint = Colour.White,
	Opacity = 1f,
};

label.LoadContent();
```

### 3. Update and draw in the game loop

```csharp
label.Update(gameTime);

spriteBatch.Begin();
label.Draw(spriteBatch);
spriteBatch.End();
```

## Core Types

### GraphicsManager

Singleton used to expose:

- `GraphicsDeviceManager Graphics`
- `SpriteBatch SpriteBatch`
- `BackBufferSize`

### Sprite base class

All sprites derive from `Sprite` and provide:

- lifecycle: `LoadContent()`, `UnloadContent()`, `Update()`, `Draw()`
- transform/state: location, scale, rotation, tint, opacity
- optional effects: movement, opacity, rotation, scale

### TextSprite

Useful for UI labels and dynamic text:

- `FontName` for loading a `SpriteFont` from `Fonts/<name>`
- auto text wrapping to `SpriteSize.Width`
- horizontal/vertical alignment via `Alignment`
- optional outlining via `FontOutline` (`None`, `Around`, `BottomRight`)

### TextureSprite

Texture-based sprite with optional masking and sprite-sheet animation:

- `ContentFile` for texture asset path
- `AlphaMaskFile` for alpha blending mask
- `SourceRectangle` and `TextureLayout` (`None`, `Centre`, `Stretch`, `Tile`, `Zoom`)
- `SpriteSheetEffect` for frame-based animation

## Sprite Effects

Effects inherit from `NuciSpriteEffect<TSprite>` and are activated at runtime.

Included effects:

- `MovementEffect` - moves the sprite toward a target location
- `FadeEffect` - oscillates opacity between min/max multipliers
- `ZoomEffect` - oscillates scale between min/max multipliers
- `OscilationEffect` - oscillates rotation multiplier
- `AnimationEffect` - advances frames for sprite sheets

Example:

```csharp
using NuciXNA.Graphics.SpriteEffects;
using NuciXNA.Primitives;

var sprite = new TextureSprite
{
	ContentFile = "Characters/Hero",
	Location = new Point2D(120, 200),
	SpriteSize = new Size2D(64, 64),
	MovementEffect = new MovementEffect
	{
		Speed = 6f,
		TargetLocation = new Point2D(320, 200),
	}
};

sprite.LoadContent();
sprite.MovementEffect.Activate();
```

## Content Notes

- `TextSprite` loads fonts from `Fonts/<FontName>` via `NuciContentManager`
- `TextureSprite` expects valid content paths resolvable by the same content manager
- effects and sprites must be loaded before activation, update, or draw

## Development

### Build

```bash
dotnet build NuciXNA.Graphics.sln
```

### Test

```bash
dotnet test NuciXNA.Graphics.sln
```

### Pack

```bash
dotnet pack NuciXNA.Graphics/NuciXNA.Graphics.csproj -c Release
```

## Contributing

Contributions are welcome.

When contributing:

- keep the project cross-platform
- preserve the existing public API unless a breaking change is intentional
- keep the changes focused and consistent with the current coding style
- update the documentation when the behavior changes
- include tests for any new behavior

## Related Projects

- [NuciXNA.DataAccess](https://github.com/hmlendea/nucixna.dataaccess)
- [NuciXNA.Graphics](https://github.com/hmlendea/nucixna.graphics)
- [NuciXNA.GUI](https://github.com/hmlendea/nucixna.gui)
- [NuciXNA.Input](https://github.com/hmlendea/nucixna.input)
- [NuciXNA.Primitives](https://github.com/hmlendea/nucixna.Primitives)

## License

Licensed under the GNU General Public License v3.0 or later.
See [LICENSE](./LICENSE) for details.
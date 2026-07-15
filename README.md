[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/nucixna.graphics)](https://github.com/hmlendea/nucixna.graphics/releases/latest)
[![Build Status](https://github.com/hmlendea/nucixna.graphics/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nucixna.graphics/actions/workflows/dotnet.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://gnu.org/licenses/gpl-3.0)

# NuciXNA.Graphics

Graphics helpers and sprite abstractions for NuciXNA, built on top of MonoGame/XNA.

## ✨ Features

- Central graphics context manager (`GraphicsManager`)
- Drawable sprite primitives (`TextSprite`, `TextureSprite`)
- Text drawing with alignment, word wrapping, and outlines
- Reusable sprite effects: movement, animation, opacity, scale, rotation

## 🚀 Usage

### 1. Initialise the graphics context

In your game setup, after creating your `GraphicsDeviceManager` and `SpriteBatch`, assign them to the shared manager:

```csharp
using NuciXNA.Graphics;

GraphicsManager.Instance.Graphics = graphics;
GraphicsManager.Instance.SpriteBatch = spriteBatch;
```

### 2. Create and load a sprite

```csharp
using NuciXNA.Graphics.Drawing;
using NuciXNA.Primitives;

TextSprite label = new()
{
    FontName = "Default",
    Text = "Hello NuciXNA.Graphics",
    Location = new Point2D(32, 24),
    SpriteSize = new Size2D(320, 64),
    HorizontalAlignment = Alignment.Middle,
    VerticalAlignment = Alignment.Middle,
    Tint = Colour.White,
    Opacity = 1.0f,
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

### 4. Apply a sprite effect

```csharp
using NuciXNA.Graphics.SpriteEffects;
using NuciXNA.Primitives;

TextureSprite hero = new()
{
    ContentFile = "Characters/Hero",
    Location = new Point2D(120, 200),
    SpriteSize = new Size2D(64, 64),
    MovementEffect = new MovementEffect
    {
        Speed = 6.0f,
        TargetLocation = new Point2D(320, 200),
    }
};

hero.LoadContent();
hero.MovementEffect.Activate();
```

## 📦 Installation

[![Get it from NuGet](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/nuget.png)](https://nuget.org/packages/NuciXNA.Graphics)

### .NET CLI

```bash
dotnet add package NuciXNA.Graphics
```

### Package Manager Console

```powershell
Install-Package NuciXNA.Graphics
```

## 🛠️ Development

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MonoGame DesktopGL (or compatible MonoGame runtime)

All NuGet dependencies are restored automatically by `dotnet restore`.

### Build

```bash
dotnet build NuciXNA.Graphics/NuciXNA.Graphics.csproj
```

### Test

```bash
dotnet test NuciXNA.Graphics.slnx
```

### Dependencies

| Package | Purpose |
|---------|---------|
| `MonoGame.Framework.DesktopGL` | XNA/MonoGame runtime |
| `NuciXNA.DataAccess` | Content loading (`NuciContentManager`) |
| `NuciXNA.Primitives` | `Point2D`, `Size2D`, `Scale2D`, `Colour`, `Rectangle2D` |

### Release

```bash
dotnet pack NuciXNA.Graphics/NuciXNA.Graphics.csproj -c Release
```

## 🗂️ Project Structure

The solution contains the following projects:

- `NuciXNA.Graphics`: Core library with sprite primitives, effects, and the graphics manager
- `NuciXNA.Graphics.UnitTests`: Unit tests for the library

Key directories inside `NuciXNA.Graphics/`:

| Directory | Purpose |
|-----------|--------|
| `Drawing/` | Sprite primitives, text rendering, alignment, and outlines |
| `SpriteEffects/` | Reusable sprite effects (movement, animation, opacity, scale, rotation) |

## 🤝 Contributing

Contributions are welcome. Please:
- Keep changes cross-platform
- Keep the existing public contract intact unless a breaking change is intentional
- Keep pull requests focused and consistent with the existing code style
- Update documentation when behaviour changes
- Add unit tests for any new or changed functionality

## 🔗 Related Projects

- [NuciXNA.DataAccess](https://github.com/hmlendea/nucixna.dataaccess): Content loading utilities for NuciXNA
- [NuciXNA.GUI](https://github.com/hmlendea/nucixna.gui): GUI controls built on top of NuciXNA.Graphics
- [NuciXNA.Input](https://github.com/hmlendea/nucixna.input): Input handling for NuciXNA
- [NuciXNA.Primitives](https://github.com/hmlendea/nucixna.primitives): Primitive types used across NuciXNA

## 💝 Support

Found a bug or have a suggestion? [Open an issue](https://github.com/hmlendea/nucixna.graphics/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or giving a ⭐️ on GitHub!

## 📄 License

Licensed under the `GNU General Public License v3.0` or later.
See [LICENSE](./LICENSE) for details.
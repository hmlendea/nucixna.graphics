[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/nucixna.graphics)](https://github.com/hmlendea/nucixna.graphics/releases/latest)
[![Build Status](https://github.com/hmlendea/nucixna.graphics/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nucixna.graphics/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/NuciXNA.Graphics)](https://nuget.org/packages/NuciXNA.Graphics)
[![License](https://img.shields.io/github/license/hmlendea/nucixna.graphics)](https://github.com/hmlendea/nucixna.graphics/blob/main/LICENSE)

# NuciXNA.Graphics

Graphics helpers and sprite abstractions for NuciXNA, built on top of MonoGame/XNA.

## 📑 Table of Contents

- [Capabilities](#capabilities)
- [Usage](#usage)
- [Installation](#installation)
- [System Requirements](#system-requirements)
- [Development](#development)
- [Dependencies](#dependencies)
- [Project Structure](#project-structure)
- [Architecture](#architecture)
- [GitHub Actions](#github-actions)
- [Contributing](#contributing)
- [Code of Conduct](#code-of-conduct)
- [Related Projects](#related-projects)
- [Security](#security)
- [Project Engagement](#project-engagement)
- [License](#license)

## ✨ Capabilities

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

[![Obtain it from NuGet](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/nuget.png)](https://nuget.org/packages/NuciXNA.Graphics)

### Package Manager Installation

```bash
dotnet add package NuciXNA.Graphics
```

Or, via the `Package Manager Console`:
```powershell
Install-Package NuciXNA.Graphics
```

## 🖥️ System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| .NET SDK | 10.0 | 10.0 |
| MonoGame | DesktopGL 3.8.4 | DesktopGL 3.8.4 |
| OS | Linux, macOS, Windows | Linux, macOS, Windows |

## 🛠️ Development

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- MonoGame DesktopGL (or compatible MonoGame runtime)

### Setup

```bash
# Clone the repository
git clone https://github.com/hmlendea/nucixna.graphics.git
cd nucixna.graphics

# Restore dependencies
dotnet restore
```

### Build

```bash
dotnet build NuciXNA.Graphics/NuciXNA.Graphics.csproj
```

### Test

```bash
dotnet test NuciXNA.Graphics.slnx
```

### Formatting

```bash
dotnet format
```

## 📦 Dependencies

| Package | Version | Scope | Purpose |
|---------|---------|-------|---------|
| `MonoGame.Framework.DesktopGL` | 3.8.4 | Runtime | XNA/MonoGame runtime |
| `NuciXNA.DataAccess` | 4.0.3 | Runtime | Content loading (`NuciContentManager`) |
| `NuciXNA.Primitives` | 2.1.7 | Runtime | `Point2D`, `Size2D`, `Scale2D`, `Colour`, `Rectangle2D` |
| `Microsoft.NET.Test.Sdk` | 18.7.0 | Development | Test runner |
| `NUnit` | 4.6.1 | Development | Test framework |
| `NUnit3TestAdapter` | 6.2.0 | Development | Test adapter |

## 🗂️ Project Structure

The solution contains the following projects:

| Project | Type | Purpose |
|---------|------|---------|
| `NuciXNA.Graphics` | Library | Core library with sprite primitives, effects, and the graphics manager |
| `NuciXNA.Graphics.UnitTests` | Test | Unit tests for the library |

### Directories

| Directory | Purpose |
|-----------|--------|
| `NuciXNA.Graphics/Drawing/` | Sprite primitives, text rendering, alignment, and outlines |
| `NuciXNA.Graphics/SpriteEffects/` | Reusable sprite effects (movement, animation, opacity, scale, rotation) |

## 🏗️ Architecture

NuciXNA.Graphics follows a library/SDK architecture with a singleton `GraphicsManager`, composable `Sprite` hierarchy, and stateless drawing helpers.

See the [architecture documentation](ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [ dotnet.yml ](https://github.com/hmlendea/nucixna.graphics/blob/main/.github/workflows/dotnet.yml) | Continuous integration | Restores dependencies, builds the solution, and runs unit tests on every push and pull request to `master` |

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Preserve the existing public contract unless a breaking change is intentional
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Add tests for additional or modified functionality

See the [contributing guidelines](CONTRIBUTING.md) for details on how to report issues, suggest enhancements, and submit changes.

## 📜 Code of Conduct

See the [Code of Conduct](CODE_OF_CONDUCT.md) for the standards of conduct expected in all project spaces.

## 🔗 Related Projects

- [NuciXNA.DataAccess](https://github.com/hmlendea/nucixna.dataaccess): Content loading utilities for NuciXNA
- [NuciXNA.GUI](https://github.com/hmlendea/nucixna.gui): GUI controls built on top of NuciXNA.Graphics
- [NuciXNA.Input](https://github.com/hmlendea/nucixna.input): Input handling for NuciXNA
- [NuciXNA.Primitives](https://github.com/hmlendea/nucixna.primitives): Primitive types used across NuciXNA

## 🔒 Security

NuciXNA.Graphics contains no network communication, telemetry, or external data processing; its attack surface is limited to rendering logic and content-loading helpers.

For information on reporting security vulnerabilities, see [SECURITY.md](SECURITY.md).

## 💝 Project Engagement

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/nucixna.graphics/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License v3.0` or later.
See [LICENSE](./LICENSE) for further information.
# Contributing to NuciXNA.Graphics

This document covers the guidelines and processes for contributing to this project, including how to report issues, suggest enhancements, submit code changes, and follow project standards.

## 📑 Table of Contents

- [How to Contribute](#how-to-contribute)
- [Code Style](#code-style)
- [Testing](#testing)
- [Documentation](#documentation)
- [Code of Conduct](#code-of-conduct)
- [Security](#security)
- [License](#license)
- [Getting Help](#getting-help)

## 🤝 How to Contribute

### Reporting Issues

- Search existing issues first.
- Use the issue templates if available.
- Provide clear reproduction steps.
- Include environment details.

### Suggesting Enhancements

- Check the roadmap and existing discussions.
- Explain the use case and expected behavior.
- Consider implementation complexity.

### Code Contributions

#### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MonoGame DesktopGL (or compatible MonoGame runtime)

#### Development Setup

```bash
# Clone the repository
git clone https://github.com/hmlendea/nucixna.graphics.git
cd nucixna.graphics

# Restore dependencies
dotnet restore
```

#### Making Changes

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Make your changes.
4. Run tests: `dotnet test NuciXNA.Graphics.slnx`
5. Run build: `dotnet build NuciXNA.Graphics/NuciXNA.Graphics.csproj`
6. Commit with clear and descriptive messages.
7. Push to your fork.
8. Open a Pull Request.

#### Pull Request Guidelines

- Target the `main` branch.
- Keep PRs focused and atomic.
- Update documentation if applicable.
- Add tests for any new functionality.
- Ensure the CI checks pass.

## 🎨 Code Style

Follow the project's coding standards:
- C# coding conventions per `csharp.coding.instructions.md`
- Naming conventions per `csharp.naming.instructions.md`
- Indentation rules per `csharp.indentation.instructions.md`
- Run `dotnet format` before committing.

## 🧪 Testing

```bash
# Run all tests
dotnet test NuciXNA.Graphics.slnx

# Run specific test project
dotnet test NuciXNA.Graphics.UnitTests/NuciXNA.Graphics.UnitTests.csproj
```

## 📝 Documentation

- Update relevant docs for changes.
- Follow the documentation style guide.
- Preview changes locally if possible.
- Update `README.md`, `ARCHITECTURE.md`, `PRIVACY.md`, `SECURITY.md` when behaviour changes.

## 📋 Code of Conduct

This project follows the [Code of Conduct](CODE_OF_CONDUCT.md).

## 🔒 Security

Report security vulnerabilities per the [Security Policy](SECURITY.md).

## 📄 License

By contributing, you agree that your contributions will be licensed under the `GNU General Public License v3.0` or later.

## ❓ Getting Help

- [Open an issue](https://github.com/hmlendea/nucixna.graphics/issues)
- Check existing discussions and documentation first.
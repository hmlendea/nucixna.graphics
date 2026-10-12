# Privacy and Personal Data

NuciXNA.Graphics is a .NET class library for MonoGame/XNA graphics rendering. It contains no telemetry, analytics, network communication, or personal data collection. The library runs entirely within the host application process and does not transmit data to any external destination.

**Information reviewed:** 2026-10-12

## 📑 Table of Contents

- [What This Document Covers](#what-this-document-covers)
- [Self-Hosted Deployments](#self-hosted-deployments)
- [Data We Handle](#data-we-handle)
- [Processing and Use](#processing-and-use)
- [Storage, Retention, and Deletion](#storage-retention-and-deletion)
- [External Processing and Integrations](#external-processing-and-integrations)
- [Document Changes](#document-changes)
- [Contact](#contact)

## 🔎 What This Document Covers

This document describes how NuciXNA.Graphics at https://github.com/hmlendea/nucixna.graphics handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 🏠 Self-Hosted Deployments

NuciXNA.Graphics is a class library distributed via NuGet. Developers integrate it into their own MonoGame/XNA applications. The library itself does not operate as a service and does not send data to project maintainers or external services. The host application developer (instance operator) controls all data processed by their application, including any data passed to NuciXNA.Graphics APIs.

No telemetry, update checks, crash reports, or network requests originate from NuciXNA.Graphics.

## 📥 Data We Handle

### Data Provided to the Application

NuciXNA.Graphics does not request, collect, or process personal data. The library accepts only rendering parameters provided by the host application: text strings, font names, texture paths, coordinates, colours, and transformation values. These are transient rendering inputs, not personal data.

### Data Generated or Collected by the Application

NuciXNA.Graphics generates no personal data. It produces only in-memory rendering state (vertex buffers, texture references, draw calls) that exists for the duration of a frame and is discarded after presentation.

### Data Received from Integrations

NuciXNA.Graphics receives no personal data from integrations or third parties. It depends on MonoGame.Framework.DesktopGL, NuciXNA.DataAccess, and NuciXNA.Primitives for rendering and content loading; none of these dependencies transmit data externally.

## 🧭 Processing and Use

The application processes the data described above for these verified functions:
- Text and texture rendering — transient rendering parameters (text, font, texture, position, colour, transform)
- Sprite effect animation — transient effect parameters (speed, target, duration, easing)

No personal data is processed, stored, or transmitted.

## 🗄️ Storage, Retention, and Deletion

NuciXNA.Graphics stores no data persistently. All rendering state is held in managed memory for the lifetime of the host application's draw cycle and is eligible for garbage collection afterward. The host application controls all storage, retention, and deletion of its own data.

## 🔗 External Processing and Integrations

NuciXNA.Graphics has no built-in external data transfer. It references the following NuGet packages at compile time only:

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| MonoGame.Framework.DesktopGL | XNA/MonoGame runtime | None | https://www.monogame.net/ |
| NuciXNA.DataAccess | Content loading (NuciContentManager) | None | https://github.com/hmlendea/nucixna.dataaccess |
| NuciXNA.Primitives | Primitive types (Point2D, Size2D, Colour, etc.) | None | https://github.com/hmlendea/nucixna.primitives |

## 🛡️ Data Protection and Security

NuciXNA.Graphics performs no I/O, network access, or cryptographic operations. It does not handle secrets, credentials, or authentication tokens. Security of the host application and its data is the responsibility of the application developer.

## 🔄 Document Changes

Update this document when application data flows, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/nucixna.graphics/blob/main/PRIVACY.md.

## 📬 Contact

For questions about application data handling, contact the project maintainers. For a self-hosted instance, contact the instance operator, unless the project explicitly handles the request. Include the repository name (nucixna.graphics) and NuGet package version if applicable; do not send passwords, access tokens, or other secrets.
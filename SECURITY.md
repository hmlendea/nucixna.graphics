# Security Policy

NuciXNA.Graphics is a .NET class library for MonoGame/XNA graphics rendering. This policy covers vulnerability reporting and disclosure for the library code distributed via NuGet. The library contains no network communication, telemetry, or external data processing; its attack surface is limited to rendering logic and content-loading helpers.

## 📑 Table of Contents

- [Supported Versions](#supported-versions)
- [Reporting a Vulnerability](#reporting-a-vulnerability)
- [Scope](#scope)
- [Disclosure Policy](#disclosure-policy)
- [Safe Harbour](#safe-harbour)
- [Recognition](#recognition)

## 🛡️ Supported Versions

Use this table to indicate which project versions currently receive security maintenance.

| Version | Distribution Channel | Supported |
|---------|--------------------|-----------|
| Latest version | NuGet | ✅ |
| Preceding versions | NuGet | ❌ |

## 🚨 Reporting a Vulnerability

Please do not disclose suspected vulnerabilities publicly before maintainers have had an opportunity to validate and remediate them.

To report a vulnerability:
- [GitHub Security Advisories](https://github.com/hmlendea/nucixna.graphics/security/advisories)
- Contact the maintainers directly

## 📌 Scope

The subsequent report categories are in scope for this repository:
- Vulnerabilities in NuciXNA.Graphics library code (rendering, sprite effects, content loading, graphics manager)
- Vulnerabilities in the NuGet package build or publication pipeline

The subsequent categories are out of scope unless explicitly stated to the contrary:
- Vulnerabilities in host applications that consume NuciXNA.Graphics
- Vulnerabilities in MonoGame.Framework.DesktopGL or upstream MonoGame/XNA
- Vulnerabilities in NuciXNA.DataAccess or NuciXNA.Primitives (separate repositories)
- Denial-of-service via resource exhaustion in host application render loops
- Issues requiring physical access or host OS compromise

## 📢 Disclosure Policy

This project follows coordinated disclosure:
1. Vulnerabilities are investigated privately.
2. A remediation plan is prepared and validated.
3. Public disclosure is published after a fix, mitigation, or agreed risk decision is available.
4. Credit is attributed in accordance with reporter preference and project policy.

## 🧾 Safe Harbour

If your research is conducted in good faith, confined to authorised scope, and disclosed responsibly, the maintainers will not pursue action for policy-compliant activity.

## 🙏 Recognition

We appreciate responsible disclosure. Reporters who desire public attribution may be acknowledged in release notes, advisories, or a dedicated acknowledgements section.
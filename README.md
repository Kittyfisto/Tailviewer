<div align="center">
    <p>[English | <a href="README_zh-CN.md">简体中文</a>]</p>
    <!--icon TBD-->
    <h1>Tailviewer NEXT</h1>
    <p>An easy-to-use, high-performance log analysis tool, designed for large real-time logs</p>
    <p>Forked from <a href="https://github.com/Kittyfisto/Tailviewer">Kittyfisto/Tailviewer</a></p>
    <p>
        <a href="https://github.com/brofea/Tailviewer-NEXT/actions/workflows/ci.yml">
            <img src="https://github.com/brofea/Tailviewer-NEXT/actions/workflows/ci.yml/badge.svg" alt="CI Status">
        </a>
        <a href="https://www.gnu.org/licenses/gpl-3.0.en.html">
            <img src="https://img.shields.io/badge/license-GPL--3.0-orange" />
        </a>
        <a href="https://github.com/brofea/Tailviewer-NEXT/releases/latest">
            <img src="https://img.shields.io/github/v/release/brofea/Tailviewer-NEXT?label=release" alt="Release Version">
        </a>
        <a href="https://github.com/brofea">
            <img src="https://img.shields.io/badge/brofea-brofea?label=GitHub&logo=github&color=purple" alt="GitHub Profile">
        </a>
    </p>
</div>

Treat logs as a structured event stream, providing powerful yet easy-to-use search, filtering, multi-line log recognition, real-time tailing, and multi-log merging capabilities, helping developers quickly locate problems in massive runtime data.

Requires Windows and .NET 4 runtime support.

<img width="1448" height="843" alt="image" src="https://github.com/user-attachments/assets/8127b3fb-cc32-4f84-ba5f-16555fcad3b4" />

## Core Features

- 🚀 **High-performance log browsing**
    - Fast loading and browsing of GB-scale large log files
- 🔍 **Powerful filtering and search**
    - Log level, keyword, regular expression, time range
- 🧩 **Smart log parsing**
    - Recognizes multi-line log events, fully displays complex log structures
- 🔄 **Real-time log tailing**
    - Monitor running services and applications
- 📚 **Multi-log file merging**
    - Merge by timeline, suitable for distributed systems
- 🔌 **Extensible plugin architecture**
    - Supports custom log format parsing for different projects and enterprises

## Use Cases

- Backend service troubleshooting
- Distributed system log analysis
- Game server debugging
- Automated test result analysis
- Production environment issue localization

## Why NEXT

The original Tailviewer is already very powerful, but has not been maintained for years. This project is dedicated to fixing bugs in the original project and adding more modern features.

The following features have been added so far, with more possibly coming in the future:

- ✅ **Dark mode**
    - Supports following the system theme
- ✅ **AND OR logic toggle**
    - Precisely filter desired content even without regular expressions
- ✅ **Configurable theme color**
    - Meets different aesthetic needs

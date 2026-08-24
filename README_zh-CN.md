<div align="center">
    <p>[<a href="README.md">English</a> | 简体中文]</p>
    <!--icon 待添加-->
    <h1>Tailviewer NEXT</h1>
    <p>易用的高性能日志分析工具，专为大型实时日志设计</p>
    <p>继承自 <a href="https://github.com/Kittyfisto/Tailviewer">Kittyfisto/Tailviewer</a></p>
    <p>
        <a href="https://github.com/brofea/Tailviewer-NEXT/actions/workflows/ci.yml">
            <img src="https://github.com/brofea/Tailviewer-NEXT/actions/workflows/ci.yml/badge.svg" alt="CI Status">
        </a>
        <a href="https://mit-license.org/">
            <img src="https://img.shields.io/badge/license-MIT-orange" />
        </a>
        <a href="https://github.com/brofea/Tailviewer-NEXT/releases/latest">
            <img src="https://img.shields.io/github/v/release/brofea/Tailviewer-NEXT?label=release" alt="Release Version">
        </a>
        <a href="https://github.com/brofea">
            <img src="https://img.shields.io/badge/brofea-brofea?label=GitHub&logo=github&color=purple" alt="GitHub Profile">
        </a>
    </p>
</div>

将日志视为结构化事件流，提供强大且易用的搜索、过滤、多行日志识别、实时跟踪和多日志合并能力，帮助开发者从海量运行数据中快速定位问题。

需要 Windows 以及 .NET 4 运行时支持。

<img width="1448" height="843" alt="image" src="https://github.com/user-attachments/assets/8127b3fb-cc32-4f84-ba5f-16555fcad3b4" />

## 核心特性

- 🚀 **高性能日志浏览**
    - GB 级大型日志文件快速加载与浏览
- 🔍 **强大的过滤与搜索**
    - 日志等级、关键字、正则表达式、时间范围
- 🧩 **智能日志解析**
    - 识别多行日志事件，完整展示复杂日志结构
- 🔄 **实时日志跟踪**
    - 监控运行中的服务和应用
- 📚 **多日志文件合并**
    - 按时间线合并，适用于分布式系统
- 🔌 **可扩展插件架构**
    - 支持自定义日志格式解析，适配不同项目和企业

## 适用场景

- 后端服务故障排查
- 分布式系统日志分析
- 游戏服务器调试
- 自动化测试结果分析
- 生产环境问题定位

## 为什么要做 NEXT

原 Tailviewer 功能上已经非常强大，但已经多年没有维护，本项目致力于修复原项目的 bug，并增加更多现代化功能

目前添加了如下功能，未来可能加入更多：

- ✅ **深色模式**
    - 支持跟随系统主题
- ✅ **AND OR 逻辑切换**
    - 即使不使用正则表达式，也可以精准过滤想要的内容
- ✅ **可配置的主题色**
    - 满足不同审美需求


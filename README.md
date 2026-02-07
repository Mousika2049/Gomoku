# 🎮 Gomoku AI / 五子棋 AI

<div align="center">

[English](#english) | [中文](#chinese)

</div>

---

## <a name="english"></a>🇬🇧 English

### 📖 Introduction

A cross-platform Gomoku (Five in a Row) game powered by .NET MAUI, featuring a traditional AI opponent based on the Minimax algorithm with alpha-beta pruning. Play against an intelligent computer opponent with three difficulty levels!

### ✨ Features

- 🤖 **Smart AI Opponent**: Traditional AI using Minimax algorithm with alpha-beta pruning
- 🎯 **Three Difficulty Levels**: 
  - 🐣 Easy Mode (Depth 2) - Perfect for beginners
  - 🐯 Medium Mode (Depth 3) - For intermediate players
  - 🐲 Hard Mode (Depth 4) - Challenge yourself!
- ⚫⚪ **Choose Your Color**: Play as Black (first move) or White (second move)
- ↩️ **Undo Function**: Take back your moves (2 steps at a time)
- 🌐 **Cross-Platform**: Works on Windows, Android, iOS, and macOS
- 🎨 **Beautiful UI**: Clean and intuitive game interface with visual feedback
- ⚡ **Optimized Performance**: 
  - Multi-threaded AI calculations using parallel processing
  - Efficient search window optimization
  - Memory-optimized board representation using byte arrays

### 🛠️ Tech Stack

- **Framework**: .NET MAUI (Multi-platform App UI)
- **Language**: C# (.NET 10)
- **AI Algorithm**: Minimax with Alpha-Beta Pruning
- **Platforms**: Windows, Android, iOS, macOS
- **UI**: XAML with custom graphics using GraphicsView

### 📋 Requirements

- .NET 10 SDK
- For Windows: Windows 10 version 1809 or higher
- For Android: Android 5.0 (API 21) or higher
- For iOS/macOS: iOS 15.0+ / macOS 15.0+

### 🚀 Getting Started

#### Building the Project

```bash
# Clone the repository
git clone https://github.com/Mousika2049/Gomoku.git
cd Gomoku

# Restore dependencies
dotnet restore

# Build for your platform
dotnet build

# Run on Windows
dotnet build -t:Run -f net10.0-windows

# Run on Android (requires Android SDK)
dotnet build -t:Run -f net10.0-android

# Run on iOS (requires macOS and Xcode)
dotnet build -t:Run -f net10.0-ios
```

### 🎮 How to Play

1. **Launch the app** and select a difficulty level
2. **Choose your color**: Black (先手/first) or White (后手/second)
3. **Make your move** by tapping on the board
4. The AI will automatically make its move
5. **Win condition**: Get 5 pieces in a row (horizontal, vertical, or diagonal)
6. Use the **Undo button** to take back moves
7. Use the **Restart button** to start a new game

### 🗺️ Roadmap

- [x] Traditional AI with Minimax algorithm
- [x] Cross-platform MAUI application
- [x] Multiple difficulty levels
- [x] Undo functionality
- [ ] Machine Learning based AI (planned future iteration)
- [ ] Online multiplayer mode
- [ ] Game statistics and history
- [ ] Custom board themes
- [ ] Web version (under consideration)

### 🤝 Contributing

Contributions are welcome! Feel free to:
- Report bugs
- Suggest new features
- Submit pull requests
- Improve documentation

### 📄 License

This project is open source. Feel free to use and modify it for your own purposes.

### 👨‍💻 Author

Mousika2049

---

## <a name="chinese"></a>🇨🇳 中文

### 📖 项目介绍

这是一个基于 .NET MAUI 开发的跨平台五子棋游戏，采用传统的 Minimax 算法配合 Alpha-Beta 剪枝实现智能 AI 对手。支持三种难度等级，让你在不同平台上都能享受五子棋的乐趣！

### ✨ 功能特色

- 🤖 **智能 AI 对手**：基于 Minimax 算法和 Alpha-Beta 剪枝的传统 AI
- 🎯 **三种难度等级**：
  - 🐣 简单模式（深度 2）- 适合新手
  - 🐯 中等模式（深度 3）- 适合进阶玩家
  - 🐲 困难模式（深度 4）- 挑战自我！
- ⚫⚪ **自由选择执子**：可选择执黑（先手）或执白（后手）
- ↩️ **悔棋功能**：支持悔棋（一次悔两步）
- 🌐 **跨平台支持**：支持 Windows、Android、iOS 和 macOS
- 🎨 **精美界面**：简洁直观的游戏界面，带有视觉反馈
- ⚡ **性能优化**：
  - 多线程并行计算 AI 落子
  - 智能搜索窗口优化
  - 使用字节数组优化内存占用

### 🛠️ 技术栈

- **框架**：.NET MAUI (多平台应用 UI)
- **语言**：C# (.NET 10)
- **AI 算法**：带 Alpha-Beta 剪枝的 Minimax 算法
- **支持平台**：Windows、Android、iOS、macOS
- **界面**：XAML + GraphicsView 自定义绘图

### 📋 环境要求

- .NET 10 SDK
- Windows 平台：Windows 10 版本 1809 或更高
- Android 平台：Android 5.0 (API 21) 或更高
- iOS/macOS 平台：iOS 15.0+ / macOS 15.0+

### 🚀 快速开始

#### 构建项目

```bash
# 克隆仓库
git clone https://github.com/Mousika2049/Gomoku.git
cd Gomoku

# 恢复依赖
dotnet restore

# 构建项目
dotnet build

# 在 Windows 上运行
dotnet build -t:Run -f net10.0-windows

# 在 Android 上运行（需要 Android SDK）
dotnet build -t:Run -f net10.0-android

# 在 iOS 上运行（需要 macOS 和 Xcode）
dotnet build -t:Run -f net10.0-ios
```

### 🎮 游戏玩法

1. **启动应用**并选择难度等级
2. **选择执子颜色**：黑棋（先手）或白棋（后手）
3. **点击棋盘**进行落子
4. AI 会自动完成它的回合
5. **获胜条件**：横、竖、斜任意方向连成 5 子
6. 使用**悔棋按钮**撤销步数
7. 使用**重新开始按钮**开启新局

### 🗺️ 开发路线图

- [x] 基于 Minimax 算法的传统 AI
- [x] 跨平台 MAUI 应用
- [x] 多难度等级
- [x] 悔棋功能
- [ ] 基于机器学习的 AI（计划中的未来迭代）
- [ ] 在线对战模式
- [ ] 游戏统计和历史记录
- [ ] 自定义棋盘主题
- [ ] Web 版本（考虑中）

### 🤝 参与贡献

欢迎各种形式的贡献：
- 报告 Bug
- 提出新功能建议
- 提交 Pull Request
- 完善文档

### 📄 开源协议

本项目为开源项目，可自由使用和修改。

### 👨‍💻 作者

Mousika2049

---

<div align="center">

**⭐ 如果这个项目对你有帮助，请给个 Star！**

**⭐ If you find this project helpful, please give it a star!**

</div>

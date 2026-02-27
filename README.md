# TextMeshPro Localization Extension

这是一个为 Unity TextMeshPro (TMP) 提供本地化支持的插件。它遵循 Unity Package Manager (UPM) 的结构规范，旨在提供高性能且易于扩展的本地化方案。

## 特性

- **解耦设计**：核心逻辑（Runtime）与具体实现（Samples）完全分离，你可以轻松定制自己的加载逻辑。
- **跨平台兼容**：默认示例采用 `Resources` 系统加载，支持 Android、iOS、PC 等全平台。
- **UPM 兼容**：符合 Unity 包管理规范，支持通过 Package Manager 导入示例内容。
- **高性能**：基于 ID 索引和弱引用管理组件，减少内存占用和 GC 压力。

## 目录结构

```text
LocalizationExtension/
├── Editor/            # 编辑器扩展脚本
├── Runtime/           # 本地化核心逻辑 (不依赖具体加载方式)
│   ├── Core/          # 接口定义 (ILocalizationLoader 等)
│   └── ...            # 运行时扩展
├── Samples~/          # 示例内容 (通过 Package Manager 导入)
│   ├── Resources/     # 本地化数据 (Language.bytes)
│   ├── Scripts/       # 示例加载器和初始化脚本
│   └── ...
└── package.json       # 包信息及 Samples 配置
```

## 安装与使用

### 1. 导入包
将 `LocalizationExtension` 文件夹放置在项目的 `Packages` 目录下，或者作为本地包导入。

### 2. 导入示例
在 Unity 的 **Package Manager** 窗口中找到 `TextMeshPro Localization Extension`，点击 **Samples** 选项卡下的 **Import** 按钮导入示例。

### 3. 初始化
你需要实现 `ILocalizationLoader` 接口并调用 `Localization.Initialize` 来初始化系统：

```csharp
using TMPro.Localization;

// 在你的游戏启动逻辑中
Localization.Initialize(new YourCustomLoader());
```

*注意：如果你导入了示例，[LocalizationInitializer.cs](file:///Samples~/Scripts/LocalizationInitializer.cs) 会自动使用示例加载器进行初始化。*

### 4. 设置文本
你可以通过 ID 或直接在 TMP 组件上绑定本地化逻辑：

```csharp
using TMPro;
using TMPro.Localization;

public TMP_Text myText;

// 通过代码设置
Localization.SetTextWithId(myText, 1001); 
```

## 贡献者

- **Author**: WJQ

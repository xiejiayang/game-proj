# 都江堰水利工程主题解谜游戏 — 架构规范

> 版本：V0.1  
> 日期：2026-06-25  
> 适用范围：Vertical Slice（L1）及后续 v1.0  
> 目标：约束技术栈、目录结构、数据原型、服务层、AI 机制、开发红线与验收标准

---

## 1. 技术栈

### 1.1 引擎与渲染
- **引擎**：Unity 6.3 LTS
- **渲染管线**：URP（Universal Render Pipeline）
- **脚本语言**：C#（目标框架 .NET Standard 2.1）
- **UI 框架**：uGUI / UI Toolkit（优先 uGUI，复杂列表可用 UI Toolkit）
- **导出目标**：WebGL（网页），后续通过 Unity 微信小游戏转换工具导出微信小程序

### 1.2 开发工具
- **IDE**：Visual Studio / Rider / VS Code
- **版本控制**：Git
- **依赖管理**：Unity Package Manager + 手动导入 Asset Store 包
- **构建**：Unity Build Pipeline，WebGL 目标平台

### 1.3 第三方资源
- **Asset Store**：低多边形自然/建筑包（需确认可商用授权）
- **字体**：Noto Serif SC / Source Han Serif SC（OFL 开源协议）
- **AI 生成**：Midjourney / Stable Diffusion（仅用于 2D 概念/叙事/剪影）

---

## 2. 项目目录结构

```
Assets/
├── _Project/                    # 项目专属资源
│   ├── Scenes/                  # 场景文件
│   │   ├── Boot.unity           # 启动场景：初始化服务、加载资源
│   │   ├── MainMenu.unity       # 标题/主菜单
│   │   └── Level_L1.unity       # L1 关卡场景
│   ├── Scripts/                 # 代码
│   │   ├── Core/                # 核心玩法系统
│   │   │   ├── PuzzleSystem.cs
│   │   │   ├── WaterSimulation.cs
│   │   │   ├── BlockPlacement.cs
│   │   │   └── LevelResult.cs
│   │   ├── UI/                  # UI 逻辑
│   │   │   ├── Screens/         # 页面级 UI
│   │   │   ├── Components/      # 可复用组件
│   │   │   └── UIManager.cs
│   │   ├── Systems/             # 全局单例服务
│   │   │   ├── SaveSystem.cs
│   │   │   ├── AudioSystem.cs
│   │   │   └── InputSystem.cs
│   │   ├── Data/                # 数据定义
│   │   │   ├── LevelConfigSO.cs
│   │   │   ├── BlockConfigSO.cs
│   │   │   └── RuntimeData.cs
│   │   └── Utils/               # 工具类
│   ├── ScriptableObjects/       # 编辑器配置资产
│   │   ├── Levels/              # 关卡配置
│   │   └── Blocks/              # 构件配置
│   ├── Prefabs/                 # 预制体
│   │   ├── UI/                  # UI 预制体
│   │   ├── Blocks/              # 构件预制体
│   │   └── Effects/             # 特效预制体
│   ├── Art/                     # 美术资源
│   │   ├── Materials/
│   │   ├── Models/
│   │   ├── Shaders/
│   │   ├── Textures/
│   │   ├── UI/
│   │   └── AI_Generated/        # AI 生成资产（必须带元数据）
│   ├── Audio/
│   │   ├── Music/
│   │   └── SFX/
│   ├── Resources/               # 运行时加载资源
│   │   └── LoadingScreens/
│   └── StreamingAssets/         # 外部可读数据（JSON 备份等）
└── Plugins/                     # 第三方插件
```

### 2.1 目录约定
- 所有项目代码和资源必须放在 `_Project` 下，不与 Asset Store 导入的包混排。
- `Scripts` 按职责分层，禁止写大而全的 Manager。
- 场景文件按功能命名，关卡场景以 `Level_` 前缀。
- 预制体、材质、纹理按用途分子目录。

---

## 3. 数据原型

### 3.1 LevelConfigSO（关卡配置）
```csharp
[CreateAssetMenu(fileName = "LevelConfig_L1", menuName = "Dujiangyan/LevelConfig")]
public class LevelConfigSO : ScriptableObject
{
    public string id;                    // 关卡唯一标识，如 "L1"
    public int act;                      // 幕号：1|2|3
    public string title;                 // 关卡名称，如「堵】
    public float targetDuration;         // 目标时长（分钟）
    public TerrainConfig terrain;        // 地形配置
    public WaterSourceConfig waterSource;// 水源参数
    public VillageConfig village;        // 村庄位置与存活条件
    public BlockInventory inventory;     // 构件库存
    public ResourceCost resourceLimit;   // 料/工/时上限
    public ResourceCost frugalThreshold; // 节俭通关阈值
    public HintNode[] hintTree;          // 提示树节点
    public NarrativeBeat[] narrative;    // 叙事节拍
    public string[] galleryUnlocks;      // 通关必解锁碑廊条目
    public string[] hiddenGalleryUnlocks;// 节俭解锁碑廊条目
}
```

### 3.2 BlockConfigSO（构件配置）
```csharp
[CreateAssetMenu(fileName = "BlockConfig_Bamboo", menuName = "Dujiangyan/BlockConfig")]
public class BlockConfigSO : ScriptableObject
{
    public string id;           // 构件 ID，如 "bamboo"
    public string displayName;  // 显示名，如「竹笼】
    public BlockType type;      // Bamboo / Maocha / Wall
    public float maxHealth;     // 最大耐久
    public ResourceCost cost;   // 消耗资源
    public WaterInteraction interaction; // 水流交互类型
    public GameObject prefab;   // 3D 预制体
}
```

### 3.3 PuzzleRuntime（运行时数据）
```csharp
[System.Serializable]
public class PuzzleRuntime
{
    public string levelId;
    public PuzzleState state;                   // Editing / Simulating / Settling / Paused
    public List<BlockInstance> placedBlocks;    // 已放置构件
    public BlockInventory inventory;            // 剩余库存
    public ResourceCost consumedResource;       // 已消耗资源
    public List<EditAction> undoStack;          // 撤销栈
    public float simulationTime;                // 当前模拟时长
    public int villageHitCount;                 // 村庄受击粒子累计
    public PuzzleResult result;                 // 模拟结果
}
```

### 3.4 BlockInstance（构件实例）
```csharp
[System.Serializable]
public class BlockInstance
{
    public string instanceId;   // 运行时唯一 ID
    public string blockId;      // 对应 BlockConfigSO.id
    public Vector3 position;    // 世界坐标
    public int rotStep;         // 0=0°, 1=90°, 2=180°, 3=270°
    public float health;        // 当前耐久
    public float maxHealth;     // 最大耐久
}
```

### 3.5 PuzzleResult（结算结果）
```csharp
[System.Serializable]
public class PuzzleResult
{
    public bool isSuccess;
    public bool isFrugal;
    public FailReason failReason;       // None / Flood / Destroyed / Timeout
    public ResourceCost consumedResource;
    public float simulationTime;
    public string[] unlockedGallery;
}
```

### 3.6 PlayerProfile（玩家档案）
```csharp
[System.Serializable]
public class PlayerProfile
{
    public string lastPlayedLevelId;
    public HashSet<string> unlockedLevels;
    public HashSet<string> unlockedGallery;
    public GameSettings settings;
}
```

### 3.7 配置与运行时数据关系
- `LevelConfigSO` 和 `BlockConfigSO` 是只读配置，由策划在编辑器中配置。
- `PuzzleRuntime` 是运行时生成的可变数据，每局独立。
- `PlayerProfile` 是持久化数据，由 `SaveSystem` 读写。

---

## 4. 服务层约定

### 4.1 架构模式
- **单例模式（Singleton）**：Vertical Slice 使用单例服务，降低复杂度。
- **事件驱动**：服务之间通过 C# `event` 或 `UnityEvent` 通信，避免直接互相引用。
- 所有单例服务放在 `Assets/_Project/Scripts/Systems/`，命名后缀 `System`。

### 4.2 核心服务

#### SaveSystem
```csharp
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    public void SaveProfile(PlayerProfile profile);
    public PlayerProfile LoadProfile();
    public void SaveLevelProgress(string levelId, PuzzleRuntime runtime);
    public PuzzleRuntime LoadLevelProgress(string levelId);
}
```
- 切片阶段使用 `PlayerPrefs`（WebGL 中映射为 `localStorage`）。
- v1.0 可选接入微信云存档。
- 所有保存操作先写本地，再异步同步云端（如有）。

#### AudioSystem
```csharp
public class AudioSystem : MonoBehaviour
{
    public static AudioSystem Instance { get; private set; }

    public void PlayMusic(AudioClip clip, bool loop = true);
    public void PlaySFX(AudioClip clip);
    public void SetMusicVolume(float volume);
    public void SetSFXVolume(float volume);
}
```
- 音乐和音效分轨。
- 音量设置保存到 `PlayerProfile`。

#### InputSystem
```csharp
public class InputSystem : MonoBehaviour
{
    public static InputSystem Instance { get; private set; }

    public event Action<Vector2> OnPointerDown;
    public event Action<Vector2> OnPointerMove;
    public event Action<Vector2> OnPointerUp;

    public bool IsTouchDevice { get; }
}
```
- 封装触摸和鼠标输入，统一返回屏幕坐标。
- 拖拽检测由 `BlockPlacement` 订阅事件处理。

#### UIManager
```csharp
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public void ShowScreen(ScreenType screen);
    public void HideScreen(ScreenType screen);
    public void ShowModal(ModalType modal);
    public void ShowResult(PuzzleResult result);
}
```
- 管理所有页面和弹窗的显示/隐藏。
- 使用栈结构管理弹窗层级。

#### PuzzleSystem
```csharp
public class PuzzleSystem : MonoBehaviour
{
    public static PuzzleSystem Instance { get; private set; }

    public event Action OnEditingStarted;
    public event Action OnSimulationStarted;
    public event Action<PuzzleResult> OnLevelSettled;

    public PuzzleRuntime InitLevel(string levelId);
    public PlaceResult TryPlaceBlock(string blockId, Vector3 worldPos, int rotStep);
    public SimulationResult StartSimulation();
    public bool Undo();
    public void Reset();
}
```
- 核心解谜状态机。
- 协调 `WaterSimulation`、`BlockPlacement`、`LevelResult`。

### 4.3 事件约定
- 事件命名：`On[Subject][Verb]`，如 `OnSimulationStarted`、`OnLevelSettled`。
- 事件参数尽量使用数据对象，避免多个零散参数。
- 订阅事件的服务在 `OnDestroy` 中取消订阅，避免内存泄漏。

---

## 5. AI 资产机制

### 5.1 AI 使用边界

| 可以用 AI | 不建议/禁止用 AI |
|---|---|
| 角色剪影、概念图、叙事插画 | 最终 3D 模型 |
| 纹理贴图的原型/灵感 | 核心玩法代码 |
| 背景氛围图、UI 装饰元素 | 关卡设计逻辑 |
| 音效参考/占位 | 最终音效（除非确认授权） |

### 5.2 AI 资产管线
1. **风格定义**：所有 AI 生成必须使用统一关键词：`水墨`、`淡彩`、`宣纸`、`宋代山水`、`留白`。
2. **输出审查**：AI 生成结果必须人工筛选，不能直接用随机结果。
3. **存放路径**：`Assets/_Project/Art/AI_Generated/`，按用途分子目录：
   - `Characters/`：人物剪影/立绘
   - `Narrative/`：叙事插画
   - `UI/`：UI 装饰元素
4. **元数据记录**：每个 AI 文件旁放置同名 `.md` 或 `.txt`，记录：
   - 使用的模型/工具
   - 关键 prompt
   - 修改历史
   - 商用授权说明
5. **版权确认**：只使用允许商业用途的模型和平台。

### 5.3 AI 定位
AI 是「风格探索和 2D 素材补充工具」，不是核心美术生产方式。最终审美 judgment 必须由人做出。

---

## 6. 开发约束

### 6.1 性能约束
- WebGL 目标帧率：≥ 30fps（中低端手机）。
- 首包资源大小：Vertical Slice < 10MB，v1.0 < 20MB。
- 单关粒子数量：≤ 200 个活跃粒子。
- 单场景 Draw Call：≤ 100（Vertical Slice），≤ 150（v1.0）。
- 纹理尺寸：最大 1024px，优先使用 2 的幂次方。

### 6.2 代码约束
- 禁止使用 `FindObjectOfType` 和 `Find` 系列方法查找服务。
- 禁止使用单例之间的直接互相调用，统一用事件。
- 禁止在 `Update` 中做重量级计算（如水流模拟），使用 `FixedUpdate` 或独立时间步。
- 禁止使用非确定性随机数影响核心玩法（如 `Random.Range` 用于水流模拟）。
- 所有 `MonoBehaviour` 必须明确生命周期：谁负责 `Awake`/`Start`/`OnDestroy`。

### 6.3 UI 约束
- Vertical Slice 所有 UI 在 Unity 内实现（uGUI/UI Toolkit）。
- UI 必须同时支持鼠标和触摸操作。
- 所有按钮最小点击热区 44 × 44 px。
- 文字必须有足够对比度，符合无障碍要求。

### 6.4 平台约束
- 关卡不与真实 GPS 坐标绑定。
- 存档先本地后云端，保证离线可玩。
- 避免使用 WebGL 不支持的后处理效果（如复杂屏幕空间反射）。
- v1.0 资源按关卡拆分为 AssetBundle，按需从 CDN 加载。

---

## 7. 禁止破坏的逻辑

以下逻辑是项目核心，任何修改必须经过评审：

| 编号 | 禁止项 | 说明 |
|---|---|---|
| F1 | 改变水流模拟的确定性 | 同一布局必须在同一设备和不同设备上结果一致 |
| F2 | 在模拟阶段允许编辑 | 模拟阶段必须锁定所有编辑操作 |
| F3 | 引入付费跳过或购买资源 | 核心玩法要求玩家理解机制后才能通关 |
| F4 | 修改核心状态机转换 | Editing→Simulating→Settling→Editing 顺序不可变 |
| F5 | 破坏构件血量顺序 | 必须保持 竹笼 < 杩槎 < 石墙 |
| F6 | 在切片阶段引入网络依赖 | 存档、资源加载必须离线可用 |
| F7 | 提前开放第三层提示 | L1 最多开放前两层提示 |
| F8 | 改变「暂时安全」结算文案 | 成功文案必须体现"水还会再来"的诚实感 |
| F9 | 在切片阶段接入真实广告 | 仅可预留广告位 |
| F10 | 将关卡绑定真实地理坐标 | 地点仅作为背景设定 |

---

## 8. 验收标准

### 8.1 Vertical Slice 退出标准
1. 玩家能在 5–10 分钟内完成 L1 并理解「堵不如疏」。
2. WebGL 在主流手机和 PC 浏览器上稳定 30fps 以上。
3. 首屏加载时间 < 15 秒（4G 网络）。
4. 水墨风格可被识别（不一定完美，但看得出是水墨）。
5. 核心状态机（编辑/模拟/结算/暂停）无卡死或异常跳转。
6. 成功/失败结算逻辑正确，按钮显示符合设计规范。
7. 拖拽放置、撤销、重置功能正常。
8. 本地存档可读可写。

### 8.2 代码验收标准
1. 所有脚本通过编译无警告。
2. 核心服务使用单例模式，无 `FindObjectOfType`。
3. 所有事件订阅在 `OnDestroy` 中取消。
4. 水流模拟使用固定时间步长，结果可复现。
5. 无硬编码字符串和魔法数字，配置通过 ScriptableObject 读取。

### 8.3 美术验收标准
1. 色彩系统符合 DESIGN.md。
2. 字体使用免费开源字体。
3. AI 生成资产附带元数据文件。
4. 3D 模型面数不超过约定上限。
5. 纹理尺寸符合 2 的幂次方规范。

---

## 9. 附录：架构决策记录

| 决策 | 内容 | 日期 |
|------|------|------|
| 引擎 | Unity 6.3 LTS + URP | 2026-06-25 |
| 目录结构 | `_Project` 根目录，分层清晰 | 2026-06-25 |
| 配置方式 | ScriptableObject 为主 | 2026-06-25 |
| 服务层 | 单例模式 + 事件驱动 | 2026-06-25 |
| AI 机制 | 仅用于 2D 概念/叙事/剪影 | 2026-06-25 |
| 开发约束 | 性能/代码/UI/平台四项约束 | 2026-06-25 |
| 禁止破坏 | 10 条核心逻辑红线 | 2026-06-25 |

---

> 本规范随项目迭代更新。任何架构修改必须先更新此文档。

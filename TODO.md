# 都江堰水利工程主题解谜游戏 — 开发 TODO

> 版本：V0.1  
> 日期：2026-06-25  
> 依据：`docs/PRD-vibe-coding-draft.md`、`docs/ARCHITECTURE.md`、`docs/DESIGN.md`  
> 目标：完成 Vertical Slice（L1《堵》），并严格遵循上述三份规范。

---

## 全局约束（所有任务必须遵守）

- [ ] **GC-01** 不得修改核心状态机顺序：Editing → Simulating → Settling → Editing。
- [ ] **GC-02** 模拟阶段必须锁定所有编辑操作。
- [ ] **GC-03** 水流模拟必须确定性与可复现（固定时间步长，不使用 `Random.Range`）。
- [ ] **GC-04** 构件耐久顺序必须保持：竹笼 < 杩槎 < 石墙。
- [ ] **GC-05** 切片阶段不引入网络依赖（存档、资源离线可用）。
- [ ] **GC-06** L1 提示最多开放前两层，禁止提前开放第三层。
- [ ] **GC-07** 成功结算文案必须保留「暂时安全」的诚实感，不得替换。
- [ ] **GC-08** 切片阶段不接入真实广告，仅预留 Banner 位。
- [ ] **GC-09** 不得引入付费跳过或购买资源。
- [ ] **GC-10** 关卡不得绑定真实地理坐标。
- [ ] **GC-11** 所有代码与资源放在 `Assets/_Project/` 下，不与 Asset Store 包混排。
- [ ] **GC-12** 禁止使用 `FindObjectOfType` / `Find` 系列查找服务。
- [ ] **GC-13** 禁止单例之间直接互相调用，统一使用事件。
- [ ] **GC-14** 禁止在 `Update` 中做重量级计算（水流模拟用 `FixedUpdate` 或独立时间步）。
- [ ] **GC-15** UI 同时支持鼠标和触摸，按钮最小热区 44 × 44 px。
- [ ] **GC-16** 字体优先 Noto Serif SC / Source Han Serif SC；字号、色彩严格遵循 `DESIGN.md`。
- [ ] **GC-17** WebGL 目标：中低端手机 ≥ 30fps；首包 < 10MB；单关活跃粒子 ≤ 200；Draw Call ≤ 100。

---

## 里程碑 M1：灰盒原型

> 目标：L1 白模 + 粒子规则 + 基本交互，能放水、能失败、能重试。

### M1.1 项目初始化

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M1.1.1 | 创建 Unity 6.3 LTS 项目，配置 URP | P0 | 已完成 | 项目能正常打开，`ProjectSettings/Graphics.asset` 已指定 URP Asset | PRD §6.1、ARCH §1.1 |
| M1.1.2 | 搭建 `Assets/_Project/` 完整目录结构（Scripts、Scenes、Prefabs、Art、Audio、ScriptableObjects、Resources、StreamingAssets） | P0 | 已完成 | 目录与 `ARCHITECTURE.md` §2 完全一致，空目录放置 `.gitkeep` | ARCH §2 |
| M1.1.3 | 配置 Git + .gitignore，确定版本控制策略 | P1 | 已完成 | `.gitignore` 包含 Library、Temp、Obj、Builds；大文件走 LFS 或 CDN | ARCH §1.2 |
| M1.1.4 | 导入基础 Asset Store 包（低多边形山水/村庄/建筑包）并确认商用授权 | P1 | 已完成 | 授权记录保存于 `docs/licenses.md`；资源仅用于 `_Project` 外部引用或已整理到 `_Project/Art/Models/` | PRD §6.7、ARCH §1.3 |
| M1.1.5 | 导入 Noto Serif SC / Source Han Serif SC 字体并配置 SDF | P1 | 已完成 | 字体文件位于 `Assets/_Project/Art/UI/Fonts/`，SDF 资产 `NotoSerifSC-Regular_SDF.asset` 已生成并设为 TMP 默认字体；含 FontTest 校验场景 | DESIGN §3、ARCH §1.3 |

### M1.2 数据原型与配置

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M1.2.1 | 实现 `LevelConfigSO` ScriptableObject | P0 | 已完成 | 包含 PRD/ARCH 中全部字段，位于 `Assets/_Project/Scripts/Data/LevelConfigSO.cs` | ARCH §3.1 |
| M1.2.2 | 实现 `BlockConfigSO` ScriptableObject | P0 | 已完成 | 包含全部字段，位于 `Assets/_Project/Scripts/Data/BlockConfigSO.cs`；已创建竹笼/杩槎/石墙三个资产 | ARCH §3.2 |
| M1.2.3 | 实现运行时数据结构：`PuzzleRuntime`、`BlockInstance`、`PuzzleResult`、`PlayerProfile`、`GameSettings` | P0 | 已完成 | 字段与 `ARCHITECTURE.md` §3.3–3.6 一致；全部可序列化，位于 `Assets/_Project/Scripts/Data/RuntimeData.cs` | ARCH §3.3–3.6 |
| M1.2.4 | 配置 L1 关卡数据资产 | P0 | 已完成 | 已创建 `Assets/_Project/ScriptableObjects/Levels/LevelConfig_L1.asset`，包含地形、水源、村庄、库存、资源上限、节俭阈值、提示树、叙事、碑廊解锁 | PRD §3.2、§5.1 |
| M1.2.5 | 配置竹笼、杩槎、石墙构件资产 | P0 | 已完成 | 已创建三个 `.asset`；耐久关系：竹笼 30 < 杩槎 60 < 石墙 200 | PRD §6.3、ARCH §3.2 |
| M1.2.6 | 配置 L1 老河工提示树（仅两层） | P1 | 已完成 | 第 3 次失败触发第一层（描述现象），第 5 次失败触发第二层（指向工具）；未配置第三层 | PRD §5.2、ARCH §F7 |

### M1.3 核心服务层（单例 + 事件）

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M1.3.1 | 实现 `InputSystem`：统一封装鼠标/触摸输入 | P0 | 已完成 | 提供 `OnPointerDown/Move/Up` 事件；`IsTouchDevice` 属性；屏幕坐标输出 | ARCH §4.2 |
| M1.3.2 | 实现 `PuzzleSystem` 核心状态机 | P0 | 已完成 | 状态：`Editing / Simulating / Settling / Paused`；事件完整；方法包含 `InitLevel`、`TryPlaceBlock`、`TryMoveBlock`、`RotateBlock`、`RemoveBlock`、`StartSimulation`、`Pause/Resume`、`Undo`、`Reset` | PRD §5.3、ARCH §4.2 |
| M1.3.3 | 实现 `BlockPlacement`：网格吸附、旋转、撤销、重置 | P0 | 已完成 | 订阅 InputSystem 实现拖拽 ghost、网格吸附、90° 旋转；与 PuzzleSystem 事件同步视觉；非法位置变红 | PRD §5.1、ARCH §4.2 |
| M1.3.4 | 实现 `WaterSimulation`：规则粒子水流 | P0 | 已完成 | 固定时间步长；石墙反弹+耐久伤害、竹笼/杩槎法线分流；村庄矩形区域受击统计 | PRD §6.3、ARCH §6.2 |
| M1.3.5 | 实现 `LevelResult`：成功/失败/节俭判定 | P0 | 已完成 | 失败原因：`Flood / Destroyed / Timeout`；节俭判定：`consumedResource <= frugalThreshold`；自动解锁对应碑廊 | PRD §5.4、ARCH §3.5 |
| M1.3.6 | 实现 `SaveSystem`（PlayerPrefs / localStorage） | P1 | 已完成 | 可读写 `PlayerProfile` 与 `PuzzleRuntime`；使用 JsonUtility + PlayerPrefs | PRD §3.2、ARCH §4.2 |
| M1.3.7 | 实现 `AudioSystem`：音乐/音效分轨 | P1 | 已完成 | 双 AudioSource 分轨；`PlayMusic`/`PlaySFX`/`SetMusicVolume`/`SetSFXVolume`；音量自动保存到 `PlayerProfile` | ARCH §4.2 |
| M1.3.8 | 实现 `UIManager`：页面/弹窗栈管理 | P1 | 已完成 | `ShowScreen`/`HideScreen`/`ShowModal`/`ShowResult`；注册表 + 栈管理弹窗层级 | ARCH §4.2 |
| M1.3.9 | 所有 MonoBehaviour 明确生命周期并在 `OnDestroy` 中取消事件订阅 | P1 | 已完成 | 订阅事件的服务在 `OnDisable`/`OnDestroy` 中取消订阅；单例在销毁时清空引用 | ARCH §4.3、§6.2 |

### M1.4 灰盒场景与基础交互

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M1.4.1 | 搭建 L1 灰盒场景 `Level_L1.unity` | P0 | 已完成 | 已创建 `Assets/_Project/Scenes/Level_L1.unity`：地面、主摄像机、方向光、水源标记、村庄触发区、Services、Canvas UI、LevelBootstrap | PRD §5.1、ARCH §2 |
| M1.4.2 | 实现水源发射器与粒子池 | P1 | 已完成 | `WaterSimulation` 维护粒子列表与视觉对象池，`maxParticles=200`，参数由 `WaterSourceConfig` 配置 | PRD §6.3 |
| M1.4.3 | 实现村庄受击检测与统计 | P1 | 已完成 | 村庄区域配置 `VillageConfig`，场景中有触发 Collider；`WaterSimulation` 按矩形区域统计受击并触发 `Flood` | PRD §5.4 |
| M1.4.4 | 实现「放水」按钮进入模拟阶段 | P1 | 已完成 | Canvas 提供「放水」按钮调用 `PuzzleSystem.StartSimulation()`；工具栏选择/旋转/重置按钮已就位 | PRD §5.1、DESIGN §6.4 |
| M1.4.5 | 实现模拟结束自动进入结算 | P1 | 已完成 | `PuzzleSystem.Update` 按固定步长驱动模拟，满足成功/失败条件后自动 `Settle` 并触发 `OnLevelSettled` | PRD §5.3 |
| M1.4.6 | 实现重试/重置保留提示触发计数 | P2 | 已完成 | `PlayerProfile` 新增 `LevelFailureCount` 列表；`PuzzleSystem.Settle` 在失败时累加并保存，`Reset` 不清除 | PRD §5.1 |

---

## 里程碑 M2：核心循环验证

> 目标：L1 完整流程 + 结算 + 提示系统，玩家能在无提示下多次试错后成功。

### M2.1 编辑阶段完整交互

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M2.1.1 | 底部 Toolbar 实现竹笼、杩槎放置 | P0 | 已完成 | ToolbarItem 60×60px，选中态、禁用态、计数显示符合 DESIGN §5.5 | DESIGN §5.5 |
| M2.1.2 | 实现拖拽放置的完整反馈（ghost、吸附、非法回退） | P0 | 已完成 | 按下即出 ghost；释放合法则 150ms 吸附动画；非法变红 200ms 后回 toolbar；释放到 canvas 外不消耗库存 | DESIGN §6.3 |
| M2.1.3 | 实现撤销（Undo）与重置（Reset） | P1 | 已完成 | Undo 回退一步编辑操作；Reset 恢复关卡初始布局 | PRD §3.2 |
| M2.1.4 | 实现实时资源消耗显示（料/工/时） | P1 | 已完成 | HUD 顶部显示三种资源，ResourceDot 颜色符合 DESIGN §5.6 | PRD §3.2、DESIGN §5.6 |
| M2.1.5 | 空操作提示（未放构件点击放水） | P2 | 已完成 | 顶部 HintPill 显示「先在河里摆上构件」，2.5s 淡出 | DESIGN §7.2 |

### M2.2 模拟与结算

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M2.2.1 | 完善 WaterInteraction 效果 | P0 | 已完成 | 石墙反弹+耐久伤害；竹笼/杩槎按法线分流；构件被冲毁 300ms 淡出 | PRD §6.3、DESIGN §6.2 |
| M2.2.2 | 实现成功/失败/节俭结算逻辑 | P0 | 已完成 | 成功条件：模拟时长 ≥ targetDuration 且村庄未淹没；失败条件：Flood / Destroyed / Timeout；节俭阈值判定正确 | PRD §5.4 |
| M2.2.3 | 实现结算弹窗第一屏（成功/失败） | P0 | 已完成 | 成功：印章「安」绿色，标题「暂时安全」，节俭/非节俭文案，按钮「查看碑廊」；失败：印章「倒」红色，标题「村子仍被淹」，按钮「再试一次」 | DESIGN §7.5、§7.6 |
| M2.2.4 | 实现结算弹窗第二屏（碑廊解锁） | P1 | 已完成 | 标题「堵不如疏」，图文内容，按钮「再试一次」+「下一关（占位）」 | PRD §5.1、DESIGN §7.5 |
| M2.2.5 | 结算页切换动画（屏1 → 屏2） | P2 | 已完成 | 300ms ease-in-out 淡入淡出 | DESIGN §6.2 |

### M2.3 提示系统

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M2.3.1 | 实现老河工提示对话框 | P1 | 已完成 | 底部居中，头像 + 文字，打字机效果 40ms/字，关闭按钮或 5s 自动关闭 | DESIGN §7.3 |
| M2.3.2 | 实现提示触发逻辑 | P1 | 已完成 | 第 3 次失败触发第一层，第 5 次失败触发第二层；不开放第三层 | PRD §5.2、ARCH §F7 |
| M2.3.3 | 提示出现动画 | P2 | 已完成 | 400ms ease-out 从底部滑入 + 淡入 | DESIGN §6.2 |

### M2.4 暂停与设置

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M2.4.1 | 实现暂停菜单 | P1 | 已完成 | 全屏 `--paper` 95% 遮罩；按钮：继续、重试、设置、返回标题 | PRD §4、DESIGN §7.4 |
| M2.4.2 | 实现设置页（音量、画质、语言预留） | P1 | 已完成 | 音乐、音效滑块；画质下拉（低/中/高）；语言预留字段；设置保存到 PlayerProfile | PRD §3.2、DESIGN §7 |
| M2.4.3 | 暂停菜单出现动画 | P2 | 已完成 | 200ms 遮罩淡入 + 按钮依次上移 50ms stagger | DESIGN §6.2 |

---

## 里程碑 M3：水墨视觉验证

> 目标：Shader + 后处理 + 占位资产，风格被识别为水墨，帧率达标。

### M3.1 3D 场景美术

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M3.1.1 | 替换灰盒为低多边形 3D 场景 | P0 | 已完成 | 地形、山石、村庄、河道使用低多边形模型；单模型面数 ≤ 500 | DESIGN §9.1 |
| M3.1.2 | 场景使用淡墨+青绿色调材质，饱和度 ≤ 40% | P0 | 已完成 | 无写实高反光材质；整体色调符合 `--paper`、`--ink-*`、`--accent` | DESIGN §2、§9.1 |
| M3.1.3 | 水面片与河道视觉效果 | P1 | 已完成 | 水面使用 `--water` 色系，配合简单波动或 Shader | DESIGN §2 |

### M3.2 水墨 Shader 与后处理

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M3.2.1 | 使用 URP Shader Graph 实现水墨材质（宣纸纹理、边缘晕染、墨色渐变） | P0 | 部分完成 | 应用到场景基底、构件上；肉眼可识别水墨感 | PRD §6.4、ARCH §1.1 |
| M3.2.2 | 实现全局后处理：水墨勾边、纸质噪点、远景雾化 | P1 | 部分完成 | 后处理不造成帧率低于 30fps；避免 WebGL 不支持的效果（复杂屏幕空间反射） | PRD §6.4、ARCH §6.4 |
| M3.2.3 | 实现水粒子 Trail Renderer + 水墨 Shader | P1 | 已完成 | 速度越快墨色越浓、拖尾越长；粒子数 ≤ 200 | PRD §6.3、DESIGN §9.2 |
| M3.2.4 | 性能优化：必要时动态降质（粒子数、后处理强度） | P1 | 已完成 | 中低端手机稳定 30fps 以上 | PRD §7.1、ARCH §6.1 |

### M3.3 2D 叙事资产

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M3.3.1 | 生成/收集 L1 引子叙事图（AI 水墨风格） | P2 | 已完成 | 统一关键词：水墨、淡彩、宣纸、宋代山水、留白；存放于 `Assets/_Project/Art/AI_Generated/Narrative/` | PRD §6.7、ARCH §5、DESIGN §9.3 |
| M3.3.2 | 生成老河工头像/人物剪影（AI 水墨风格） | P2 | 已完成 | 存放于 `Assets/_Project/Art/AI_Generated/Characters/` | ARCH §5.2 |
| M3.3.3 | 为每个 AI 生成文件创建元数据 `.md` | P2 | 已完成 | 记录工具、prompt、修改历史、商用授权说明 | ARCH §5.2 |
| M3.3.4 | UI 装饰元素（印章、水墨纹理、进度条素材） | P2 | 已完成 | 优先程序生成宣纸噪点；贴图尺寸为 2 的幂次方，最大 1024px | DESIGN §9.4 |

---

## 里程碑 M4：切片完成

> 目标：合并启动/标题页、引子、结算/碑廊弹窗、设置，符合页面清单和退出标准。

### M4.1 页面清单实现

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M4.1.1 | 启动/标题页 | P0 | 已完成 | Logo、水墨背景、「开始治水」按钮、设置入口；两页合并为一页 | PRD §4、DESIGN §7 |
| M4.1.2 | L1 引子页 | P0 | 已完成 | 官方筑墙失败、村庄危急叙事；可跳过；400–600ms 水墨过渡 | PRD §4、§5.1、DESIGN §6.2 |
| M4.1.3 | 游戏主界面 | P0 | 已完成 | 3D 河道场景 + HUD（关卡名、阶段、资源条、底部 Toolbar） | PRD §4 |
| M4.1.4 | 提示弹窗 | P1 | 未开始 | 老河工分层提示（L1 最多两层） | PRD §4 |
| M4.1.5 | 暂停菜单 | P1 | 未开始 | 继续、重试、设置、返回标题 | PRD §4 |
| M4.1.6 | 结算/碑廊弹窗 | P0 | 未开始 | 两屏弹窗：屏1 结果反馈，屏2 碑廊「堵不如疏」图文 | PRD §4 |
| M4.1.7 | 设置页 | P1 | 未开始 | 从暂停菜单进入；音乐、音效、画质、语言 | PRD §4 |
| M4.1.8 | 加载页 | P1 | 已完成 | 水墨进度条 + 文案「研墨中…」；不显示百分比但进度真实 | DESIGN §7.1 |

### M4.2 UI 组件与动效

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M4.2.1 | 实现 `BtnPrimary`、`BtnSecondary`、`BtnGhost` 三种按钮 | P1 | 已完成 | 颜色、圆角、Hover/Press 效果符合 DESIGN §5.1–5.3 | DESIGN §5 |
| M4.2.2 | 实现 `Card`、`ToolbarItem`、`ResourceDot`、`Seal`、`DialogOverlay`、`HintPill` 组件 | P1 | 未开始 | 尺寸、颜色、动画符合 DESIGN §5 | DESIGN §5 |
| M4.2.3 | 页面切换动画 | P2 | 已完成 | 400ms ease-in-out 淡入 + 向上位移 12px | DESIGN §6.2 |
| M4.2.4 | 按钮点击反馈 | P2 | 已完成 | 100ms ease-out 缩放 0.97 或背景色填充 | DESIGN §6.2 |
| M4.2.5 | 响应式适配与安全区域 | P1 | 已完成 | 基准 375×812；宽度 > 430 居中留白；高度 < 700 压缩 HUD/Toolbar；顶部 44px / 底部 34px 安全区 | DESIGN §8 |

### M4.3 存档与设置持久化

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M4.3.1 | 本地存档可读可写 | P0 | 未开始 | 保存/读取 PlayerProfile、关卡进度、设置；WebGL 使用 localStorage | PRD §3.2、ARCH §4.2 |
| M4.3.2 | 存档异常处理 | P1 | 已完成 | 存档失败时显示错误弹窗，不崩溃 | DESIGN §7.7 |

---

## 里程碑 M5：上线测试

> 目标：部署到网页，3–5 人完整游玩并反馈。

### M5.1 WebGL 构建与部署

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M5.1.1 | 配置 WebGL 构建设置（压缩 LZMA/Brotli） | P0 | 未开始 | 首包资源 < 10MB；加载时间 < 15s（4G） | PRD §3.4、§7.2、ARCH §6.1 |
| M5.1.2 | 制作外部网页加载壳（进度条、全屏按钮、WebGL 不支持提示） | P0 | 未开始 | 网页仅做加载壳，游戏逻辑在 Unity 内 | PRD §6.2、ARCH §6.4 |
| M5.1.3 | 部署到测试服务器/静态托管 | P1 | 未开始 | 可通过 URL 访问并完整游玩 L1 | PRD §8 |
| M5.1.4 | 主流浏览器兼容性测试（Chrome/Safari/Firefox/Edge，PC + 手机） | P1 | 未开始 | 无崩溃、无严重渲染错误 | PRD §3.4 |
| M5.1.5 | 性能测试：中低端手机 30fps | P1 | 未开始 | 使用真机或远程设备测试；必要时降质 | PRD §3.4、ARCH §6.1 |

### M5.2 用户测试与反馈

| 编号 | 任务 | 优先级 | 状态 | 验收标准 | 关联规范 |
|---|---|---|---|---|---|
| M5.2.1 | 组织 3–5 人试玩 L1 | P1 | 未开始 | 记录玩家是否能 5–10 分钟内完成并理解「堵不如疏」 | PRD §3.4、§7.4 |
| M5.2.2 | 收集并整理反馈，识别核心循环问题 | P1 | 未开始 | 输出 `docs/feedback-m5.md` | PRD §8 |
| M5.2.3 | 根据反馈迭代 L1（关卡难度、提示时机、视觉表现） | P2 | 未开始 | 变更记录到 `docs/CHANGELOG.md` | PRD §7.5 |

---

## 验收检查清单（退出标准）

- [ ] 玩家能在 5–10 分钟内完成 L1 并理解「堵不如疏」。
- [ ] WebGL 在主流手机和 PC 浏览器上稳定 30fps 以上。
- [ ] 首屏加载时间 < 15 秒（4G 网络）。
- [ ] 水墨风格可被识别。
- [ ] 核心状态机（编辑/模拟/结算/暂停）无卡死或异常跳转。
- [ ] 成功/失败结算逻辑正确，按钮显示符合设计规范。
- [ ] 拖拽放置、撤销、重置功能正常。
- [ ] 本地存档可读可写。
- [ ] 所有脚本编译无警告。
- [ ] 无 `FindObjectOfType` 调用。
- [ ] 所有事件订阅在 `OnDestroy` 中取消。
- [ ] 色彩、字体、组件符合 `DESIGN.md`。
- [ ] AI 生成资产附带元数据文件。
- [ ] 10 条禁止破坏逻辑（F1–F10）全部满足。

---

## 附录：参考文档

- [PRD 草稿](docs/PRD-vibe-coding-draft.md)
- [架构规范](docs/ARCHITECTURE.md)
- [UI/UX 设计规范](docs/DESIGN.md)

> 本 TODO 随项目迭代更新。任何范围变更需先更新 `docs/PRD-vibe-coding-draft.md` 与本文件。

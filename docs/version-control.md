# 版本控制策略

> 适用于 `game-proj` 的 Unity 项目。

## 基本原则

- **源代码与文本配置**：直接提交到 Git，享受 diff、merge 与代码审查。
- **二进制美术/音效/模型资源**：使用 Git LFS 跟踪，避免仓库膨胀。
- **超大或运行时下载资源**：走 CDN，不在仓库中跟踪。

## Git 忽略规则

见仓库根目录 `.gitignore`：

- Unity 生成目录：`Library/`、`Temp/`、`Obj/`、`Build/`、`Builds/`、`Logs/`、`UserSettings/`。
- IDE/OS 临时文件：`.vscode/`、`.idea/`、`.DS_Store`、`Thumbs.db`。
- 构建产物：`.apk`、`.aab`、`.exe`、`.unitypackage` 等。

## Git LFS 规则

见仓库根目录 `.gitattributes`：

- 图片：`.psd`、`.tga`、`.png`、`.jpg`、`.exr` 等。
- 音频：`.mp3`、`.wav`、`.ogg` 等。
- 视频：`.mp4`、`.mov` 等。
- 模型/动画：`.fbx`、`.obj`、`.blend`、`.gltf`、`.glb` 等。
- 字体：`.ttf`、`.otf`。
- 文档/压缩包：`.pdf`、`.zip`、`.7z`、`.unitypackage`。

## 工作流

- 功能分支：`feature/<name>`。
- 每个任务完成后在功能分支提交，合并到 `main` 并 push 到 `origin`。
- 提交前确保 Unity 可在 batchmode 下正常打开（退出码 0）。

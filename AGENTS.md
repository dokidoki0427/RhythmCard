# AGENTS.md — 卡牌音游（Unity + Game Framework）

本文件是给 AI agent（DSH / Claude Code / Codex 等）的项目工作协议。人类同事也请遵守同样的门禁。

## 环境事实

- Unity **6000.0.61f1**（`E:\Unity\Editor\Unity.exe`），Unity Personal 许可。
- 工程根目录 = 本仓库根目录；框架是内嵌包 `Packages/com.jiangyin.gameframework`（Game Framework 2021.5.31，已打 Unity 6 兼容补丁）。
- 没有可用的 in-Editor MCP；**批处理（batchmode）是唯一的自动化通道**。

## 一键命令（Windows / PowerShell）

| 目的 | 命令 | 热启动耗时 |
|---|---|---|
| 编译 + 工程校验 | `tools\verify.ps1` | ~7s |
| EditMode 测试 | `tools\test.ps1` | ~6s |
| Windows 出包 | `tools\build.ps1` | ~10–45s |
| 全流程（等价 CI） | `tools\ci.ps1` | ~20–60s |
| 首次初始化 Boot 场景 | `tools\init.ps1` | ~8s |
| 双击运行（不敲命令） | `一键验证.bat` / `一键CI.bat` | 同上 |

`一键*.bat` 里已经带了 `-ExecutionPolicy Bypass`，双击即可，不受系统脚本策略影响。手动敲 `.\tools\*.ps1` 若报“未进行数字签名”，改用 `powershell -ExecutionPolicy Bypass -File .\tools\ci.ps1`。

**批处理需要独占工程**：Unity 编辑器开着时脚本会直接退出（exit 4）。编辑器开着想快速自检，用菜单 `Tools/卡牌音游/验证工程（不关编辑器）`。

## 硬性规则

1. 改完任何 `.cs` / `.asmdef` / `Packages` 之前先想清楚，改完必须跑 `tools\verify.ps1`，红了就修到绿。
2. 不要手改 `Library/`、`Temp/`、`Builds/`、`Logs/`——都是生成物。
3. 玩法代码放 `Assets/GameMain/`；Editor 工具放 `Assets/Editor/`；测试放 `Assets/GameMain/Tests/`。
4. `Packages/com.jiangyin.gameframework/` 是“框架代码”：非必要不改；必须改时在 commit message 里写明原因，并同步更新 `开发环境说明.md`。
5. 自动化日志/报告**只用 ASCII**（`ProjectAutomation.cs` 里的字符串），中文注释可以——不同终端的编码会把中文打成乱码，干扰判断。
6. 提交前 `tools\ci.ps1` 必须全绿；只跑 `verify` 不算完成。

## 完成定义（Definition of Done）

- [ ] `tools\verify.ps1` → exit 0，`Logs/automation-report.json` 里 `result=PASS`
- [ ] `tools\test.ps1` → 全部通过
- [ ] 涉及可运行内容时 `tools\build.ps1` → 产出 `Builds/Windows/CardRhythm.exe`
- [ ] 新增/变更的约定已写进 `自动化开发流程.md` 或相应文档

## 失败时看哪里

| 文件 | 内容 |
|---|---|
| `Logs/verify.log` | 编译错误（搜 `error CS`）、包解析、执行异常 |
| `Logs/automation-report.json` | 结构化校验结果（机器可读） |
| `Logs/test-EditMode.xml` | NUnit 结果（`test-case[@result='Failed']`） |
| `Logs/build.log` | 构建摘要、`BuildReport` 错误 |

## Game Framework 约定

- 场景入口是 `Assets/Scenes/Boot.unity`，里面放 `GameFramework` 预制体实例（22 个组件）。
- 游戏流程继承 `ProcedureBase`，由 `ProcedureComponent` 驱动；不要用 `Update` 里手写状态机。
- 资源一律走 GF 异步接口（`LoadAsset` / `LoadUIForm` / `LoadEntity`），禁止 `Resources.Load` 和直接 `Instantiate` 业务预制体。
- UI 用 uGUI + GF `UIComponent`：界面继承 `UIFormBase`，通过 `UIFormCatalog` 注册、`UIRouter.Open/Close/Back` 打开关闭；层级见 `界面层级框架.md`。
- 新增界面必须同步：`UIFormId` 枚举、`UIFormCatalog` 定义、`Forms/XxxForm.cs`，然后跑 `Tools → 卡牌音游 → UI → 重建占位界面`。

## 内容制作

- 卡牌用 `Tools → 卡牌音游 → 卡牌编辑器` 制作，资产在 `Assets/GameMain/Data/Cards/`，说明见 `卡牌编辑器.md`。
- 卡牌 `Id` 是稳定契约（存档 / 谱面 `sourceCardId` 都引用它），不要随意改。
- 新增卡牌字段：改 `CardDefinition` + 在 `CardDesignerWindow.DrawFields` 里加一行 `PropertyField` + 在 `CardValidator` 里加规则。
- `tools\verify.ps1` 的 `cards` 检查会扫描全部卡牌，重复 Id 或非法字段会让 CI 变红。
- **数值不进卡牌**：伤害/治疗等归 `CardEffect`（`Assets/GameMain/Data/Effects/`），卡牌只引用效果。新增功能时派生 `CardEffect`，不要在 `CardDefinition` 上加数值字段。

# 卡牌音游（RhythmCard）

把牌组变成谱面的肉鸽卡牌 + 音游原型：你打出的牌决定这一小节出什么音符，牌组顺序就是乐章结构。

> 当前阶段：**框架骨架**（流程 / UI 层级 / 卡牌编辑器 / 自动化），判定内核尚未接入。

## 技术栈

| 项 | 版本 |
|---|---|
| Unity | 6000.0.61f1（Built-in 渲染管线） |
| Game Framework | 2021.5.31（内嵌包，已打 Unity 6 兼容补丁） |
| UI | uGUI + GF `UIComponent` |
| 测试 | Unity Test Framework（EditMode） |

## 快速开始

```powershell
# 打开工程（需要 Unity 6000.0.61f1）
unity open "E:\UnityProgram\卡牌音游"

# 命令行门禁（需先关闭 Unity 编辑器）
.\tools\verify.ps1     # 编译 + 工程校验
.\tools\test.ps1       # EditMode 测试
.\tools\ci.ps1         # 校验 + 测试 + 出 Windows 包
```

也可以直接双击 `一键验证.bat` / `一键CI.bat`。

## 目录

```
Assets/
  GameMain/
    Scripts/
      Procedure/   流程（Launch → Menu → Map → Battle → Result）
      UI/          界面层级、路由、基类
      Cards/       卡牌数据、效果、校验、键位分布
    UIForms/       占位界面预制体
    Data/          卡牌与效果资产
    Tests/         EditMode 测试
  Editor/
    Automation/    批处理校验/构建入口
    UI/            界面占位生成与场景配置
    Cards/         卡牌编辑器
    Flow/          流程配置
Packages/
  com.jiangyin.gameframework/   内嵌的 Game Framework
tools/            命令行门禁脚本
ci/               GitHub Actions 模板
```

## 文档

| 文档 | 内容 |
|---|---|
| [开发环境说明.md](开发环境说明.md) | Unity / CLI / GF 的安装与补丁记录 |
| [自动化开发流程.md](自动化开发流程.md) | 门禁脚本、退出码、CI |
| [界面层级框架.md](界面层级框架.md) | 六层 UI、路由、新增界面步骤 |
| [卡牌编辑器.md](卡牌编辑器.md) | 卡牌制作、效果资产、校验规则 |
| [流程骨架.md](流程骨架.md) | 流程清单与如何扩展 |
| [开工前决策清单.md](开工前决策清单.md) | 已确认的玩法/技术决策 |
| [AGENTS.md](AGENTS.md) | 给 AI agent 的工作协议 |

## 状态

- [x] 工程与框架、自动化门禁
- [x] UI 层级框架 + 占位界面
- [x] 卡牌编辑器 + 效果资产
- [x] 流程骨架（键盘 Space 推进）
- [ ] 判定内核（dspTime 时钟 / 判定窗 / 音符视图）
- [ ] 谱面生成（牌组 → 音符）
- [ ] 战斗闭环（敌人 / 预算 / 强拍指定 / 结算）

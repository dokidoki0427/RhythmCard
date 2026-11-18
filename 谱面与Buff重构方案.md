# 谱面与 Buff 重构方案（预置谱面 · 取消卡牌）

> 状态：**方案评估稿**（未开工）
> 目标：每个敌人使用一张手工制作的固定谱面；原卡牌效果改为绑定在谱面音符上的 Buff，命中才生效；不再有卡组、抽牌、卡牌编辑器。

---

## 1. 结论：可行

核心节奏内核（`Conductor` / `Judger` / `Chart` / `NoteStageView` / `LaneAudioPlayer`）与卡牌**没有强耦合**，卡牌只影响三处：

1. 谱面从哪来（`ChartGenerator.Generate(deck, ...)`）
2. 音符带什么数据（`NoteData.SourceCardId`）
3. 判定后结算什么（`BattleRunner.OnJudged` → `BattleState.ApplyJudgement(judgement, damage, accent)`）

把这三处换成「谱面资产 → 音符携带 BuffId → 命中触发 Buff」，其余可以基本不动。

**工作量集中在编辑器谱面工具**，不在战斗逻辑。

---

## 2. 现状：卡牌渗透点

| 文件 | 现在的职责 | 处理 |
|---|---|---|
| `Rhythm/ChartGenerator.cs` | 用卡组确定性地生成谱面 | 降级为「编辑器里的草稿生成器」，运行时不再调用 |
| `Rhythm/NoteData.cs` | `SourceCardId` 标记音符来自哪张卡 | 改为 `BuffId`（可空） |
| `Rhythm/BattleRunner.cs` | `LoadCards` / `ResolveDeckCards` / `BuildDeck` / `BuildColors` / 显示卡面 / `DamageOf` | 删掉卡组相关，改为加载 `ChartDefinition`；判定后触发 Buff |
| `Battle/BattleState.cs` | `ApplyJudgement(judgement, damage, accent)`，护盾只被 accent 破 | 增加 Buff 容器与效果结算；伤害改由 Buff 提供 |
| `Battle/RunState.cs` | 8 层地图 + 当前敌人；牌组存 PlayerPrefs | 保留地图；删牌组；每层映射到「敌人 + 谱面」 |
| `UI/Forms/DeckForm.cs` | 牌组编辑（20 张） | 删除，或改成 Buff 图鉴 |
| `UI/Forms/RewardForm.cs` | 胜利三选一加卡 | 删除，或改成层间 Buff 三选一（待定） |
| `Cards/CardDefinition.cs`、`CardEffect/*` | 卡牌与效果 | 保留作为参考，战斗不再引用；后续可删 |
| `Assets/Editor/Cards/*` | 卡牌编辑器、卡牌校验 | 改为谱面编辑器 / Buff 编辑器 |
| `EditorAutomation` 的 `cards` 检查 | 校验卡牌资产 | 换成 `charts` / `buffs` 校验 |

---

## 3. 新数据模型（建议）

### 3.1 谱面

```csharp
public enum ChartNoteKind { Tap, Accent, Buff }

[Serializable]
public struct ChartNoteData
{
    public int Bar;         // 小节
    public int Slot;        // 1/8 格（0..7）
    public int Lane;        // 0..3
    public ChartNoteKind Kind;
    public string BuffId;   // Kind == Buff 时必填
}

[Serializable]
public struct ChartEventData
{
    public int Bar;         // 敌人攻击 / 预警 / 阶段切换发生在哪小节
    public ChartEventType Type;
    public int Value;
}

public sealed class ChartDefinition : ScriptableObject
{
    public string Id;
    public string EnemyId;
    public float Bpm = 120f;
    public int Bars = 32;
    public List<ChartNoteData> Notes;
    public List<ChartEventData> Events;
}
```

要点：

- 用**整数格坐标（Bar/Slot）**而不是浮点时间，保证永远对齐 1/8 网格，编辑器也更好做。
- 时间在运行时由 `Bpm` 推导，和现在的 `Chart.SlotTime` 一致。
- `Events` 把「敌人第几小节攻击 / 第几小节进二阶段」也放进谱面，避免难度和谱面脱节。

### 3.2 Buff

```csharp
public enum BuffEffectType
{
    Damage,          // 直接造成伤害
    Shield,          // 获得护盾
    Heal,            // 回复生命
    DamageBuff,      // 本局伤害倍率
    EnemyDelay,      // 推迟下一次敌人攻击
    ScoreBonus,      // 加分
}

public sealed class BuffDefinition : ScriptableObject
{
    public string Id;
    public string DisplayName;
    [TextArea] public string Description;
    public BuffEffectType Effect;
    public float Value;
    public float DurationBeats;   // 0 = 瞬时
    public Color Color = Color.white;
    public Sprite Icon;           // 可选，放 Resources/Pic
}
```

运行时 `BattleState` 维护 `List<ActiveBuff>`，按剩余拍数递减；瞬时效果立即结算。

---

## 4. 运行时改造清单

| 文件 | 改动 |
|---|---|
| `NoteData.cs` | `SourceCardId` → `BuffId`；增加 `Kind` |
| `Chart.cs` | 构造函数增加 `Events`；音符已经按时间排序，逻辑不变 |
| `Judger.cs` | **不改**（只消费 `NoteData`） |
| `BattleRunner.cs` | 删除卡组加载与 `BuildDeck`；`Begin()` 读取 `ChartDefinition` 并转成 `Chart`；`OnJudged` 里若音符带 BuffId 且判定成功 → `m_State.ApplyBuff(buffId)` |
| `BattleState.cs` | 新增 `ApplyBuff` / `TickBuffs` / Buff 列表；伤害、护盾、治疗走 Buff；去掉 `DamageOf(note)` 的卡牌依赖（可保留「基础音符伤害 1」） |
| `NoteStageView.cs` | 卡面展示（`ShowPlayedCard`）改为「Buff 触发横幅」；带 Buff 的音符画不同颜色/图标 |
| `RunState.cs` | 删除 `Deck`/`PlayerPrefs` 牌组；新增 `CurrentChartId` |
| `ProcedureBattle.cs` | 传入敌人对应的谱面 Id |
| `BattleResult.cs` | 可增加「触发的 Buff 次数」统计 |

---

## 5. 谱面生成器（编辑器）功能清单

入口建议：`Tools → 卡牌音游 → 谱面编辑器`

**最小可用（P2 第一批）**

- 4 轨 × 小节网格（1/8 格），鼠标点击放置/删除音符
- 选择音符类型：Tap / Accent / Buff
- Buff 下拉框（从 `Resources/Buffs` 读取），绑定到选中音符
- 保存/另存为 `ChartDefinition` 资产到 `Resources/Charts/`
- 校验按钮：越界、同格重复、Buff 引用失效、空谱面、BPM 未设置

**第二批**

- 框选、复制/粘贴、撤销
- 播放预览（只跑 `Judger` + 音效，不结算）
- 从旧 `ChartGenerator` 一键生成草稿，再手改
- 波形/节拍线显示

**第三批**

- 难度分档（同敌人多张谱面）
- 谱面版本号与迁移
- 导出 JSON（给外部工具/策划表格）

---

## 6. 流程与 UI 影响

- **地图**：每层 → 敌人 → 固定谱面，不再依赖牌组。
- **战斗**：进入即加载该敌人谱面。
- **奖励**：如果保留成长，改成「层间 Buff 三选一」；如果不保留，直接删除 `RewardForm`。
- **牌组界面**：删除，或改成「本关 Buff 一览」。
- **结算**：保留，增加 Buff 统计。
- **HUD**：卡面区域改成 Buff 触发提示；顶部可加 Buff 持续时间条。

---

## 7. 需要拍板的关键问题

1. **普通音符是否造成伤害？**
   如果只有 Buff 造成伤害，那大部分音符命中没有反馈，需要「基础伤害」或「连击加成」。
2. **Buff 生效判定**：Perfect 才生效？Perfect + Good？Bad 算不算？
3. **一个音符能带几个 Buff？** 建议先做 1 个。
4. **Buff 持续**：瞬时 / 固定拍数 / 整场 / 可叠加？
5. **敌人攻击**：写进谱面 `Events`（推荐），还是继续按小节固定？
6. **难度**：每个敌人一张谱，还是普通/困难两张？
7. **层间是否保留成长**（Buff 三选一）？不保留的话 8 层就是 8 张固定谱，重玩性靠评分/难度。
8. **空格强拍**：保留为全局机制，还是变成谱面里的 Accent 音符？
9. **旧卡牌资产**：删除，还是留着做参考？
10. **BGM**：固定谱面需要固定音乐；每关一首，还是共用一首？

---

## 8. 分阶段计划（粗估）

| 阶段 | 内容 | 估时 |
|---|---|---|
| P0 | 回答上面 10 个问题，定稿 schema | 0.5 天 |
| P1 | `ChartDefinition` / `BuffDefinition` + `BattleRunner`/`BattleState` 改造，用临时代码谱面跑通 | 1–2 天 |
| P2 | 谱面编辑器最小可用（网格 + Buff 绑定 + 保存 + 校验） | 2–3 天 |
| P3 | 删除牌组/奖励流程，HUD 改 Buff 展示，地图接谱面 | 1 天 |
| P4 | 测试、CI 检查项（charts/buffs）、文档 | 0.5–1 天 |
| P5 | 内容：4 个敌人各 1 张谱 | 0.5 天/张 |

---

## 9. 风险

| 风险 | 说明 | 对策 |
|---|---|---|
| 谱面编辑器是最大工作量 | 网格交互、撤销、预览都费时间 | 先做最小可用，预览后置 |
| 旧卡牌体系牵连广 | UI、存档、测试、编辑器都引用 | 分两步：先让战斗不依赖卡牌，再删 UI/资产 |
| 固定谱面重玩性下降 | 没有随机性 | 评分、难度分档、Buff 组合 |
| 谱面资产版本迁移 | 字段变更会丢数据 | 加 `Version` 字段 + 迁移函数 |
| 音乐与谱面对齐 | 固定谱面必须绑固定 BGM | 每张谱面记录 `BgmId`，加载时校验时长 |

---

## 10. 建议目录

```
Assets/GameMain/Data/Charts/*.asset      # 谱面
Assets/GameMain/Data/Buffs/*.asset       # Buff 定义
Assets/GameMain/Scripts/Charts/          # ChartDefinition / BuffDefinition / BuffSystem
Assets/Editor/Charts/ChartEditorWindow.cs
Assets/GameMain/Resources/Charts/        # 运行时加载（原型期）
Assets/GameMain/Resources/Buffs/
```

---

## 11. 建议的落地顺序

1. 先定 P0 的 10 个问题（尤其第 1、2、5、7 条，直接决定数据结构）。
2. P1 用「硬编码一张测试谱面」跑通 Buff 结算，验证战斗手感。
3. 手感 OK 再做编辑器，避免编辑器做完发现数值模型要推倒。

---

## 12. 规则 v0.1（按 2026-11-18 答复整理）

### 12.1 已确认

1. **取消基础伤害**：命中普通音符不再掉血，战局所有变化都来自 Buff。
2. **判定等级决定效果强度**：Perfect / Good / Bad / Miss 对应效果系数。
3. **一敌多谱**：一个敌人可绑定 1..N 张固定谱面；敌人攻击不写进谱面，沿用现有「每 N 次非完美攻击 / 每 N 小节掉血」。
4. **层间成长**：每 N 场战斗后三选一（N 可调，测试先设 1），选择物从“卡牌”换成“Buff”；界面沿用选卡界面，左侧显示效果描述。
5. **Buff 需要一组音符**：和卡牌的 `NoteCount` 类似，Buff 绑定 N 个音符，全部判定后才结算。

### 12.2 必须先解决的设计冲突

“固定谱面”和“玩家选 Buff”天然冲突：如果 Buff 写死在谱面里，层间选 Buff 就没有意义。

**建议方案：谱面留槽位，Buff 填槽。**

- 谱面编辑器里把若干组音符标记为 **BuffSlot**（槽位），槽位记录所需音符数 `NoteCount`。
- 进战斗时，把玩家持有的 Buff 按顺序（或手动）填入槽位。
- Buff 的 `NoteCount` 必须等于槽位音符数；填不满的槽位降级为普通音符。
- 结果：同一张固定谱面，因持有 Buff 不同而打法不同，层间成长才有意义。

需要你选：

- **A. 自动填**：按获取顺序填槽，玩家不操作（简单，推荐先做）。
- **B. 战前手动配**：进战斗前打开配装界面，把 Buff 拖进槽位（更策略，但多一个界面）。

### 12.3 判定削减

| 判定 | 权重 |
|---|---|
| Perfect | 1.0 |
| Good | 0.7 |
| Bad | 0.4 |
| Miss | 0 |

一个 Buff 占用 N 个音符，结算值 = `Power × 平均权重`；Miss 的权重 0 直接拉低平均，不额外惩罚。

- 伤害/治疗/护盾：四舍五入取整
- 倍率类：直接乘激活值

### 12.4 强拍

现有 Space 强拍机制先保留，作用改为：**带强拍标记的 Buff 槽位效果 ×1.5**。谱面编辑器里可以给槽位勾“强拍”。

### 12.5 Buff 定义字段（编辑器）

`Id`、`DisplayName`、`Description`、`EffectType`、`Power`、`NoteCount`(1..8)、`AccentEligible`、`Color`、`Icon`。

校验：Id 唯一、NoteCount 在 1..8、Power > 0、EffectType 合法。

### 12.6 谱面编辑器职责

- 4 轨 × 小节网格（1/8 格）放置/删除音符
- 框选一组音符 → “设为 Buff 槽位”，填槽位 Id 与所需音符数
- 槽位需连续或同组，校验：组内音符数 = NoteCount、槽位不重叠、每张谱至少一个槽位（可选）
- 保存为 `ChartDefinition`，并可选“用当前 Buff 预演填充”

### 12.7 还需要你确认的 3 件事

1. Buff 填槽：**自动**还是**手动**？
2. 玩家持有的 Buff 是否有**数量上限**（例如 6 个）？超了怎么办（替换/放弃）？
3. 强拍是否保留为“Buff ×1.5”？还是直接删掉强拍？

### 12.8 更新后的实施顺序

| 阶段 | 内容 |
|---|---|
| P1 | `BuffDefinition` / `ChartDefinition` + 战斗内 Buff 结算（先用一张写死的测试谱验证手感） |
| P2 | Buff 编辑器（沿用卡牌编辑器结构） |
| P3 | 谱面编辑器（网格 + 槽位 + 校验） |
| P4 | 层间 Buff 三选一 + 持有限制 + 配装 |
| P5 | 删除卡牌 UI/资产/测试，CI 改 charts/buffs 校验 |

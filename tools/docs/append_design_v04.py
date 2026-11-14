from docx import Document
from docx.shared import Pt

PATH = r"E:\UnityProgram\卡牌音游\策划文档.docx"


def heading(doc, text, size=14):
    p = doc.add_paragraph()
    run = p.add_run(text)
    run.bold = True
    run.font.size = Pt(size)
    return p


def body(doc, text):
    p = doc.add_paragraph()
    run = p.add_run(text)
    run.font.size = Pt(10.5)
    return p


def main():
    doc = Document(PATH)

    doc.add_paragraph()
    heading(doc, "十二、下一阶段设计决策（v0.4，2026-11-14）", 15)
    body(doc, "说明：本节把“从原型进入可试玩垂直切片”所需的设计决策一次定死，后续开发以本节为准；"
              "与《开工前决策清单》冲突时，以本节较新的日期为准。")

    heading(doc, "12.1 卡组成长", 12)
    body(doc, "· 起始牌组 8 张（当前 8 张示例牌），牌组上限 20 张。")
    body(doc, "· 每场战斗胜利后三选一：获得 1 张新牌。")
    body(doc, "· 每 3 层提供一次“删卡 / 升级”二选一：删卡优先，保持“薄牌组 = 单位音符效率更高”的核心设计。")
    body(doc, "· 战败即本局结束；暂不做元进度（解锁/永久强化），等垂直切片验证后再定。")

    heading(doc, "12.2 地图结构", 12)
    body(doc, "· 每章 10 层：战斗 6、事件 2、精英 1、Boss 1。")
    body(doc, "· 每 3 层出现一次分支：两条路节点类型不同（战斗/事件/精英），都通向下一层。")
    body(doc, "· 垂直切片先做 8 层线性：战斗 5、事件 1、精英 1、Boss 1；分支与事件内容延后。")

    heading(doc, "12.3 敌人模板", 12)
    body(doc, "· 攻击型：每 2 次非完美判定攻击一次（当前实现）。")
    body(doc, "· 干扰型：每 8 小节插入 1 个“必须命中”的额外音符，Miss 则玩家掉血。")
    body(doc, "· 护盾型：拥有护盾值，只有被强拍指定的牌能破盾；破盾前本体免伤。")
    body(doc, "· 所有敌人行为对齐小节线，并提前 1 小节给预警（HUD 闪烁 + 文字）。")
    body(doc, "· 数据放 EnemyDefinition（ScriptableObject）：血量、攻击、间隔、行为类型、预警拍数。")

    heading(doc, "12.4 音乐与音效", 12)
    body(doc, "· 垂直切片：单层循环 BGM（2 首占位 loop）+ 节拍器轻混，不做实时分层。")
    body(doc, "· 卡牌/按键音效继续用程序合成占位，验证后再替换采样。")
    body(doc, "· v0.3 的 L0/L1/L2 实时分层是最大自研成本，放到试玩验证通过之后。")

    heading(doc, "12.5 阶段划分与验收", 12)
    body(doc, "· P0 试玩验证（约 1 周）：5 名非本人试玩；指标：3 分钟内上手、能听出两套牌组的差异、"
              "失败后归因于“手”还是“牌组”。")
    body(doc, "· P1 肉鸽骨架（约 2–3 周）：卡组成长、8 层地图、3 类敌人、奖励/删卡界面、暂停与键位重绑。")
    body(doc, "· P2 表现打磨（约 3–4 周）：8 张卡面、角色/怪物替换、2 首 BGM、判定与命中特效。")
    body(doc, "· 垂直切片验收：一局 15–20 分钟，能完整走完 8 层并结算，胜率 30%–50%。")

    heading(doc, "12.6 技术待办（对应实现）", 12)
    body(doc, "· 暂停：Esc 暂停并冻结 dspTime 与谱面，恢复时重新对齐（禁止 timeScale=0）。")
    body(doc, "· 键位重绑：设置页支持改 4 个键，存 PlayerPrefs。")
    body(doc, "· 存档：牌组已存；后续加“当前层数/卡组/血量”存档。")
    body(doc, "· 资源管线：卡牌暂用 Resources，接入正式资源后迁到 GF 资源管线。")

    doc.save(PATH)
    print("saved", PATH)


if __name__ == "__main__":
    main()

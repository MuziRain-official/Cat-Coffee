# CLAUDE.md — 项目约定与工作流

本项目目标：**完全利用 AI（Claude + MCP for Unity）开发一款模拟经营类小体量休闲游戏**。
这是一份给 AI 的长期约定文档，每次会话都要遵守。

---

## 项目状态

- **引擎**：Unity `2022.3.62f3c1`，2D/3D 待定（P1 定玩法时确认）
- **路径**：`F:/UnityDemo/AI project`
- **当前阶段**：P0 基建（已完成）→ 进入 P1 定玩法
- **核心循环 / MVP 范围**：见 `Docs/GDD.md`

## 已确认的工作流决策

1. **先写精简 GDD 作锚点**——核心循环、MVP 范围、系统清单。每次迭代对照 GDD，防止跑偏。
2. **人工验收节奏**——每完成一个系统，用户亲自 play 验收，通过才进下一个。不要一路做到 MVP 再统一验收。
3. **美术音频 = AI 生成 + 风格锁**——用 `generate_image` / `generate_audio`，写死统一的 prompt 前缀保证风格一致。

## 宏观阶段

| 阶段 | 内容 | 结束标志 |
|------|------|----------|
| P0 基建 | git + 目录 + 本文件 | ✅ 完成 |
| P1 定玩法 | GDD：主题 + 核心循环 + MVP + 系统清单 | GDD 定稿 |
| P2 竖切片 | 最小可玩：1资源 + 1时间tick + 1建筑 + 1升级 + 买卖 | 用户 play 通过 |
| P3 系统扩展 | 更多建筑 / 解锁 / 存档 | 系统清单打勾 |
| P4 数值平衡 | 改 ScriptableObject（不碰代码） | 数值满意 |
| P5 美术音频 | generate_image / generate_audio | 资源到位 |
| P6 打磨打包 | 修边 + manage_build | 出包 |

## 微观循环（每个功能/系统内部，AI 自主完成）

```
定规格 → 实现 → 编译验证(read_console 归零) → 运行验证(play+截图+execute_code) → 修复迭代 → git commit
```

---

## 三条铁律

1. **数值与逻辑分离**：所有成本/收益/产量/升级费用放 `ScriptableObject`（在 `Assets/Data/`），逻辑只引用不硬编码。**调平衡 = 改 .asset，绝不改已验证的代码。**
2. **经济公式必须有测试**：每个经济系统配 EditMode 单元测试（`Assets/Scripts/Tests/`），靠 `run_tests` 自证"算得对"。
3. **git 是生命线**：每跑通一个系统 `git commit` 一次。没有检查点的改动等于裸奔。

---

## 目录结构约定

```
Assets/
├── Scripts/
│   ├── Core/        # 单例、GameManager、事件总线、扩展方法
│   ├── Data/        # ScriptableObject 定义类（数值容器，不是 .asset 实例）
│   ├── Systems/     # 各玩法系统：经济、时间、建造、升级、存档…
│   ├── UI/          # UI 绑定与表现
│   └── Tests/       # EditMode 单元测试（经济公式）
├── Data/            # ScriptableObject 实例（.asset），数值都在这里
├── Prefabs/         # 预制体
├── Art/             # AI 生成的美术资源
├── Audio/           # AI 生成的音频
└── Materials/       # 材质
```

## 命名约定

- 类/方法：PascalCase；字段 camelCase；私有字段 `_camelCase`
- ScriptableObject 定义类后缀 `SO`（如 `BuildingSO`），实例命名与类名一致
- 测试类后缀 `Tests`，方法名描述行为

## MCP 使用规范

- **改引擎状态前先读资源**：`mcpforunity://editor/state` 确认 `ready_for_tools`，`mcpforunity://project/tags` / `layers` 在加 tag/layer 前先读。
- **读状态用 resource，改动作用 tool**。
- **多实例时必须先 `set_active_instance`**。
- **批量操作用 `batch_execute`**，减少往返。
- **改完代码 `refresh_unity` + `read_console` 确认零报错**，再往下走。
- **写 C# 前可用 `unity_reflect` 验证 API 存在性**（训练数据可能过时，尤其 Unity 2022 的 API）。
- **验证画面用 `manage_camera` 截图，`execute_code` 跑纯逻辑**。

# ProtoPick 历史记录功能设计

> 日期: 2026-05-07
> 状态: Draft

## 背景

ProtoPick 是一个注入式 bookmarklet DOM 元素选择工具。用户选择元素、添加备注、收集候选元素后，生成结构化 Prompt 并复制到剪贴板。当前所有状态仅存于内存中，页面刷新或工具关闭后即丢失。

本设计为 ProtoPick 添加复制历史记录功能，保留最近 10 次复制的完整记录，支持查看元素选择、备注信息，并重新复制任意历史记录。

## 设计决策

| 决策项 | 选择 | 理由 |
|--------|------|------|
| 存储方式 | localStorage | 无需后端，bookmarklet 可直接访问目标页面的 localStorage |
| 作用范围 | 按域名隔离 | localStorage 天然按域名隔离，避免跨站信息泄露 |
| UI 入口 | 标题栏按钮 | 不增加面板高度，与暂停/关闭按钮并列 |
| 交互方式 | 下拉弹出层 | 不替换面板内容，快速查看后可关闭 |
| 详情展示 | 悬停预览 + 列表内展开 | 悬停快速浏览，点击展开查看完整 Prompt |
| 存储内容 | 结构化数据 + Prompt | 支持丰富的列表展示（元素、备注、时间） |

## 数据模型

### localStorage Key

```
protopick_history
```

### 记录结构

```json
{
  "id": "1778140000000",
  "timestamp": 1778140000000,
  "pagePath": "/dashboard",
  "pageTitle": "Dashboard - My App",
  "elements": [
    {
      "selector": "main > section:nth-child(1)",
      "tag": "section",
      "text": "Welcome back, Alex",
      "source": "HeroSection.vue:42",
      "component": "HeroSection",
      "annotation": "改背景为渐变色"
    }
  ],
  "prompt": "Page: /dashboard\n\n1. section.hero\n   selector: main > section:nth-child(1)\n   source: HeroSection.vue:42\n   component: HeroSection\n   text: \"Welcome back, Alex\"\n   html: <section class=\"hero\">...</section>\n   instruction: 改背景为渐变色",
  "summary": "section.hero + button.submit"
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `id` | string | 唯一标识，使用时间戳字符串 |
| `timestamp` | number | 记录创建时间 (ms) |
| `pagePath` | string | 页面路径 (`window.location.pathname`) |
| `pageTitle` | string | 页面标题 (`document.title`) |
| `elements` | array | 选择的元素列表，含 selector、tag、text、source、component、annotation |
| `prompt` | string | 最终生成的完整 Prompt 文本 |
| `summary` | string | 摘要（前 2 个元素的 tag.class 拼接），用于列表显示 |

### 容量与限制

- **最大条数**: 10 条
- **淘汰策略**: FIFO — 满时删除最旧的记录
- **去重**: 连续两条内容完全相同的记录不重复存储（通过比较 prompt 文本判断）
- **预估大小**: 每条约 2-5 KB，10 条总计 20-50 KB，远低于 localStorage 5 MB 限制

## UI 设计

### 入口按钮

在面板标题栏暂停按钮 (⏸) 左侧新增历史记录按钮 (📋)。

**按钮状态**:
- 默认: 显示 📋 图标
- 有历史记录时: 图标右上角显示数量角标（红色圆形，白字，类似未读消息数）
- 角标数字 = 当前域名下的历史记录条数

**位置**:

```
[● Selecting]          [📋] [⏸] [✕]
```

### 下拉弹出层

点击 📋 按钮后，从标题栏下方弹出悬浮列表。

**布局**:
- 宽度与面板一致 (340px)
- 最大高度 400px，超出时内部滚动
- 圆角底部，深色背景 `#1e293b`
- 通过 `position: absolute` 定位于标题栏正下方
- z-index 与面板其他元素同级

**关闭条件**:
- 再次点击 📋 按钮
- 点击弹出层外部区域
- 按 Esc 键

**空状态**:
- 居中显示"暂无复制历史"文字提示
- 灰色图标 + 说明文字

### 列表项

每条历史记录显示为一行，包含：

```
[摘要文字]                    [复制] [详情]
[元素数量 · 相对时间]
```

- **摘要**: `summary` 字段（前 2 个元素的 tag.class）
- **元素数量**: "N 个元素"
- **相对时间**: "刚刚" / "5分钟前" / "1小时前" / "昨天" / 日期
- **复制按钮**: 紧凑文字按钮，点击直接复制该条 Prompt
- **详情按钮**: 紧凑文字按钮，点击展开完整 Prompt

### 悬停预览

鼠标悬停在列表项上时，在该条目下方显示简要预览区域：

```
元素预览：
1. section.hero — 改背景为渐变色
2. button.submit
```

- 背景色 `#0f172a`（比列表项更深）
- 显示每个元素的 selector 和 annotation（如有）
- 悬停出现，移出消失

### 列表内展开详情

点击「详情」按钮后，在该条目下方展开 Prompt 文本区域：

- 背景色 `#0f172a`
- monospace 字体
- 最大高度 150px，超出滚动
- `white-space: pre-wrap` 保留 Prompt 格式
- 底部操作栏：「复制 Prompt」按钮 + 「删除」按钮
- 「详情」按钮文字变为「收起 ▲」，点击收起

### 底部操作

弹出层底部固定显示「清空全部」按钮：
- 点击后弹出确认提示（`confirm()`）
- 确认后清空当前域名的所有历史记录

## 触发时机

每次执行"复制 Prompt"操作时自动保存历史记录：

1. 用户按 Cmd/Ctrl+C 触发复制
2. 用户点击"Copy Prompt"按钮触发复制
3. 在 `writeToClipboard()` 函数中，复制成功后调用保存历史记录的函数

**去重逻辑**: 与上一条记录比较 `prompt` 文本，完全相同则跳过存储。

## 相对时间格式

| 时间差 | 显示 |
|--------|------|
| < 60 秒 | 刚刚 |
| < 60 分钟 | N分钟前 |
| < 24 小时 | N小时前 |
| < 48 小时 | 昨天 |
| >= 48 小时 | MM-DD |

## 视觉规范

| 元素 | 颜色 | 说明 |
|------|------|------|
| 弹出层背景 | `#1e293b` | 与面板一致 |
| 预览区域背景 | `#0f172a` | 比列表更深 |
| 数量角标 | `#ef4444` 红 | 右上角圆形 |
| 角标文字 | `#ffffff` 白 | |
| 复制按钮 | `#6366f1` 紫 | 与 Copy Prompt 按钮一致 |
| 详情按钮 | `#334155` 灰 | |
| 摘要文字 | `#e2e8f0` 亮灰 | |
| 元信息文字 | `#94a3b8` 灰 | |
| 备注文字 | `#f59e0b` 黄 | 与 annotation 风格一致 |
| Prompt 文字 | `#94a3b8` 灰 | monospace |
| "Copied" 反馈 | `#4ade80` 绿 | 复制成功后显示 2 秒 |

## 文件变更范围

| 文件 | 变更 |
|------|------|
| `frontend/public/protopick/assets/editor.js` | 新增历史记录逻辑（存储、UI、交互） |
| `frontend/public/protopick/assets/editor.css` | 新增历史记录相关样式 |

所有变更限于这两个文件，不涉及后端或其他前端代码。

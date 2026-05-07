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
  "id": "1778140000123-a3f",
  "timestamp": 1778140000123,
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
  "prompt": "Page: /dashboard\n\n1. section.hero\n   selector: main > section:nth-child(1)\n   ...",
  "summary": "section.hero + button.submit"
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `id` | string | 唯一标识，格式 `{timestamp}-{4位随机hex}`，避免同毫秒碰撞 |
| `timestamp` | number | 记录创建时间 (ms) |
| `pagePath` | string | 页面路径 (`window.location.pathname`) |
| `pageTitle` | string | 页面标题 (`document.title`) |
| `elements` | array | 选择的元素列表，含 selector、tag、text、source、component、annotation |
| `prompt` | string | 最终生成的完整 Prompt 文本 |
| `summary` | string | 摘要，使用现有 `elementLabel()` 函数取前 2 个元素的标签拼接 |

### 容量与限制

- **最大条数**: 10 条
- **淘汰策略**: FIFO — 满时删除最旧的记录
- **去重**: 与最新一条记录比较 `prompt` 文本，完全相同则跳过存储（跨会话也生效，因为数据持久化在 localStorage）
- **预估大小**: 每条约 2-5 KB，10 条总计 20-50 KB，远低于 localStorage 5 MB 限制

## UI 设计

### 入口按钮

在面板标题栏暂停按钮 (⏸) 左侧新增历史记录按钮。按钮使用 SVG 图标（与现有暂停/关闭按钮风格一致），而非 emoji。

**按钮状态**:
- 默认: 显示历史图标 SVG
- 有历史记录时: 图标右上角显示数量角标（红色圆形 `#ef4444`，白字，类似未读消息数）
- 角标数字 = 当前域名下的历史记录条数
- 角标在每次复制操作成功后更新，面板初始化时也读取并显示

**位置**:

```
[● Selecting]          [历史] [⏸] [✕]
```

### 下拉弹出层

点击历史按钮后，从标题栏下方弹出悬浮列表。

**布局**:
- 宽度与面板一致 (340px)
- 最大高度 400px，超出时内部滚动
- 圆角底部
- 作为面板 DOM 的子元素（`appendChild` 到 chatPanel），使用 `position: absolute` 相对于面板定位。因为弹出层是面板的子元素，拖动面板时弹出层自动跟随
- `z-index: 2147483646`（高于面板内容，低于遮罩层）

**关闭条件**:
- 再次点击历史按钮
- 点击弹出层外部区域（通过 `on(document, 'mousedown', ...)` 监听，使用现有 `on()` 注册机制）
- 按 Esc 键（插入到现有 Esc 优先级链的**最前面**，见下方"键盘事件"章节）

**空状态**:
- 居中显示"暂无复制历史"文字提示
- 灰色图标 + 说明文字

### 列表项

每条历史记录显示为一行，包含：

```
[摘要文字]                    [复制] [详情]
[元素数量 · 相对时间]
```

- **摘要**: `summary` 字段，使用 `elementLabel()` 的输出拼接
- **元素数量**: "N 个元素"
- **相对时间**: "刚刚" / "5分钟前" / "1小时前" / "昨天" / 日期
- **复制按钮**: 紧凑文字按钮，点击直接复制该条 Prompt
- **详情按钮**: 紧凑文字按钮，点击展开完整 Prompt

### 悬停预览与展开详情的交互

悬停预览和展开详情共用同一个区域（条目下方的空间），互斥显示：

- **条目处于折叠状态** → 悬停时显示简要预览（元素 selector + annotation），移出消失
- **条目处于展开状态** → 悬停预览不显示，该区域始终显示完整 Prompt 文本
- 展开详情时自动关闭其他已展开的条目（手风琴模式）

### 列表内展开详情

点击「详情」按钮后，在该条目下方展开 Prompt 文本区域：

- 背景色 `#0a0a0f`（与现有面板内部背景一致）
- monospace 字体
- 最大高度 150px，超出滚动
- `white-space: pre-wrap` 保留 Prompt 格式
- 底部操作栏：「复制 Prompt」按钮 + 「删除」按钮
- 「详情」按钮文字变为「收起 ▲」，点击收起

### 底部操作

弹出层底部固定显示「清空全部」按钮：
- 使用自定义确认弹窗（复用面板内的 inline 确认 UI），而非 `window.confirm()`（某些页面可能覆盖 confirm）
- 确认后清空当前域名的所有历史记录

## 键盘事件

### Esc 优先级链（更新后）

在现有 Esc 处理链的**最前面**插入历史弹出层的关闭：

```
Esc 处理优先级（从高到低）:
1. 关闭历史下拉弹出层（如已打开）
2. 关闭 annotation popover（如已打开）
3. 清除当前选中元素
4. 清空候选元素列表
```

弹出层打开时，Esc 仅关闭弹出层，不传递到后续优先级。

## 触发时机与数据序列化

### 保存时机

所有复制路径都在成功后触发历史保存：

1. **主路径**: `writeToClipboard()` 中 `navigator.clipboard.writeText()` 的 `.then()` 回调内调用保存
2. **降级路径**: `fallbackCopy()`（`execCommand("copy")`）成功后同样调用保存
3. 两条路径在调用保存前都已完成 Prompt 文本构建

### 结构化数据提取

新增函数 `serializeElementContext(element)` 从单个 DOM 元素提取结构化数据：

```javascript
function serializeElementContext(el) {
  const aiId = el.getAttribute('data-ai-id');
  const ctx = buildElementContext(el); // 复用现有函数
  return {
    selector: ctx.selector,
    tag: ctx.tag,
    text: ctx.text,
    source: ctx.source || '',
    component: ctx.component || '',
    annotation: annotations.get(aiId) || ''
  };
}
```

在 `copyPrompt()` 中，构建 Prompt 文本的同时，遍历 `candidateElements`（和 `selectedElements`）调用 `serializeElementContext()` 收集结构化数据，传入历史保存函数。

### 历史保存函数

```javascript
function saveToHistory(promptText, elements) {
  try {
    const history = loadHistory();
    // 去重：与最新一条比较 prompt
    if (history.length > 0 && history[0].prompt === promptText) return;
    const record = {
      id: Date.now().toString(36) + '-' + Math.random().toString(16).slice(2, 6),
      timestamp: Date.now(),
      pagePath: window.location.pathname,
      pageTitle: document.title,
      elements: elements,
      prompt: promptText,
      summary: elements.slice(0, 2).map(e => elementLabel(/* 通过 tag+class 构建 */)).join(' + ')
    };
    history.unshift(record);
    if (history.length > 10) history.length = 10;
    localStorage.setItem('protopick_history', JSON.stringify(history));
    updateHistoryBadge();
  } catch (e) {
    // localStorage 不可用或已满 — 静默失败，不影响复制操作
  }
}
```

**错误处理**: 整个保存操作包裹在 `try/catch` 中。`localStorage` 不可用（隐私模式、被禁用）或空间不足（`QuotaExceededError`）时，静默跳过，不中断用户的复制流程。

### 角标更新

- `updateHistoryBadge()`: 读取历史记录数量，更新按钮上的角标数字和可见性
- 调用时机：面板初始化时、每次成功复制后、删除/清空记录后

## 生命周期与清理

### DOM 创建

历史下拉弹出层 DOM 在首次点击历史按钮时懒创建（不在 `initEditor` 时创建），缓存到变量 `historyDropdown` 中。后续点击仅切换显示/隐藏。

### 事件监听

所有事件监听使用现有 `on()` 注册机制：

- 历史按钮点击: `on(historyBtn, 'click', ...)`
- 外部点击关闭: `on(document, 'mousedown', ...)`
- 列表项交互: `on(item, 'mouseenter/mouseleave/click', ...)`
- 删除/清空按钮: `on(btn, 'click', ...)`

这些监听器会自动包含在 `destroy()` 的清理流程中（现有 `listeners` 数组机制）。

### destroy() 清理

在 `destroy()` 函数中补充：

- 将 `historyDropdown` 设为 `null`
- 弹出层 DOM 随面板一起移除（因为它是面板的子元素）
- 不清空 localStorage 中的历史数据（持久化保留）

## 相对时间格式

| 时间差 | 显示 |
|--------|------|
| < 60 秒 | 刚刚 |
| < 60 分钟 | N分钟前 |
| < 24 小时 | N小时前 |
| < 48 小时 | 昨天 |
| >= 48 小时 | MM-DD |

## 视觉规范

颜色与现有面板风格统一：

| 元素 | 颜色 | 说明 |
|------|------|------|
| 弹出层背景 | `#0a0a0f` | 与面板背景一致 |
| 列表项背景 | `#141420` | 略浅于弹出层 |
| 预览/展开区背景 | `#0a0a0f` | 与面板背景一致 |
| 数量角标 | `#ef4444` 红 | 右上角圆形 |
| 角标文字 | `#ffffff` 白 | |
| 复制按钮 | `#6366f1` 紫 | 与 Copy Prompt 按钮一致 |
| 详情按钮 | `#334155` 灰 | |
| 摘要文字 | `#e2e8f0` 亮灰 | |
| 元信息文字 | `#94a3b8` 灰 | |
| 备注文字 | `#f59e0b` 黄 | 与 annotation 风格一致 |
| Prompt 文字 | `#94a3b8` 灰 | monospace |
| "Copied" 反馈 | `#4ade80` 绿 | 复制成功后显示 2 秒 |

所有图标使用 inline SVG，与现有暂停、关闭、批注等按钮风格一致。

## 文件变更范围

| 文件 | 变更 |
|------|------|
| `frontend/public/protopick/assets/editor.js` | 新增：`serializeElementContext()`、`saveToHistory()`、`loadHistory()`、`updateHistoryBadge()`、`createHistoryDropdown()` 函数；修改：`writeToClipboard()` 添加 `.then()` 回调、`fallbackCopy()` 添加保存调用、`copyPrompt()` 添加结构化数据收集、`handleKeyDown()` 更新 Esc 优先级链、`destroy()` 添加清理 |
| `frontend/public/protopick/assets/editor.css` | 新增：`.ai-editor-history-btn`、`.ai-editor-history-badge`、`.ai-editor-history-dropdown`、`.ai-editor-history-item`、`.ai-editor-history-preview`、`.ai-editor-history-detail` 相关样式 |

所有变更限于这两个文件，不涉及后端或其他前端代码。

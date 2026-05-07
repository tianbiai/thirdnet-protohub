# ProtoPick DOM 导航快捷键设计

## 背景

ProtoPick 当前通过鼠标悬停高亮、点击选取元素。用户需要精确选取某个元素时，如果鼠标悬停到了不理想的层级，只能通过面包屑导航手动点击父/子元素跳转。这个过程需要手离开键盘去操作鼠标，效率较低。

## 目标

增加方向键 DOM 导航功能，让用户可以在 DOM 树中用键盘快速移动焦点，提升元素选取效率。

## 设计方案

### 方案选择：始终跟随焦点（方案 A）

方向键始终作用于当前悬停元素。悬停和键盘共享同一个"当前焦点"，鼠标移动也会更新焦点。

选择理由：ProtoPick 的核心使用场景是快速指向元素，鼠标和键盘应无缝协作。不需要模式切换，认知负担最低。

### 状态管理

当前代码中有两种元素状态：`lastMoveTarget`（鼠标原始目标）和 `selectedElements[]`（已选元素）。方向键导航不引入新的独立状态变量——它复用现有的悬停机制：

1. 方向键按下时，计算出目标元素
2. 调用现有的 `showHover(targetElement)` 移动高亮框
3. 更新 `lastMoveTarget` 为目标元素，使后续鼠标移动的基准一致

**焦点与选择的关系**：方向键只影响悬停高亮，不影响已选元素。如果元素已被选中（有实线边框），方向键导航时选中状态不变，悬停框移动到新位置。用户可以随后点击新焦点元素来选取它。

**鼠标覆盖行为**：方向键导航后，任何鼠标移动都会通过 `handleMouseMove` → `showHover(resolveTarget(...))` 覆盖键盘设置的焦点。这是期望行为——键盘导航是临时的，鼠标操作随时接管。

**无初始焦点时**：如果没有悬停元素（工具刚启动、暂停恢复后、鼠标离开文档），按下方向键时不做任何操作。用户需要先移动鼠标或点击一个元素建立初始焦点。

### 方向键映射

| 按键 | 动作 | 行为 |
|------|------|------|
| `↑` | 父元素 | 移动到当前元素的直接父元素（跳过不可见元素） |
| `↓` | 子元素 | 移动到当前元素的第一个有意义子元素 |
| `←` | 前兄弟 | 移动到当前元素的前一个兄弟元素 |
| `→` | 后兄弟 | 移动到当前元素的后一个兄弟元素 |

### 导航逻辑

1. **上（父元素）**：取 `el.parentElement`。如果父元素不存在、是 `document.documentElement` 或 `document.body`，则不动。如果父元素不可见（`display:none` / `visibility:hidden` / `opacity:0`），继续向上查找到第一个可见祖先。到达 body 时停止。不使用 `resolveTarget()`，因为它的向上遍历语义（跳到有意义祖先）不适合显式的"上一级"操作——用户按上键就是想看父元素，而不是跳过它。

2. **下（子元素）**：遍历 `el.children`，找第一个通过 `isMeaningful()` 检查且可见的子元素。如果没有有意义的子元素，则不动。

3. **左（前兄弟）**：从当前元素向前遍历 `el.parentElement.children`，找第一个通过 `isMeaningful()` 检查且可见的前兄弟元素。如果 `isMeaningful()` 的 `children.length > 1` 规则导致导航到布局 div，这是可接受的——方向键导航是显式操作，和悬停时的自动解析有不同的语义预期。

4. **右（后兄弟）**：从当前元素向后遍历 `el.parentElement.children`，找第一个通过 `isMeaningful()` 检查且可见的后兄弟元素。

### 视觉反馈

- **成功移动**：调用 `showHover(targetElement)` 移动悬停框，效果与鼠标悬停一致（橙色虚线边框 `rgba(249, 115, 22, 0.35)`）
- **到达边界**（无父/无子/无兄弟）：给悬停框元素（`hoverBox`）添加 `.ai-editor-flash-boundary` 类，200ms 后移除。CSS 动画将边框颜色短暂变为红色 `#ef4444`：

```css
@keyframes ai-editor-flash-boundary {
  0% { border-color: rgba(249, 115, 22, 0.35); }
  50% { border-color: rgba(239, 68, 68, 0.8); }
  100% { border-color: rgba(249, 115, 22, 0.35); }
}
.ai-editor-flash-boundary {
  animation: ai-editor-flash-boundary 200ms ease-out;
}
```

### 事件处理

在现有的 `handleKeyDown` 函数（editor.js 第 472 行附近）中增加 ArrowKey 分支：

```javascript
if (["ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight"].includes(e.key)) {
  if (paused) return;
  // 注释弹窗的 textarea 已通过 stopPropagation 阻止事件冒泡
  // 但仍需检查其他可编辑元素
  if (e.target.isContentEditable) return;
  const tag = e.target.tagName;
  if (tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT") return;

  e.preventDefault(); // 阻止方向键滚动页面

  navigateDOM(e.key);
  return;
}
```

`navigateDOM(key)` 函数：
1. 获取当前悬停元素：从 `hoverBox` 的定位目标或 `lastMoveTarget` 经 `resolveTarget()` 后的元素
2. 如果没有当前悬停元素，直接返回
3. 根据方向键计算目标元素
4. 如果目标存在：调用 `showHover(target)` 并更新 `lastMoveTarget`
5. 如果目标不存在（边界）：给 `hoverBox` 添加闪红动画类

### 边界条件

- **输入框内不触发**：当焦点在 `textarea`、`input`、`select` 等可编辑元素中时，方向键正常编辑文本。注释弹窗的 textarea 通过 `stopPropagation` 已阻止冒泡，无需额外处理；但仍需检查页面原生的可编辑元素
- **暂停状态不触发**：在 arrow key 处理中显式检查 `if (paused) return;`。注意：`keydown` 监听器（通过 `on(document, "keydown", handleKeyDown, true)`）在暂停时不会被移除，必须在分支内检查 paused 状态
- **到达 body 根节点**：`↑` 不再向上移动，闪红提示
- **无有意义的子元素**：`↓` 不移动，闪红提示
- **无前/后兄弟**：`←` / `→` 不移动，闪红提示
- **ProtoPick 自身元素排除**：导航时跳过带有 `data-ai-id` 属性但属于编辑器面板自身的元素（class 以 `ai-editor-` 开头）

### 面板 UI 更新

在 `createChatPanel()` 函数（editor.js 第 612-618 行）的快捷键栏中，在现有 `<span>` 之间新增：

```html
<span><kbd>↑↓←→</kbd> DOM导航</span>
```

## 修改文件

| 文件 | 修改内容 |
|------|---------|
| `frontend/public/protopick/assets/editor.js` | 新增 `navigateDOM(key)` 函数（约 60 行），在 `handleKeyDown` 中增加 ArrowKey 分支（约 15 行），在 `createChatPanel` 快捷键栏增加提示 |
| `frontend/public/protopick/assets/editor.css` | 新增 `@keyframes ai-editor-flash-boundary` 动画和 `.ai-editor-flash-boundary` 类（约 10 行） |

## 不做的事

- 不增加"锁定模式"（键盘导航时屏蔽鼠标）
- 不增加"导航历史栈"（和面包屑功能重叠）
- 不修改现有的面包屑导航功能
- 不修改现有的选取/注释/候选区流程
- 不引入独立于悬停机制的"焦点元素"状态变量

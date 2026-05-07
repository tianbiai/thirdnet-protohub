# ProtoPick DOM 导航快捷键 实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在 ProtoPick 中添加方向键 DOM 导航功能，用 ↑↓←→ 在 DOM 树中快速移动悬停焦点。

**Architecture:** 复用现有悬停机制（`showHover` + `lastMoveTarget`），不引入新状态变量。在 `handleKeyDown` 中添加 arrow key 分支，调用新函数 `navigateDOM` 计算目标元素并移动悬停框。CSS 中添加边界闪烁动画。

**Tech Stack:** 纯 JavaScript（IIFE），CSS 动画。无框架依赖。ProtoPick 是静态 bookmarklet 工具，无构建系统和测试框架，测试需手动进行。

**Spec:** `docs/superpowers/specs/2026-05-07-protopick-dom-nav-shortcuts-design.md`

---

## File Structure

| File | Action | Responsibility |
|------|--------|---------------|
| `frontend/public/protopick/assets/editor.css` | Modify | 添加边界闪烁动画 |
| `frontend/public/protopick/assets/editor.js` | Modify | 添加 `navigateDOM` 函数 + `handleKeyDown` 分支 + 面板快捷键提示 |

---

### Task 1: 添加 CSS 边界闪烁动画

**Files:**
- Modify: `frontend/public/protopick/assets/editor.css` (末尾追加)

- [ ] **Step 1: 在 editor.css 末尾添加闪烁动画**

在文件最后一行（第 681 行）之后追加：

```css
/* ── DOM nav boundary flash ────────────────────────────── */
@keyframes ai-editor-flash-boundary {
  0% { border-color: rgba(249, 115, 22, 0.35); }
  50% { border-color: rgba(239, 68, 68, 0.8); }
  100% { border-color: rgba(249, 115, 22, 0.35); }
}
.ai-editor-flash-boundary {
  animation: ai-editor-flash-boundary 200ms ease-out;
}
```

- [ ] **Step 2: 验证 CSS 语法**

确认新增 CSS 不破坏已有样式。`.ai-editor-flash-boundary` 类将应用到 `.ai-editor-hover-box` 元素上。

---

### Task 2: 添加 navigateDOM 函数

**Files:**
- Modify: `frontend/public/protopick/assets/editor.js` (在 `handleKeyDown` 函数之前插入)

- [ ] **Step 1: 在 editor.js 中 `handleKeyDown` 函数之前（第 472 行之前）插入 `navigateDOM` 函数**

```javascript
  // ── DOM navigation ──────────────────────────────────────
  function navigateDOM(key) {
    const current = lastMoveTarget ? resolveTarget(lastMoveTarget) : null;
    if (!current) return;

    let target = null;

    if (key === "ArrowUp") {
      // 父元素：向上查找第一个可见祖先，到达 body 停止
      let parent = current.parentElement;
      while (parent && parent !== document.body && parent !== document.documentElement) {
        if (!isEditorElement(parent) && isVisible(parent)) { target = parent; break; }
        parent = parent.parentElement;
      }
    } else if (key === "ArrowDown") {
      // 子元素：找第一个有意义且可见的子元素
      const children = current.children;
      for (let i = 0; i < children.length; i++) {
        if (!isEditorElement(children[i]) && isVisible(children[i]) && isMeaningful(children[i])) {
          target = children[i];
          break;
        }
      }
    } else if (key === "ArrowLeft") {
      // 前兄弟：向前找第一个有意义且可见的兄弟
      let prev = current.previousElementSibling;
      while (prev) {
        if (!isEditorElement(prev) && isVisible(prev) && isMeaningful(prev)) { target = prev; break; }
        prev = prev.previousElementSibling;
      }
    } else if (key === "ArrowRight") {
      // 后兄弟：向后找第一个有意义且可见的兄弟
      let next = current.nextElementSibling;
      while (next) {
        if (!isEditorElement(next) && isVisible(next) && isMeaningful(next)) { target = next; break; }
        next = next.nextElementSibling;
      }
    }

    if (target) {
      showHover(target);
      lastMoveTarget = target;
    } else {
      // 到达边界：闪烁提示（先移除再添加以支持快速重复按键）
      hoverBox.classList.remove(NS + "-flash-boundary");
      void hoverBox.offsetWidth;
      hoverBox.classList.add(NS + "-flash-boundary");
      setTimeout(function () { hoverBox.classList.remove(NS + "-flash-boundary"); }, 220);
    }
  }
```

---

### Task 3: 在 handleKeyDown 中添加方向键分支

**Files:**
- Modify: `frontend/public/protopick/assets/editor.js:472-498` (`handleKeyDown` 函数)

- [ ] **Step 1: 在 `handleKeyDown` 函数的 `Escape` 分支之前插入方向键处理**

在第 476 行 `const mod = e.metaKey || e.ctrlKey;` 之后，第 477 行 `if (e.key === "Escape")` 之前，插入：

```javascript
    // Arrow key DOM navigation
    if (e.key === "ArrowUp" || e.key === "ArrowDown" || e.key === "ArrowLeft" || e.key === "ArrowRight") {
      if (paused) return;
      if (e.target.tagName === "INPUT" || e.target.tagName === "TEXTAREA" || e.target.tagName === "SELECT") return;
      e.preventDefault();
      navigateDOM(e.key);
      return;
    }
```

注意：`handleKeyDown` 中第 473 行的 `isContentEditable` 检查和第 474 行的编辑器 INPUT/TEXTAREA 检查已先于本分支执行。注释弹窗的 textarea 通过 `stopPropagation`（第 544 行）阻止冒泡，不会到达此处理器。本分支只需检查页面原生的 INPUT/TEXTAREA/SELECT。

---

### Task 4: 更新面板快捷键提示

**Files:**
- Modify: `frontend/public/protopick/assets/editor.js:612-618` (createChatPanel 内的 shortcuts div)

- [ ] **Step 1: 在快捷键栏中添加 DOM 导航提示**

将第 612-618 行的 shortcuts div：

```html
        <div class="${NS}-shortcuts">
          <span><kbd>Click</kbd> 选取</span>
          <span><kbd>⌘Enter</kbd> 加入候选</span>
          <span><kbd>⌘C</kbd> 复制</span>
          <span><kbd>Esc</kbd> 清除</span>
          <span><kbd>⇧P</kbd> 暂停</span>
        </div>
```

修改为：

```html
        <div class="${NS}-shortcuts">
          <span><kbd>Click</kbd> 选取</span>
          <span><kbd>↑↓←→</kbd> DOM导航</span>
          <span><kbd>⌘Enter</kbd> 加入候选</span>
          <span><kbd>⌘C</kbd> 复制</span>
          <span><kbd>Esc</kbd> 清除</span>
          <span><kbd>⇧P</kbd> 暂停</span>
        </div>
```

仅在第 613 行之后插入了一行 `<span><kbd>↑↓←→</kbd> DOM导航</span>`。

---

### Task 5: 手动测试

ProtoPick 是静态 bookmarklet 工具，没有自动化测试框架。所有测试通过浏览器手动验证。

- [ ] **Step 1: 启动本地 HTTP 服务器**

```bash
cd frontend/public/protopick
python -m http.server 8888
```

- [ ] **Step 2: 在浏览器中打开 `http://localhost:8888`**

- [ ] **Step 3: 将 ProtoPick 书签拖到书签栏**

- [ ] **Step 4: 打开一个测试页面（如 `http://localhost:8888` 自身），点击书签激活 ProtoPick**

- [ ] **Step 5: 测试基本导航**

| 操作 | 预期结果 |
|------|---------|
| 鼠标悬停一个元素，按 `↑` | 悬停框移到父元素 |
| 按 `↓` | 悬停框移到第一个有意义子元素 |
| 按 `←` | 悬停框移到前一个兄弟 |
| 按 `→` | 悬停框移到后一个兄弟 |
| 鼠标移动 | 键盘焦点被鼠标覆盖 |

- [ ] **Step 6: 测试边界行为**

| 操作 | 预期结果 |
|------|---------|
| 在 body 直接子元素上按 `↑` 到达 body | 悬停框闪红 |
| 在叶子元素（无子元素）上按 `↓` | 悬停框闪红 |
| 在第一个兄弟上按 `←` | 悬停框闪红 |
| 在最后一个兄弟上按 `→` | 悬停框闪红 |

- [ ] **Step 7: 测试不影响现有功能**

| 操作 | 预期结果 |
|------|---------|
| 点击元素选取 | 正常选中（不受方向键影响） |
| 在已选元素上按方向键 | 选中状态不变，悬停框移动 |
| 点击注释按钮，在 textarea 中按方向键 | 光标正常移动，不触发导航 |
| 按 `⇧P` 暂停后按方向键 | 不触发导航 |
| `⌘Enter` / `⌘C` / `Esc` | 原有快捷键不受影响 |

- [ ] **Step 8: 确认面板快捷键栏显示 `↑↓←→ DOM导航`**

---

### Task 6: 提交

- [ ] **Step 1: 提交所有更改**

```bash
cd E:/SVN/AI/V1.0.0/Program/2_SourceCode/InterFace/protohub
git add frontend/public/protopick/assets/editor.css frontend/public/protopick/assets/editor.js
git commit -m "feat(protopick): add arrow key DOM navigation for element selection

Use ↑↓←→ to navigate parent/child/sibling elements in DOM tree.
Reuses existing hover mechanism (showHover + lastMoveTarget).
Flash-red animation on boundary (no parent/child/sibling).

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

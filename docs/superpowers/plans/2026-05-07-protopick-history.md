# ProtoPick History Feature Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a copy history feature to ProtoPick that persists the last 10 copied prompts in localStorage with structured element data, accessible via a dropdown from the panel header.

**Architecture:** New history functions (load/save/serialize/UI) added to the existing IIFE in `editor.js`. CSS classes added to `editor.css`. History dropdown is lazily created as a child of `chatPanel` for automatic drag-follow. All event listeners registered via the existing `on()` mechanism for cleanup.

**Tech Stack:** Vanilla JavaScript (IIFE), CSS, localStorage API

**Spec:** `docs/superpowers/specs/2026-05-07-protopick-history-design.md`

---

### Task 1: Add history state variables and data functions

**Files:**
- Modify: `frontend/public/protopick/assets/editor.js`

Add new state variables alongside existing ones (after line 29), and add the core data functions: `loadHistory()`, `saveToHistory()`, `serializeElementContext()`, `relativeTime()`, and `buildSummary()`.

- [ ] **Step 1: Add state variables after line 29 (`let paused = false;`)**

```javascript
  let historyDropdown = null;
  let historyOpen = false;
  let expandedHistoryId = null;
```

- [ ] **Step 2: Add `loadHistory()` function (place after the `clearAllCandidates()` function, around line 451)**

```javascript
  // ── History storage ─────────────────────────────────────────
  const HISTORY_KEY = "protopick_history";
  const HISTORY_MAX = 10;

  function loadHistory() {
    try {
      const raw = localStorage.getItem(HISTORY_KEY);
      if (!raw) return [];
      const data = JSON.parse(raw);
      return Array.isArray(data) ? data : [];
    } catch (_) {
      return [];
    }
  }

  function serializeElementContext(el) {
    const aiId = el.getAttribute(AI_ID);
    const ctx = buildElementContext(el);
    return {
      selector: ctx.selector,
      tag: ctx.tag,
      text: ctx.text,
      source: ctx.source || "",
      component: ctx.component || "",
      annotation: annotations.get(aiId) || ""
    };
  }

  function buildSummary(serializedElements) {
    return serializedElements.slice(0, 2).map(e => {
      if (e.annotation) return `${e.tag}.${e.selector.split(">").pop().trim().replace(/:nth-child\(\d+\)/, "")} ✎`;
      const seg = e.selector.split(">").pop().trim();
      return seg.length > 24 ? seg.slice(0, 24) + "…" : seg;
    }).join(" + ");
  }

  function relativeTime(ts) {
    const diff = Date.now() - ts;
    const sec = Math.floor(diff / 1000);
    if (sec < 60) return "刚刚";
    const min = Math.floor(sec / 60);
    if (min < 60) return `${min}分钟前`;
    const hr = Math.floor(min / 60);
    if (hr < 24) return `${hr}小时前`;
    if (hr < 48) return "昨天";
    const d = new Date(ts);
    return `${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
  }

  function saveToHistory(promptText, elements) {
    try {
      const history = loadHistory();
      if (history.length > 0 && history[0].prompt === promptText) return;
      const now = Date.now();
      const record = {
        id: now.toString(36) + "-" + Math.random().toString(16).slice(2, 6),
        timestamp: now,
        pagePath: window.location.pathname,
        pageTitle: document.title,
        elements: elements,
        prompt: promptText,
        summary: buildSummary(elements)
      };
      history.unshift(record);
      if (history.length > HISTORY_MAX) history.length = HISTORY_MAX;
      localStorage.setItem(HISTORY_KEY, JSON.stringify(history));
      updateHistoryBadge();
    } catch (_) {
      // localStorage unavailable or full — silent fail
    }
  }

  function deleteHistoryRecord(id) {
    try {
      let history = loadHistory();
      history = history.filter(r => r.id !== id);
      localStorage.setItem(HISTORY_KEY, JSON.stringify(history));
      updateHistoryBadge();
    } catch (_) {}
  }

  function clearAllHistory() {
    try {
      localStorage.removeItem(HISTORY_KEY);
      updateHistoryBadge();
    } catch (_) {}
  }

  function updateHistoryBadge() {
    if (!chatPanel) return;
    const badge = chatPanel.querySelector(`.${NS}-history-badge`);
    if (!badge) return;
    const count = loadHistory().length;
    if (count > 0) {
      badge.textContent = count > 9 ? "9+" : String(count);
      badge.style.display = "";
    } else {
      badge.style.display = "none";
    }
  }
```

- [ ] **Step 3: Commit**

```bash
git add frontend/public/protopick/assets/editor.js
git commit -m "feat(protopick): add history data layer — load/save/serialize/relativeTime"
```

---

### Task 2: Add history CSS styles

**Files:**
- Modify: `frontend/public/protopick/assets/editor.css`

- [ ] **Step 1: Append history styles at end of file**

```css
/* ── History button (panel header) ──────────────────────── */
.ai-editor-history-btn {
  position: relative;
  width: 24px;
  height: 24px;
  border: none;
  background: none;
  color: #a1a1aa;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  transition: color 100ms, background 100ms;
  padding: 0;
}

.ai-editor-history-btn:hover {
  color: #e4e4e7;
  background: rgba(255, 255, 255, 0.06);
}

.ai-editor-history-badge {
  position: absolute;
  top: -3px;
  right: -5px;
  background: #ef4444;
  color: #fff;
  font-size: 9px;
  font-weight: 700;
  min-width: 14px;
  height: 14px;
  padding: 0 3px;
  border-radius: 7px;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}

/* ── History dropdown ───────────────────────────────────── */
.ai-editor-history-dropdown {
  position: absolute;
  top: 36px;
  left: 0;
  right: 0;
  z-index: 2147483646;
  background: #0a0a0f;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-top: none;
  border-radius: 0 0 10px 10px;
  max-height: 400px;
  overflow-y: auto;
  padding: 6px;
}

.ai-editor-history-empty {
  text-align: center;
  padding: 20px 10px;
  color: #52525b;
  font-size: 12px;
}

.ai-editor-history-item {
  padding: 7px 8px;
  border-radius: 6px;
  cursor: default;
  margin-bottom: 3px;
  transition: background 100ms;
}

.ai-editor-history-item:hover {
  background: #141420;
}

.ai-editor-history-item-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 4px;
}

.ai-editor-history-summary {
  font-size: 12px;
  color: #e2e8f0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  flex: 1;
  min-width: 0;
  font-family: 'SF Mono', Menlo, Monaco, monospace;
}

.ai-editor-history-meta {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 2px;
  font-size: 10px;
  color: #94a3b8;
}

.ai-editor-history-actions {
  display: flex;
  gap: 3px;
  flex-shrink: 0;
}

.ai-editor-history-act-btn {
  border: none;
  border-radius: 3px;
  padding: 2px 6px;
  font-size: 10px;
  cursor: pointer;
  font-family: inherit;
  transition: background 100ms;
}

.ai-editor-history-copy-btn {
  background: rgba(99, 102, 241, 0.15);
  color: #818cf8;
}

.ai-editor-history-copy-btn:hover {
  background: rgba(99, 102, 241, 0.25);
}

.ai-editor-history-copy-btn.copied {
  background: rgba(74, 222, 128, 0.15);
  color: #4ade80;
}

.ai-editor-history-detail-btn {
  background: rgba(255, 255, 255, 0.05);
  color: #94a3b8;
}

.ai-editor-history-detail-btn:hover {
  background: rgba(255, 255, 255, 0.1);
  color: #d4d4d8;
}

/* ── History preview (hover) ────────────────────────────── */
.ai-editor-history-preview {
  background: #050507;
  border-radius: 4px;
  padding: 5px 8px;
  margin-top: 4px;
  font-size: 10px;
  color: #94a3b8;
  line-height: 1.6;
}

.ai-editor-history-preview-el {
  color: #e2e8f0;
}

.ai-editor-history-preview-note {
  color: #f59e0b;
  font-style: italic;
}

/* ── History detail (expanded) ──────────────────────────── */
.ai-editor-history-detail {
  background: #050507;
  border-radius: 4px;
  padding: 6px 8px;
  margin-top: 4px;
}

.ai-editor-history-detail-text {
  font-size: 10px;
  color: #94a3b8;
  font-family: 'SF Mono', Menlo, Monaco, monospace;
  line-height: 1.5;
  white-space: pre-wrap;
  word-break: break-all;
  max-height: 150px;
  overflow-y: auto;
}

.ai-editor-history-detail-actions {
  display: flex;
  gap: 4px;
  margin-top: 6px;
}

.ai-editor-history-detail-copy {
  border: none;
  border-radius: 4px;
  padding: 3px 10px;
  font-size: 10px;
  cursor: pointer;
  font-family: inherit;
  background: rgba(99, 102, 241, 0.15);
  color: #818cf8;
  transition: background 100ms;
}

.ai-editor-history-detail-copy:hover {
  background: rgba(99, 102, 241, 0.25);
}

.ai-editor-history-detail-copy.copied {
  background: rgba(74, 222, 128, 0.15);
  color: #4ade80;
}

.ai-editor-history-detail-delete {
  border: none;
  border-radius: 4px;
  padding: 3px 10px;
  font-size: 10px;
  cursor: pointer;
  font-family: inherit;
  background: rgba(255, 255, 255, 0.05);
  color: #71717a;
  transition: color 100ms, background 100ms;
}

.ai-editor-history-detail-delete:hover {
  color: #ef4444;
  background: rgba(239, 68, 68, 0.1);
}

/* ── History clear all ──────────────────────────────────── */
.ai-editor-history-footer {
  padding: 6px 8px 4px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
  margin-top: 4px;
}

.ai-editor-history-clear-all {
  border: none;
  background: none;
  color: #52525b;
  font-size: 11px;
  cursor: pointer;
  padding: 2px 4px;
  font-family: inherit;
  transition: color 100ms;
}

.ai-editor-history-clear-all:hover {
  color: #ef4444;
}

/* ── History confirm inline ─────────────────────────────── */
.ai-editor-history-confirm {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px 0;
  font-size: 11px;
  color: #94a3b8;
}

.ai-editor-history-confirm-yes {
  border: none;
  border-radius: 3px;
  padding: 2px 8px;
  font-size: 10px;
  cursor: pointer;
  font-family: inherit;
  background: rgba(239, 68, 68, 0.15);
  color: #ef4444;
}

.ai-editor-history-confirm-yes:hover {
  background: rgba(239, 68, 68, 0.25);
}

.ai-editor-history-confirm-no {
  border: none;
  border-radius: 3px;
  padding: 2px 8px;
  font-size: 10px;
  cursor: pointer;
  font-family: inherit;
  background: rgba(255, 255, 255, 0.05);
  color: #a1a1aa;
}

.ai-editor-history-confirm-no:hover {
  background: rgba(255, 255, 255, 0.1);
}
```

- [ ] **Step 2: Commit**

```bash
git add frontend/public/protopick/assets/editor.css
git commit -m "feat(protopick): add history UI CSS styles"
```

---

### Task 3: Add history button to panel header

**Files:**
- Modify: `frontend/public/protopick/assets/editor.js:664-729` (the `createChatPanel()` function)

Insert the history button SVG in the panel actions area, before the pause button. Wire up click handler. Call `updateHistoryBadge()` after panel creation.

- [ ] **Step 1: In `createChatPanel()`, insert history button before the pause button in the HTML template**

In the `chatPanel.innerHTML` template (line 667), find the `<div class="${NS}-panel-actions">` block (line 674). Add the history button before the pause button:

Change:
```html
        <div class="${NS}-panel-actions">
          <button class="${NS}-panel-btn ${NS}-panel-btn-pause" data-action="pause" title="暂停">
```

To:
```html
        <div class="${NS}-panel-actions">
          <button class="${NS}-history-btn" data-action="history" title="历史记录">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
            <span class="${NS}-history-badge" style="display:none;">0</span>
          </button>
          <button class="${NS}-panel-btn ${NS}-panel-btn-pause" data-action="pause" title="暂停">
```

- [ ] **Step 2: Add click handler for history button after existing button handlers (after line 726)**

After `chatPanel.querySelector('[data-action="pause"]').onclick = () => togglePause();` add:

```javascript
    chatPanel.querySelector('[data-action="history"]').onclick = (e) => {
      e.stopPropagation();
      toggleHistoryDropdown();
    };
```

- [ ] **Step 3: Add `updateHistoryBadge()` call at the end of `createChatPanel()`**

After `makeDraggable(chatPanel, chatPanel.querySelector(\`.${NS}-drag-handle\`));` add:

```javascript
    updateHistoryBadge();
```

- [ ] **Step 4: Commit**

```bash
git add frontend/public/protopick/assets/editor.js
git commit -m "feat(protopick): add history button to panel header"
```

---

### Task 4: Create history dropdown UI

**Files:**
- Modify: `frontend/public/protopick/assets/editor.js`

Add `toggleHistoryDropdown()`, `createHistoryDropdown()`, and all dropdown interaction logic.

- [ ] **Step 1: Add `toggleHistoryDropdown()` and `createHistoryDropdown()` functions**

Place these after `updateHistoryBadge()` (after the functions added in Task 1):

```javascript
  function toggleHistoryDropdown() {
    if (historyOpen && historyDropdown) {
      historyDropdown.remove();
      historyDropdown = null;
      historyOpen = false;
      expandedHistoryId = null;
      return;
    }
    historyOpen = true;
    expandedHistoryId = null;
    historyDropdown = createHistoryDropdown();
    chatPanel.appendChild(historyDropdown);

    // Close on outside click
    const closeOnOutside = (e) => {
      if (!historyDropdown) return;
      if (e.target.closest(`.${NS}-history-dropdown`) || e.target.closest('[data-action="history"]')) return;
      if (historyDropdown) {
        historyDropdown.remove();
        historyDropdown = null;
        historyOpen = false;
        expandedHistoryId = null;
      }
      document.removeEventListener("mousedown", closeOnOutside, true);
    };
    // Delay to avoid the current click
    setTimeout(() => {
      on(document, "mousedown", closeOnOutside, true);
    }, 0);
  }

  function createHistoryDropdown() {
    const dd = document.createElement("div");
    dd.className = `${NS}-root ${NS}-history-dropdown`;

    const history = loadHistory();

    if (history.length === 0) {
      dd.innerHTML = `<div class="${NS}-history-empty">暂无复制历史</div>`;
      return dd;
    }

    history.forEach(record => {
      const item = document.createElement("div");
      item.className = `${NS}-history-item`;
      item.dataset.id = record.id;

      // Main row
      const row = document.createElement("div");
      row.className = `${NS}-history-item-row`;

      const left = document.createElement("div");
      left.style.cssText = "flex:1;min-width:0;";

      const summary = document.createElement("div");
      summary.className = `${NS}-history-summary`;
      summary.textContent = record.summary || record.elements.map(e => e.selector.split(">").pop()).join(" + ");

      const meta = document.createElement("div");
      meta.className = `${NS}-history-meta`;
      const elCount = record.elements.length;
      const hasNotes = record.elements.some(e => e.annotation);
      let metaText = `${elCount} 个元素`;
      if (hasNotes) metaText += " · 含备注";
      metaText += ` · ${relativeTime(record.timestamp)}`;
      meta.textContent = metaText;

      left.appendChild(summary);
      left.appendChild(meta);

      const actions = document.createElement("div");
      actions.className = `${NS}-history-actions`;

      const copyBtn = document.createElement("button");
      copyBtn.className = `${NS}-history-act-btn ${NS}-history-copy-btn`;
      copyBtn.textContent = "复制";
      copyBtn.onclick = (e) => {
        e.stopPropagation();
        writeToClipboard(record.prompt);
        copyBtn.textContent = "Copied";
        copyBtn.classList.add("copied");
        setTimeout(() => {
          copyBtn.textContent = "复制";
          copyBtn.classList.remove("copied");
        }, 2000);
      };

      const detailBtn = document.createElement("button");
      detailBtn.className = `${NS}-history-act-btn ${NS}-history-detail-btn`;
      detailBtn.textContent = "详情";
      detailBtn.onclick = (e) => {
        e.stopPropagation();
        toggleDetailInDropdown(record.id, item, detailBtn);
      };

      actions.appendChild(copyBtn);
      actions.appendChild(detailBtn);

      row.appendChild(left);
      row.appendChild(actions);
      item.appendChild(row);

      // Hover preview (only when not expanded)
      item.addEventListener("mouseenter", () => {
        if (expandedHistoryId === record.id) return;
        removePreviewFromItem(item);
        const preview = document.createElement("div");
        preview.className = `${NS}-history-preview`;
        record.elements.forEach((el, i) => {
          const line = document.createElement("div");
          line.innerHTML = `<span class="${NS}-history-preview-el">${i + 1}. ${escapeHtml(el.selector.split(">").pop())}</span>${el.annotation ? ` <span class="${NS}-history-preview-note">— ${escapeHtml(el.annotation)}</span>` : ""}`;
          preview.appendChild(line);
        });
        item.appendChild(preview);
      });

      item.addEventListener("mouseleave", () => {
        if (expandedHistoryId === record.id) return;
        removePreviewFromItem(item);
      });

      dd.appendChild(item);
    });

    // Footer: clear all
    const footer = document.createElement("div");
    footer.className = `${NS}-history-footer`;
    const clearAllBtn = document.createElement("button");
    clearAllBtn.className = `${NS}-history-clear-all`;
    clearAllBtn.textContent = "清空全部";
    clearAllBtn.onclick = (e) => {
      e.stopPropagation();
      // Show inline confirm
      footer.innerHTML = "";
      const confirm = document.createElement("div");
      confirm.className = `${NS}-history-confirm`;
      confirm.textContent = "确认清空？";
      const yesBtn = document.createElement("button");
      yesBtn.className = `${NS}-history-confirm-yes`;
      yesBtn.textContent = "确认";
      yesBtn.onclick = (e2) => {
        e2.stopPropagation();
        clearAllHistory();
        // Rebuild dropdown
        if (historyDropdown) {
          historyDropdown.remove();
          historyDropdown = null;
        }
        historyOpen = false;
        toggleHistoryDropdown();
      };
      const noBtn = document.createElement("button");
      noBtn.className = `${NS}-history-confirm-no`;
      noBtn.textContent = "取消";
      noBtn.onclick = (e2) => {
        e2.stopPropagation();
        // Restore footer
        footer.innerHTML = "";
        footer.appendChild(clearAllBtn);
      };
      confirm.appendChild(yesBtn);
      confirm.appendChild(noBtn);
      footer.appendChild(confirm);
    };
    footer.appendChild(clearAllBtn);
    dd.appendChild(footer);

    return dd;
  }

  function removePreviewFromItem(item) {
    const preview = item.querySelector(`.${NS}-history-preview`);
    if (preview) preview.remove();
  }

  function toggleDetailInDropdown(recordId, itemEl, detailBtn) {
    // If this record is already expanded, collapse it
    if (expandedHistoryId === recordId) {
      const detail = itemEl.querySelector(`.${NS}-history-detail`);
      if (detail) detail.remove();
      expandedHistoryId = null;
      detailBtn.textContent = "详情";
      return;
    }

    // Collapse any previously expanded
    if (historyDropdown && expandedHistoryId) {
      const prevItem = historyDropdown.querySelector(`[data-id="${expandedHistoryId}"]`);
      if (prevItem) {
        const prevDetail = prevItem.querySelector(`.${NS}-history-detail`);
        if (prevDetail) prevDetail.remove();
        const prevBtn = prevItem.querySelector(`.${NS}-history-detail-btn`);
        if (prevBtn) prevBtn.textContent = "详情";
      }
      // Remove preview from previous if present
      if (prevItem) removePreviewFromItem(prevItem);
    }

    expandedHistoryId = recordId;

    // Remove hover preview if present
    removePreviewFromItem(itemEl);

    const history = loadHistory();
    const record = history.find(r => r.id === recordId);
    if (!record) return;

    const detail = document.createElement("div");
    detail.className = `${NS}-history-detail`;

    const text = document.createElement("div");
    text.className = `${NS}-history-detail-text`;
    text.textContent = record.prompt;

    const detailActions = document.createElement("div");
    detailActions.className = `${NS}-history-detail-actions`;

    const copyBtn = document.createElement("button");
    copyBtn.className = `${NS}-history-detail-copy`;
    copyBtn.textContent = "复制 Prompt";
    copyBtn.onclick = (e) => {
      e.stopPropagation();
      writeToClipboard(record.prompt);
      copyBtn.textContent = "Copied";
      copyBtn.classList.add("copied");
      setTimeout(() => {
        copyBtn.textContent = "复制 Prompt";
        copyBtn.classList.remove("copied");
      }, 2000);
    };

    const deleteBtn = document.createElement("button");
    deleteBtn.className = `${NS}-history-detail-delete`;
    deleteBtn.textContent = "删除";
    deleteBtn.onclick = (e) => {
      e.stopPropagation();
      deleteHistoryRecord(recordId);
      // Rebuild dropdown
      if (historyDropdown) {
        historyDropdown.remove();
        historyDropdown = null;
      }
      historyOpen = false;
      expandedHistoryId = null;
      toggleHistoryDropdown();
    };

    detailActions.appendChild(copyBtn);
    detailActions.appendChild(deleteBtn);

    detail.appendChild(text);
    detail.appendChild(detailActions);
    itemEl.appendChild(detail);

    detailBtn.textContent = "收起 ▲";
  }
```

- [ ] **Step 2: Commit**

```bash
git add frontend/public/protopick/assets/editor.js
git commit -m "feat(protopick): add history dropdown UI with preview, detail, delete"
```

---

### Task 5: Hook history saving into copy flow

**Files:**
- Modify: `frontend/public/protopick/assets/editor.js:958-1024` (`copyPrompt()`, `writeToClipboard()`, `fallbackCopy()`)

- [ ] **Step 1: Modify `copyPrompt()` to collect structured data and pass to clipboard/history**

Replace the existing `copyPrompt()` function (lines 958-963) with:

```javascript
  function copyPrompt() {
    const allElements = [...candidateElements];
    selectedElements.forEach(el => {
      if (!allElements.includes(el)) allElements.push(el);
    });

    const text = buildPromptText();
    if (!text) return;

    const serializedElements = allElements.map(el => serializeElementContext(el));
    writeToClipboard(text, serializedElements);
    showCopyFeedback("已复制");
  }
```

- [ ] **Step 2: Modify `writeToClipboard()` to accept elements and save history on success**

Replace the existing `writeToClipboard()` function (lines 1008-1014) with:

```javascript
  function writeToClipboard(text, serializedElements) {
    if (navigator.clipboard) {
      navigator.clipboard.writeText(text).then(() => {
        if (serializedElements) saveToHistory(text, serializedElements);
      }).catch(() => {
        fallbackCopy(text, serializedElements);
      });
    } else {
      fallbackCopy(text, serializedElements);
    }
  }
```

Note: History button's quick copy calls `writeToClipboard(record.prompt)` without elements — `serializedElements` will be `undefined`, so `saveToHistory` won't be called (correct: re-copying old history should not create a new record).

- [ ] **Step 3: Modify `fallbackCopy()` to accept elements and save history**

Replace the existing `fallbackCopy()` function (lines 1016-1024) with:

```javascript
  function fallbackCopy(text, serializedElements) {
    const ta = document.createElement("textarea");
    ta.value = text;
    ta.style.cssText = "position:fixed;opacity:0;top:0;left:0";
    document.body.appendChild(ta);
    ta.focus(); ta.select();
    try {
      document.execCommand("copy");
      if (serializedElements) saveToHistory(text, serializedElements);
    } catch (_) {}
    ta.remove();
  }
```

- [ ] **Step 4: Commit**

```bash
git add frontend/public/protopick/assets/editor.js
git commit -m "feat(protopick): hook history saving into copy flow"
```

---

### Task 6: Update Esc key handler

**Files:**
- Modify: `frontend/public/protopick/assets/editor.js:571-576` (the `if (e.key === "Escape")` block in `handleKeyDown()`)

- [ ] **Step 1: Add history dropdown close as the highest Esc priority**

In `handleKeyDown()`, change the Escape block (lines 571-576):

From:
```javascript
    if (e.key === "Escape") {
      if (activePopover) { removeAnnotationPopover(); }
      else if (selectedElements.length > 0) { clearSelection(); updateTags(); }
      else if (candidateElements.length > 0) { clearAllCandidates(); }
      return;
    }
```

To:
```javascript
    if (e.key === "Escape") {
      if (historyOpen) { toggleHistoryDropdown(); }
      else if (activePopover) { removeAnnotationPopover(); }
      else if (selectedElements.length > 0) { clearSelection(); updateTags(); }
      else if (candidateElements.length > 0) { clearAllCandidates(); }
      return;
    }
```

- [ ] **Step 2: Commit**

```bash
git add frontend/public/protopick/assets/editor.js
git commit -m "feat(protopick): add history dropdown to Esc priority chain"
```

---

### Task 7: Update destroy() cleanup

**Files:**
- Modify: `frontend/public/protopick/assets/editor.js:72-89` (the `destroy()` function)

- [ ] **Step 1: Add history cleanup to destroy()**

In the `destroy()` function, before `delete window.__selectorDestroy;` (line 88), add:

```javascript
    historyDropdown = null;
    historyOpen = false;
    expandedHistoryId = null;
```

- [ ] **Step 2: Commit**

```bash
git add frontend/public/protopick/assets/editor.js
git commit -m "feat(protopick): add history cleanup to destroy"
```

---

### Task 8: Build and manual test

- [ ] **Step 1: Verify the build still works**

ProtoPick is a static asset (no build step), but verify the host app still builds:

```bash
cd frontend/protohub && npm run build
```

Expected: Build succeeds with no errors.

- [ ] **Step 2: Manual test — open the ProtoPick page**

Open `http://localhost:3000/protopick/` in a browser. Install the bookmarklet on a test page.

- [ ] **Step 3: Manual test — verify history button and badge**

1. After panel loads, confirm the clock icon appears in the header (before pause button)
2. Badge should be hidden (no history yet)
3. Click history button → dropdown shows "暂无复制历史"

- [ ] **Step 4: Manual test — verify copy saves history**

1. Select an element on the page
2. Add it to candidates
3. Click "Copy Prompt" or Cmd/Ctrl+C
4. Badge should appear with "1"
5. Click history button → dropdown shows the record with summary and relative time

- [ ] **Step 5: Manual test — verify hover preview**

1. Open history dropdown
2. Hover over a record → preview appears showing element selectors and annotations

- [ ] **Step 6: Manual test — verify detail expand**

1. Click "详情" → record expands showing full Prompt text
2. Click "收起 ▲" → collapses back
3. Verify accordion: expand one record, expand another — first one collapses

- [ ] **Step 7: Manual test — verify copy from history**

1. Click "复制" on a history record
2. Button shows "Copied" for 2 seconds
3. Paste into a text editor → shows the original Prompt

- [ ] **Step 8: Manual test — verify delete and clear**

1. Create 2+ history records
2. Expand detail → click "删除" → record removed, dropdown refreshes
3. Click "清空全部" → inline confirm appears
4. Click "确认" → all records cleared, badge hidden

- [ ] **Step 9: Manual test — verify Esc closes dropdown**

1. Open history dropdown
2. Press Esc → dropdown closes
3. Press Esc again → existing behavior (close popover / clear selection / clear candidates)

- [ ] **Step 10: Manual test — verify persistence**

1. Create a history record
2. Close ProtoPick (click X)
3. Re-inject ProtoPick on the same page
4. Badge should still show the count
5. Open dropdown → previous records still there

- [ ] **Step 11: Manual test — verify dedup**

1. Copy the same prompt twice in a row
2. History should only have 1 record (not 2 identical ones)

- [ ] **Step 12: Final commit if any fixes were needed**

```bash
git add -A
git commit -m "fix(protopick): fix issues found during manual testing"
```

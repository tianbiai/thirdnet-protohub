/**
 * ProtoPick — visual element picker with per-element annotations.
 * Inject via bookmarklet. Click = select.
 */
(function () {
  "use strict";

  if (document.querySelector(".ai-editor-root")) return;

  const NS = "ai-editor";
  const AI_ID = "data-ai-id";
  const MAX_HTML_LENGTH = 2000;

  let selectedElements = [];
  let candidateElements = [];
  let chatPanel = null;
  let hoverBox = null;
  let aiIdCounter = 0;
  let rafPending = false;
  let lastMoveTarget = null;
  const selOverlays = new Map();
  const annotations = new Map();
  const listeners = [];
  let activePopover = null;
  const capturedErrors = [];
  let originalConsoleError = null;
  let errorsVisible = false;
  let errorToastTimer = null;
  let paused = false;
  let historyDropdown = null;
  let historyOpen = false;
  let expandedHistoryId = null;

  function on(target, type, fn, capture) {
    target.addEventListener(type, fn, capture);
    listeners.push({ target, type, fn, capture });
  }

  const SELECTION_EVENTS = new Set(["mousedown", "click", "mousemove"]);

  function toggleSelectionListeners(attach) {
    for (const { target, type, fn, capture } of listeners) {
      if (SELECTION_EVENTS.has(type)) {
        if (attach) target.addEventListener(type, fn, capture);
        else target.removeEventListener(type, fn, capture);
      }
    }
  }

  // ── Init ───────────────────────────────────────────────────
  function init() {
    assignAiIds(document.body);
    createHoverBox();
    createChatPanel();
    startErrorCapture();

    on(document, "mousedown", handleMouseDown, true);
    on(document, "click", handleClick, true);
    on(document, "mousemove", handleMouseMove, true);
    on(document, "mouseleave", () => { showHover(null); }, true);
    on(document, "keydown", handleKeyDown, true);

    let repositionRaf = false;
    const scheduleReposition = () => {
      if (!repositionRaf) {
        repositionRaf = true;
        requestAnimationFrame(() => { positionAllOverlays(); repositionRaf = false; });
      }
    };
    on(window, "scroll", scheduleReposition, true);
    on(window, "resize", scheduleReposition, false);
  }

  // ── Destroy ────────────────────────────────────────────────
  function destroy() {
    stopErrorCapture();
    for (const { target, type, fn, capture } of listeners) {
      target.removeEventListener(type, fn, capture);
    }
    destroyAllOverlays();
    removeAnnotationPopover();
    if (hoverBox) hoverBox.remove();
    if (chatPanel) chatPanel.remove();
    const toast = document.querySelector(`.${NS}-error-toast`);
    if (toast) toast.remove();
    if (panelDragListeners) {
      document.removeEventListener("mousemove", panelDragListeners.move);
      document.removeEventListener("mouseup", panelDragListeners.up);
      panelDragListeners = null;
    }
    historyDropdown = null;
    historyOpen = false;
    expandedHistoryId = null;
    delete window.__selectorDestroy;
  }

  // ── Error capture ──────────────────────────────────────────
  function addError(message, stack) {
    if (capturedErrors.some(e => e.message === message)) return;
    capturedErrors.push({ message, stack: stack || "", time: Date.now() });
    if (capturedErrors.length > 20) capturedErrors.shift();
    updateErrorBadge();
    updateCopyButton();
    showErrorToast(capturedErrors.length);
  }

  function positionToast(toast) {
    if (!chatPanel) return;
    const pr = chatPanel.getBoundingClientRect();
    toast.style.left = (pr.left + pr.width / 2) + "px";
    toast.style.top = (pr.top - 8) + "px";
  }

  function showErrorToast(count) {
    let toast = document.querySelector(`.${NS}-error-toast`);
    if (!toast) {
      toast = document.createElement("div");
      toast.className = `${NS}-root ${NS}-error-toast`;
      document.body.appendChild(toast);
    }
    toast.textContent = `已捕获 ${count} 个错误`;
    positionToast(toast);
    toast.classList.add(`${NS}-error-toast-show`);
    if (errorToastTimer) clearTimeout(errorToastTimer);
    errorToastTimer = setTimeout(() => {
      toast.classList.remove(`${NS}-error-toast-show`);
    }, 3000);
  }

  function startErrorCapture() {
    originalConsoleError = console.error;
    console.error = function () {
      const args = Array.from(arguments);
      const msg = args.map(a => {
        if (a instanceof Error) return `${a.message}\n${a.stack || ""}`;
        if (typeof a === "object") try { return JSON.stringify(a); } catch (_) { return String(a); }
        return String(a);
      }).join(" ");
      const lines = msg.split("\n");
      const firstLine = lines[0];
      const rest = lines.slice(1).join("\n");
      addError(firstLine, rest);
      originalConsoleError.apply(console, args);
    };

    const onError = (e) => {
      const msg = e.error ? `${e.error.name}: ${e.error.message}` : e.message;
      const stack = e.error ? e.error.stack : "";
      addError(msg, stack);
    };
    const onRejection = (e) => {
      const reason = e.reason;
      const msg = reason instanceof Error ? `${reason.name}: ${reason.message}` : String(reason);
      const stack = reason instanceof Error ? reason.stack : "";
      addError("Unhandled: " + msg, stack);
    };

    on(window, "error", onError, true);
    on(window, "unhandledrejection", onRejection, true);
  }

  function stopErrorCapture() {
    if (originalConsoleError) {
      console.error = originalConsoleError;
      originalConsoleError = null;
    }
    capturedErrors.length = 0;
  }

  function repositionToast() {
    const toast = document.querySelector(`.${NS}-error-toast`);
    if (toast) positionToast(toast);
  }

  function clearErrors() {
    capturedErrors.length = 0;
    errorsVisible = false;
    updateErrorBadge();
    updateErrorList();
    updateCopyButton();
  }

  // ── AI-ID ──────────────────────────────────────────────────
  function assignAiIds(root) {
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_ELEMENT);
    let node;
    while ((node = walker.nextNode())) {
      if (isEditorElement(node)) continue;
      if (!node.hasAttribute(AI_ID)) node.setAttribute(AI_ID, `el-${aiIdCounter++}`);
    }
  }

  function isEditorElement(el) {
    return el && el.closest && !!el.closest(`.${NS}-root`);
  }

  function byAiId(id) {
    return document.querySelector(`[${AI_ID}="${id}"]`);
  }

  // ── Resolve target ─────────────────────────────────────────
  function resolveTarget(el) {
    let cur = el;
    while (cur && cur !== document.body && cur !== document.documentElement) {
      if (isEditorElement(cur)) { cur = cur.parentElement; continue; }
      if (!isVisible(cur)) { cur = cur.parentElement; continue; }
      if (isMeaningful(cur)) return cur;
      cur = cur.parentElement;
    }
    return el;
  }

  function isVisible(el) {
    const r = el.getBoundingClientRect();
    if (r.width < 2 && r.height < 2) return false;
    const s = getComputedStyle(el);
    if (s.display === "none" || s.visibility === "hidden" || s.opacity === "0") return false;
    let ancestor = el.parentElement;
    while (ancestor && ancestor !== document.body) {
      const as = getComputedStyle(ancestor);
      if (as.display === "none" || as.visibility === "hidden" || as.opacity === "0") return false;
      ancestor = ancestor.parentElement;
    }
    return true;
  }

  function isMeaningful(el) {
    if (hasDirectText(el)) return true;
    if (el.querySelector("img,video,canvas,svg,button,a,input,select,textarea,iframe")) return true;
    if (el.children.length > 1) return true;
    return false;
  }

  function hasDirectText(el) {
    for (const n of el.childNodes) {
      if (n.nodeType === 3 && n.textContent.trim()) return true;
    }
    return false;
  }

  // ── Hover overlay ──────────────────────────────────────────
  function createHoverBox() {
    hoverBox = document.createElement("div");
    hoverBox.className = `${NS}-hover-box`;
    document.body.appendChild(hoverBox);
  }

  function showHover(el) {
    if (!el || isEditorElement(el) || selectedElements.includes(el)) {
      hoverBox.style.opacity = "0";
      return;
    }
    const r = el.getBoundingClientRect();
    hoverBox.style.top = (r.top - 1) + "px";
    hoverBox.style.left = (r.left - 1) + "px";
    hoverBox.style.width = (r.width + 2) + "px";
    hoverBox.style.height = (r.height + 2) + "px";
    hoverBox.style.opacity = "1";
  }

  // ── Mouse handling ─────────────────────────────────────────
  function handleMouseMove(e) {
    lastMoveTarget = e.target;
    if (!rafPending) {
      rafPending = true;
      requestAnimationFrame(() => { showHover(resolveTarget(lastMoveTarget)); rafPending = false; });
    }
  }

  function handleMouseDown(e) {
    if (isEditorElement(e.target)) return;
    if (e.button !== 0) return;
    e.preventDefault();
  }

  function handleClick(e) {
    if (isEditorElement(e.target)) return;

    e.preventDefault();
    e.stopPropagation();
    removeAnnotationPopover();
    const sel = window.getSelection();
    if (sel) sel.removeAllRanges();

    const el = resolveTarget(e.target);
    clearSelection();
    addSelection(el);
    updateTags();
  }

  // ── Selection overlays ─────────────────────────────────────
  function createSelOverlay(el) {
    const aiId = el.getAttribute(AI_ID);
    if (selOverlays.has(aiId)) return;

    const box = document.createElement("div");
    box.className = `${NS}-sel-box`;

    const corners = [0, 1, 2, 3].map((i) => {
      const c = document.createElement("div");
      c.className = `${NS}-sel-corner`;
      c.style.animationDelay = `${i * 28}ms`;
      document.body.appendChild(c);
      return c;
    });

    const label = document.createElement("div");
    label.className = `${NS}-sel-label`;
    label.textContent = elementLabel(el);

    const annotateBtn = document.createElement("button");
    annotateBtn.className = `${NS}-root ${NS}-annotate-btn`;
    annotateBtn.title = "添加批注";
    annotateBtn.innerHTML = '<svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M12 20h9"/><path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4Z"/></svg>';
    annotateBtn.onclick = (e) => {
      e.stopPropagation();
      e.preventDefault();
      showAnnotationPopover(el, annotateBtn);
    };

    const addBtn = document.createElement("button");
    addBtn.className = `${NS}-root ${NS}-add-btn`;
    addBtn.title = "加入收集区";
    addBtn.innerHTML = '<svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>';
    addBtn.onclick = (e) => {
      e.stopPropagation();
      e.preventDefault();
      addToCandidates(el);
    };

    document.body.appendChild(box);
    document.body.appendChild(label);
    document.body.appendChild(annotateBtn);
    document.body.appendChild(addBtn);
    selOverlays.set(aiId, { box, corners, label, annotateBtn, addBtn });
    positionSelOverlay(el);
  }

  function positionSelOverlay(el) {
    const aiId = el.getAttribute(AI_ID);
    const ov = selOverlays.get(aiId);
    if (!ov) return;
    const r = el.getBoundingClientRect();
    const pad = 2;

    ov.box.style.top = (r.top - pad) + "px";
    ov.box.style.left = (r.left - pad) + "px";
    ov.box.style.width = (r.width + pad * 2) + "px";
    ov.box.style.height = (r.height + pad * 2) + "px";

    const cs = 6;
    const pos = [
      { top: r.top - pad - cs / 2,    left: r.left - pad - cs / 2 },
      { top: r.top - pad - cs / 2,    left: r.right + pad - cs / 2 },
      { top: r.bottom + pad - cs / 2, left: r.left - pad - cs / 2 },
      { top: r.bottom + pad - cs / 2, left: r.right + pad - cs / 2 },
    ];
    for (let i = 0; i < 4; i++) {
      ov.corners[i].style.top = pos[i].top + "px";
      ov.corners[i].style.left = pos[i].left + "px";
    }

    ov.label.style.top = (r.top - pad - 24) + "px";
    ov.label.style.left = (r.left - pad) + "px";

    ov.annotateBtn.style.top = (r.top - pad - 26) + "px";
    ov.annotateBtn.style.left = (r.right + pad + 4) + "px";

    ov.addBtn.style.top = (r.top - pad - 26) + "px";
    ov.addBtn.style.left = (r.right + pad + 30) + "px";

    if (annotations.has(aiId)) {
      ov.annotateBtn.classList.add(`${NS}-has-note`);
    } else {
      ov.annotateBtn.classList.remove(`${NS}-has-note`);
    }
  }

  function positionAllOverlays() {
    for (const el of selectedElements) positionSelOverlay(el);
  }

  function destroySelOverlay(aiId) {
    const ov = selOverlays.get(aiId);
    if (!ov) return;
    ov.box.remove();
    ov.corners.forEach(c => c.remove());
    ov.label.remove();
    ov.annotateBtn.remove();
    ov.addBtn.remove();
    selOverlays.delete(aiId);
  }

  function destroyAllOverlays() {
    for (const [aiId] of selOverlays) destroySelOverlay(aiId);
  }

  function addSelection(el) {
    if (!selectedElements.includes(el)) {
      selectedElements.push(el);
      createSelOverlay(el);
    }
  }

  function removeSelection(el) {
    const idx = selectedElements.indexOf(el);
    if (idx >= 0) {
      selectedElements.splice(idx, 1);
      const aiId = el.getAttribute(AI_ID);
      destroySelOverlay(aiId);
    }
  }

  function clearSelection() {
    destroyAllOverlays();
    selectedElements = [];
    removeAnnotationPopover();
  }

  // ── Candidate management ───────────────────────────────────
  function saveActivePopover() {
    if (!activePopover) return;
    const textarea = activePopover.querySelector(`.${NS}-annotate-input`);
    if (!textarea) return;
    const aiId = textarea.closest(`.${NS}-annotate-popover`).dataset.aiId;
    if (!aiId) return;
    const val = textarea.value.trim();
    if (val) annotations.set(aiId, val);
    else annotations.delete(aiId);
  }

  function addToCandidates(el) {
    if (candidateElements.includes(el)) return;
    saveActivePopover();
    candidateElements.push(el);
    clearSelection();
    updateTags();
  }

  function removeFromCandidates(el) {
    const idx = candidateElements.indexOf(el);
    if (idx >= 0) {
      candidateElements.splice(idx, 1);
      const aiId = el.getAttribute(AI_ID);
      annotations.delete(aiId);
      updateTags();
    }
  }

  function clearAllCandidates() {
    candidateElements.forEach(el => {
      const aiId = el.getAttribute(AI_ID);
      annotations.delete(aiId);
    });
    candidateElements = [];
    updateTags();
  }

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

  function buildSummary(domElements) {
    return domElements.slice(0, 2).map(el => elementLabel(el)).join(" + ");
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

  function saveToHistory(promptText, serializedElements, domElements) {
    try {
      const history = loadHistory();
      if (history.length > 0 && history[0].prompt === promptText) return;
      const now = Date.now();
      const record = {
        id: now.toString(36) + "-" + Math.random().toString(16).slice(2, 6),
        timestamp: now,
        pagePath: window.location.pathname,
        pageTitle: document.title,
        elements: serializedElements,
        prompt: promptText,
        summary: buildSummary(domElements)
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
    historyDropdown = createHistoryModal();

    const closeOnOutside = (e) => {
      if (!historyDropdown) {
        document.removeEventListener("mousedown", closeOnOutside, true);
        return;
      }
      if (e.target.closest(`.${NS}-history-modal-overlay`) || e.target.closest('[data-action="history"]')) return;
      historyDropdown.remove();
      historyDropdown = null;
      historyOpen = false;
      expandedHistoryId = null;
      document.removeEventListener("mousedown", closeOnOutside, true);
    };
    setTimeout(() => {
      document.addEventListener("mousedown", closeOnOutside, true);
    }, 0);
  }

  function formatFullTime(ts) {
    const d = new Date(ts);
    const pad = (n) => String(n).padStart(2, "0");
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }

  function createHistoryModal() {
    const overlay = document.createElement("div");
    overlay.className = `${NS}-root ${NS}-history-modal-overlay`;

    const modal = document.createElement("div");
    modal.className = `${NS}-history-list-modal`;

    // Header
    const header = document.createElement("div");
    header.className = `${NS}-history-list-header`;
    const title = document.createElement("span");
    title.className = `${NS}-history-list-title`;
    title.textContent = "历史记录";
    const closeBtn = document.createElement("button");
    closeBtn.className = `${NS}-panel-btn`;
    closeBtn.innerHTML = '<svg width="10" height="10" viewBox="0 0 10 10" fill="none"><line x1="1" y1="1" x2="9" y2="9" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/><line x1="9" y1="1" x2="1" y2="9" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg>';
    closeBtn.title = "关闭";
    closeBtn.onclick = (e) => { e.stopPropagation(); toggleHistoryDropdown(); };
    header.appendChild(title);
    header.appendChild(closeBtn);

    const list = document.createElement("div");
    list.className = `${NS}-history-list`;

    const history = loadHistory();
    if (history.length === 0) {
      list.innerHTML = `<div class="${NS}-history-empty">暂无复制历史</div>`;
    } else {
      history.forEach(record => {
        const item = document.createElement("div");
        item.className = `${NS}-history-item`;
        item.dataset.id = record.id;

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

        const timeRow = document.createElement("div");
        timeRow.className = `${NS}-history-time`;
        timeRow.textContent = formatFullTime(record.timestamp);

        left.appendChild(summary);
        left.appendChild(meta);
        left.appendChild(timeRow);

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
          setTimeout(() => { copyBtn.textContent = "复制"; copyBtn.classList.remove("copied"); }, 2000);
        };

        const detailBtn = document.createElement("button");
        detailBtn.className = `${NS}-history-act-btn ${NS}-history-detail-btn`;
        detailBtn.textContent = "详情";
        detailBtn.onclick = (e) => {
          e.stopPropagation();
          showHistoryDetailModal(record.id);
        };

        actions.appendChild(copyBtn);
        actions.appendChild(detailBtn);

        row.appendChild(left);
        row.appendChild(actions);
        item.appendChild(row);

        // Hover preview
        item.addEventListener("mouseenter", () => {
          if (expandedHistoryId) return;
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
          if (expandedHistoryId) return;
          removePreviewFromItem(item);
        });

        list.appendChild(item);
      });
    }

    // Footer
    const footer = document.createElement("div");
    footer.className = `${NS}-history-footer`;
    if (history.length > 0) {
      const clearAllBtn = document.createElement("button");
      clearAllBtn.className = `${NS}-history-clear-all`;
      clearAllBtn.textContent = "清空全部";
      clearAllBtn.onclick = (e) => {
        e.stopPropagation();
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
          toggleHistoryDropdown();
        };
        const noBtn = document.createElement("button");
        noBtn.className = `${NS}-history-confirm-no`;
        noBtn.textContent = "取消";
        noBtn.onclick = (e2) => {
          e2.stopPropagation();
          footer.innerHTML = "";
          footer.appendChild(clearAllBtn);
        };
        confirm.appendChild(yesBtn);
        confirm.appendChild(noBtn);
        footer.appendChild(confirm);
      };
      footer.appendChild(clearAllBtn);
    }

    modal.appendChild(header);
    modal.appendChild(list);
    modal.appendChild(footer);
    overlay.appendChild(modal);

    overlay.onclick = (e) => { if (e.target === overlay) toggleHistoryDropdown(); };

    return overlay;
  }

  function removePreviewFromItem(item) {
    const preview = item.querySelector(`.${NS}-history-preview`);
    if (preview) preview.remove();
  }

  function showHistoryDetailModal(recordId) {
    closeHistoryDetailModal();
    const history = loadHistory();
    const record = history.find(r => r.id === recordId);
    if (!record) return;

    const overlay = document.createElement("div");
    overlay.className = `${NS}-root ${NS}-history-modal-overlay`;

    const modal = document.createElement("div");
    modal.className = `${NS}-history-modal`;

    // Header
    const header = document.createElement("div");
    header.className = `${NS}-history-modal-header`;
    const title = document.createElement("div");
    title.className = `${NS}-history-modal-title`;
    title.textContent = record.summary || "历史记录详情";
    const timeSpan = document.createElement("span");
    timeSpan.className = `${NS}-history-modal-time`;
    timeSpan.textContent = `${relativeTime(record.timestamp)} · ${record.pagePath}`;
    const headerLeft = document.createElement("div");
    headerLeft.appendChild(title);
    headerLeft.appendChild(timeSpan);
    const closeBtn = document.createElement("button");
    closeBtn.className = `${NS}-panel-btn`;
    closeBtn.innerHTML = '<svg width="10" height="10" viewBox="0 0 10 10" fill="none"><line x1="1" y1="1" x2="9" y2="9" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/><line x1="9" y1="1" x2="1" y2="9" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg>';
    closeBtn.title = "关闭";
    closeBtn.onclick = (e) => { e.stopPropagation(); closeHistoryDetailModal(); };
    header.appendChild(headerLeft);
    header.appendChild(closeBtn);

    // Element list
    const elSection = document.createElement("div");
    elSection.className = `${NS}-history-modal-elements`;
    record.elements.forEach((el, i) => {
      const elRow = document.createElement("div");
      elRow.className = `${NS}-history-modal-el`;
      const elLabel = document.createElement("div");
      elLabel.className = `${NS}-history-modal-el-label`;
      elLabel.innerHTML = `<span class="${NS}-history-modal-el-num">${i + 1}</span> <span class="${NS}-history-modal-el-tag">&lt;${escapeHtml(el.tag)}&gt;</span> <span class="${NS}-history-modal-el-selector">${escapeHtml(el.selector)}</span>`;
      if (el.component) {
        elLabel.innerHTML += ` <span class="${NS}-history-modal-el-component">${escapeHtml(el.component)}</span>`;
      }
      elRow.appendChild(elLabel);
      if (el.source) {
        const src = document.createElement("div");
        src.className = `${NS}-history-modal-el-source`;
        src.textContent = el.source;
        elRow.appendChild(src);
      }
      if (el.annotation) {
        const note = document.createElement("div");
        note.className = `${NS}-history-modal-el-note`;
        note.textContent = el.annotation;
        elRow.appendChild(note);
      }
      elSection.appendChild(elRow);
    });

    // Prompt text
    const promptSection = document.createElement("div");
    promptSection.className = `${NS}-history-modal-prompt`;
    const promptLabel = document.createElement("div");
    promptLabel.className = `${NS}-history-modal-prompt-label`;
    promptLabel.textContent = "Prompt";
    const promptText = document.createElement("div");
    promptText.className = `${NS}-history-modal-prompt-text`;
    promptText.textContent = record.prompt;
    promptSection.appendChild(promptLabel);
    promptSection.appendChild(promptText);

    // Actions
    const actions = document.createElement("div");
    actions.className = `${NS}-history-modal-actions`;
    const copyBtn = document.createElement("button");
    copyBtn.className = `${NS}-history-modal-copy`;
    copyBtn.textContent = "复制 Prompt";
    copyBtn.onclick = (e) => {
      e.stopPropagation();
      writeToClipboard(record.prompt);
      copyBtn.textContent = "Copied";
      copyBtn.classList.add("copied");
      setTimeout(() => { copyBtn.textContent = "复制 Prompt"; copyBtn.classList.remove("copied"); }, 2000);
    };
    const deleteBtn = document.createElement("button");
    deleteBtn.className = `${NS}-history-modal-delete`;
    deleteBtn.textContent = "删除";
    deleteBtn.onclick = (e) => {
      e.stopPropagation();
      deleteHistoryRecord(recordId);
      closeHistoryDetailModal();
      if (historyDropdown) { historyDropdown.remove(); historyDropdown = null; }
      historyOpen = false;
      expandedHistoryId = null;
      toggleHistoryDropdown();
    };
    actions.appendChild(copyBtn);
    actions.appendChild(deleteBtn);

    modal.appendChild(header);
    modal.appendChild(elSection);
    modal.appendChild(promptSection);
    modal.appendChild(actions);
    overlay.appendChild(modal);

    overlay.onclick = (e) => { if (e.target === overlay) closeHistoryDetailModal(); };
    document.body.appendChild(overlay);
    expandedHistoryId = recordId;
  }

  function closeHistoryDetailModal() {
    const existing = document.querySelector(`.${NS}-history-modal-overlay`);
    if (existing) existing.remove();
    expandedHistoryId = null;
  }

  const PLAY_ICON = '<svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polygon points="5 3 19 12 5 21 5 3"/></svg>';
  const PAUSE_ICON = '<svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><rect x="6" y="4" width="4" height="16"/><rect x="14" y="4" width="4" height="16"/></svg>';

  function togglePause() {
    paused = !paused;
    const dot = chatPanel.querySelector(`.${NS}-status-dot`);
    const label = chatPanel.querySelector(`.${NS}-status-label`);
    const btn = chatPanel.querySelector('[data-action="pause"]');

    if (paused) {
      toggleSelectionListeners(false);
      showHover(null);
      lastMoveTarget = null;
      rafPending = false;
      dot.classList.add(`${NS}-status-dot-paused`);
      label.textContent = "已暂停";
      btn.innerHTML = PLAY_ICON;
      btn.title = "继续";
    } else {
      toggleSelectionListeners(true);
      dot.classList.remove(`${NS}-status-dot-paused`);
      label.textContent = "选取中";
      btn.innerHTML = PAUSE_ICON;
      btn.title = "暂停";
    }
  }

  function flashBoundary(el) {
    el.classList.remove(NS + "-flash-boundary");
    void el.offsetWidth;
    el.classList.add(NS + "-flash-boundary");
    setTimeout(function () { el.classList.remove(NS + "-flash-boundary"); }, 250);
  }

  let navRafPending = false;
  let navRafKey = null;

  function navigateDOM(key) {
    const hasSelection = selectedElements.length > 0;
    const current = hasSelection ? selectedElements[selectedElements.length - 1]
                 : lastMoveTarget ? resolveTarget(lastMoveTarget) : null;
    if (!current) return;

    let target = null;

    if (key === "ArrowUp") {
      let parent = current.parentElement;
      while (parent && parent !== document.body && parent !== document.documentElement) {
        if (!isEditorElement(parent) && isVisible(parent)) { target = parent; break; }
        parent = parent.parentElement;
      }
    } else if (key === "ArrowDown") {
      const children = current.children;
      for (let i = 0; i < children.length; i++) {
        if (!isEditorElement(children[i]) && isVisible(children[i]) && isMeaningful(children[i])) {
          target = children[i];
          break;
        }
      }
    } else if (key === "ArrowLeft") {
      let prev = current.previousElementSibling;
      while (prev) {
        if (!isEditorElement(prev) && isVisible(prev) && isMeaningful(prev)) { target = prev; break; }
        prev = prev.previousElementSibling;
      }
    } else if (key === "ArrowRight") {
      let next = current.nextElementSibling;
      while (next) {
        if (!isEditorElement(next) && isVisible(next) && isMeaningful(next)) { target = next; break; }
        next = next.nextElementSibling;
      }
    }

    if (target) {
      lastMoveTarget = target;
      if (hasSelection) {
        saveActivePopover();
        clearSelection();
        addSelection(target);
        updateTags();
        showHover(null);
      } else {
        showHover(target);
      }
    } else {
      if (hasSelection) {
        const aiId = current.getAttribute(AI_ID);
        const ov = aiId && selOverlays.get(aiId);
        if (ov && ov.box) flashBoundary(ov.box);
      } else {
        showHover(current);
        lastMoveTarget = current;
        flashBoundary(hoverBox);
      }
    }
  }

  function handleKeyDown(e) {
    if (e.target.isContentEditable) return;
    if (isEditorElement(e.target) && (e.target.tagName === "INPUT" || e.target.tagName === "TEXTAREA")) return;
    const mod = e.metaKey || e.ctrlKey;

    // Arrow key DOM navigation (rAF-throttled)
    if (e.key === "ArrowUp" || e.key === "ArrowDown" || e.key === "ArrowLeft" || e.key === "ArrowRight") {
      if (paused) return;
      if (e.target.tagName === "INPUT" || e.target.tagName === "TEXTAREA" || e.target.tagName === "SELECT") return;
      e.preventDefault();
      navRafKey = e.key;
      if (!navRafPending) {
        navRafPending = true;
        requestAnimationFrame(function () {
          navigateDOM(navRafKey);
          navRafPending = false;
        });
      }
      return;
    }

    if (e.key === "Escape") {
      if (expandedHistoryId) { closeHistoryDetailModal(); }
      else if (historyOpen) { toggleHistoryDropdown(); }
      else if (activePopover) { removeAnnotationPopover(); }
      else if (selectedElements.length > 0) { clearSelection(); updateTags(); }
      else if (candidateElements.length > 0) { clearAllCandidates(); }
      return;
    }
    if (mod && e.key === "Enter" && selectedElements.length > 0) {
      e.preventDefault();
      addToCandidates(selectedElements[0]);
      return;
    }
    if (e.key === "P" && e.shiftKey && !e.metaKey && !e.ctrlKey) {
      e.preventDefault();
      togglePause();
      return;
    }
    if (mod && e.key.toLowerCase() === "c" && !e.shiftKey && (candidateElements.length > 0 || selectedElements.length > 0 || capturedErrors.length > 0)) {
      e.preventDefault();
      copyPrompt();
      return;
    }
  }

  // ── Annotation popover ─────────────────────────────────────
  function showAnnotationPopover(el, btn) {
    if (!el.isConnected) return;
    removeAnnotationPopover();

    const aiId = el.getAttribute(AI_ID);
    const popover = document.createElement("div");
    popover.className = `${NS}-root ${NS}-annotate-popover`;
    popover.dataset.aiId = aiId;

    const textarea = document.createElement("textarea");
    textarea.className = `${NS}-annotate-input`;
    textarea.value = annotations.get(aiId) || "";
    textarea.placeholder = "输入修改要求…";
    textarea.rows = 2;

    const actions = document.createElement("div");
    actions.className = `${NS}-annotate-actions`;

    const clearNoteBtn = document.createElement("button");
    clearNoteBtn.className = `${NS}-annotate-clear`;
    clearNoteBtn.textContent = "清除";

    const doneBtn = document.createElement("button");
    doneBtn.className = `${NS}-annotate-done`;
    doneBtn.textContent = "完成";

    const save = () => {
      const val = textarea.value.trim();
      if (val) annotations.set(aiId, val);
      else annotations.delete(aiId);
      removeAnnotationPopover();
      if (selOverlays.has(aiId)) positionSelOverlay(el);
    };

    doneBtn.onclick = (e) => { e.stopPropagation(); save(); };
    clearNoteBtn.onclick = (e) => {
      e.stopPropagation();
      textarea.value = "";
      save();
    };

    textarea.addEventListener("keydown", (e) => {
      if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); save(); }
      e.stopPropagation();
    });
    textarea.addEventListener("click", (e) => e.stopPropagation());

    actions.appendChild(clearNoteBtn);
    actions.appendChild(doneBtn);
    popover.appendChild(textarea);
    popover.appendChild(actions);

    const r = btn.getBoundingClientRect();
    popover.style.top = (r.bottom + 6) + "px";
    popover.style.right = Math.max(8, window.innerWidth - r.right) + "px";

    document.body.appendChild(popover);
    activePopover = popover;
    textarea.focus();
  }

  function removeAnnotationPopover() {
    if (activePopover) {
      activePopover.remove();
      activePopover = null;
    }
  }

  // ── Chat panel ─────────────────────────────────────────────
  function createChatPanel() {
    chatPanel = document.createElement("div");
    chatPanel.className = `${NS}-root ${NS}-chat`;
    chatPanel.innerHTML = `
      <div class="${NS}-drag-handle">
        <span class="${NS}-drag-title">
          <span class="${NS}-status-dot"></span>
          <span class="${NS}-status-label">选取中</span>
          <span class="${NS}-error-badge ${NS}-hidden">0</span>
        </span>
        <div class="${NS}-panel-actions">
          <button class="${NS}-panel-btn ${NS}-history-btn" data-action="history" title="历史记录">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
            <span class="${NS}-history-badge" style="display:none;">0</span>
          </button>
          <button class="${NS}-panel-btn ${NS}-panel-btn-pause" data-action="pause" title="暂停">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><rect x="6" y="4" width="4" height="16"/><rect x="14" y="4" width="4" height="16"/></svg>
          </button>
          <button class="${NS}-panel-btn" data-action="close" title="关闭">
            <svg width="10" height="10" viewBox="0 0 10 10" fill="none">
              <line x1="1" y1="1" x2="9" y2="9" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
              <line x1="9" y1="1" x2="1" y2="9" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
            </svg>
          </button>
        </div>
      </div>
      <div class="${NS}-panel-body">
        <div class="${NS}-selection-panel ${NS}-hidden">
          <div class="${NS}-selection-row">
            <div class="${NS}-selection-heading">当前选中</div>
            <div class="${NS}-chat-tags"></div>
          </div>
          <div class="${NS}-selection-row">
            <div class="${NS}-selection-heading">DOM 路径</div>
            <div class="${NS}-css-paths"></div>
          </div>
        </div>
        <div class="${NS}-candidate-panel ${NS}-hidden">
          <div class="${NS}-candidate-header">
            <div class="${NS}-selection-heading">收集区</div>
            <button class="${NS}-tags-action ${NS}-clear-all" title="清除全部"><svg width="8" height="8" viewBox="0 0 8 8" fill="none"><line x1="1" y1="1" x2="7" y2="7" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/><line x1="7" y1="1" x2="1" y2="7" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg> 清除全部</button>
          </div>
          <div class="${NS}-candidate-list"></div>
        </div>
        <div class="${NS}-error-section ${NS}-hidden"></div>
        <button class="${NS}-copy-btn" disabled>复制提示词</button>
        <div class="${NS}-shortcuts-heading">快捷键</div>
        <div class="${NS}-shortcuts">
          <span><kbd>Click</kbd> 选取</span>
          <span><kbd>↑↓←→</kbd> DOM导航</span>
          <span><kbd>⌘Enter</kbd> 加入收集区</span>
          <span><kbd>⌘C</kbd> 复制</span>
          <span><kbd>Esc</kbd> 清除</span>
          <span><kbd>⇧P</kbd> 暂停</span>
        </div>
      </div>
    `;
    document.body.appendChild(chatPanel);

    chatPanel.querySelector(`.${NS}-copy-btn`).onclick = () => copyPrompt();
    chatPanel.querySelector('[data-action="close"]').onclick = destroy;
    chatPanel.querySelector('[data-action="pause"]').onclick = () => togglePause();
    chatPanel.querySelector('[data-action="history"]').onclick = (e) => { e.stopPropagation(); toggleHistoryDropdown(); };
    chatPanel.querySelector(`.${NS}-error-badge`).onclick = (e) => {
      e.stopPropagation();
      errorsVisible = !errorsVisible;
      updateErrorList();
    };

    makeDraggable(chatPanel, chatPanel.querySelector(`.${NS}-drag-handle`));
    updateHistoryBadge();
  }

  function updateErrorBadge() {
    const badge = chatPanel.querySelector(`.${NS}-error-badge`);
    if (!badge) return;
    const count = capturedErrors.length;
    if (count > 0) {
      badge.textContent = count;
      badge.classList.remove(`${NS}-hidden`);
    } else {
      badge.classList.add(`${NS}-hidden`);
      errorsVisible = false;
    }
  }

  function updateErrorList() {
    const section = chatPanel.querySelector(`.${NS}-error-section`);
    if (!section) return;
    section.innerHTML = "";

    if (!errorsVisible || capturedErrors.length === 0) {
      section.classList.add(`${NS}-hidden`);
      return;
    }

    section.classList.remove(`${NS}-hidden`);

    const header = document.createElement("div");
    header.className = `${NS}-error-header`;
    header.innerHTML = `<span>错误 (${capturedErrors.length})</span>`;
    const clearBtn = document.createElement("button");
    clearBtn.className = `${NS}-error-clear`;
    clearBtn.textContent = "清除";
    clearBtn.onclick = (e) => { e.stopPropagation(); clearErrors(); };
    header.appendChild(clearBtn);
    section.appendChild(header);

    capturedErrors.forEach((err, i) => {
      const item = document.createElement("div");
      item.className = `${NS}-error-item`;
      const msg = document.createElement("div");
      msg.className = `${NS}-error-msg`;
      msg.textContent = truncate(err.message, 80);
      item.appendChild(msg);

      if (err.stack) {
        const stack = document.createElement("div");
        stack.className = `${NS}-error-stack`;
        const stackLines = err.stack.split("\n").slice(0, 5).join("\n");
        stack.textContent = stackLines;
        item.appendChild(stack);
      }

      section.appendChild(item);
    });
  }

  function updateCopyButton() {
    const copyBtn = chatPanel.querySelector(`.${NS}-copy-btn`);
    if (!copyBtn) return;
    copyBtn.disabled = candidateElements.length === 0 && selectedElements.length === 0 && capturedErrors.length === 0;
  }

  let panelDragListeners = null;

  function makeDraggable(panel, handle) {
    let sx, sy, sl, st;
    handle.addEventListener("mousedown", (e) => {
      if (e.target.closest(`.${NS}-panel-btn`)) return;
      e.preventDefault();
      const r = panel.getBoundingClientRect();
      sx = e.clientX; sy = e.clientY; sl = r.left; st = r.top;
      const move = (e) => {
        panel.style.left   = sl + e.clientX - sx + "px";
        panel.style.top    = st + e.clientY - sy + "px";
        panel.style.right  = "auto";
        panel.style.bottom = "auto";
        repositionToast();
      };
      const up = () => {
        document.removeEventListener("mousemove", move);
        document.removeEventListener("mouseup", up);
        panelDragListeners = null;
      };
      document.addEventListener("mousemove", move);
      document.addEventListener("mouseup", up);
      panelDragListeners = { move, up };
    });
  }

  // ── Element label ──────────────────────────────────────────
  function elementLabel(el) {
    if (el.id) return `#${el.id}`;
    if (el.classList.length) return `.${el.classList[0]}`;
    const tag = el.tagName.toLowerCase();
    const text = (el.textContent || "").trim();
    if (text) {
      const preview = text.length > 20 ? text.slice(0, 20) + "…" : text;
      return `${tag} "${preview}"`;
    }
    return `<${tag}>`;
  }

  // ── Tags ───────────────────────────────────────────────────
  function updateTags() {
    const panel = chatPanel.querySelector(`.${NS}-selection-panel`);
    const tagsContainer = chatPanel.querySelector(`.${NS}-chat-tags`);
    const cssContainer = chatPanel.querySelector(`.${NS}-css-paths`);
    const candidatePanel = chatPanel.querySelector(`.${NS}-candidate-panel`);
    const candidateList = chatPanel.querySelector(`.${NS}-candidate-list`);
    tagsContainer.innerHTML = "";
    cssContainer.innerHTML = "";
    candidateList.innerHTML = "";

    // ── 当前选中 ──
    if (selectedElements.length > 0) {
      panel.classList.remove(`${NS}-hidden`);

      for (let i = 0; i < selectedElements.length; i++) {
        const el = selectedElements[i];
        const aiId = el.getAttribute(AI_ID);
        const tag = document.createElement("span");
        tag.className = `${NS}-tag`;
        const hasNote = annotations.has(aiId);
        tag.innerHTML = `<span class="${NS}-tag-num">${i + 1}</span><span class="${NS}-tag-label">${escapeHtml(elementLabel(el))}${hasNote ? ' ✎' : ''}</span><button class="${NS}-tag-x" data-aiid="${escapeHtml(aiId)}" title="移除">×</button>`;
        tagsContainer.appendChild(tag);
      }

      tagsContainer.querySelectorAll(`.${NS}-tag-x`).forEach((btn) => {
        btn.addEventListener("click", (e) => {
          e.stopPropagation();
          const el = byAiId(btn.dataset.aiid);
          if (el) removeSelection(el);
          updateTags();
        }, true);
      });

      // CSS path breadcrumbs
      selectedElements.forEach((el, i) => {
        const selector = buildSelector(el);
        const parts = selector.split(" > ");

        const pathEl = document.createElement("div");
        pathEl.className = `${NS}-css-path-row`;

        const numSpan = document.createElement("span");
        numSpan.className = `${NS}-css-path-num`;
        numSpan.textContent = `${i + 1}`;
        pathEl.appendChild(numSpan);

        parts.forEach((part, j) => {
          if (j > 0) {
            const sep = document.createElement("span");
            sep.className = `${NS}-css-path-sep`;
            sep.textContent = "›";
            pathEl.appendChild(sep);
          }
          const seg = document.createElement("span");
          seg.className = `${NS}-css-path-seg`;
          seg.textContent = part;

          if (j === parts.length - 1) seg.classList.add(`${NS}-css-path-active`);

          const stepsUp = parts.length - 1 - j;
          seg.onclick = (ev) => {
            ev.stopPropagation();
            let target = el;
            for (let k = 0; k < stepsUp && target.parentElement; k++) {
              target = target.parentElement;
            }
            if (target && !isEditorElement(target) && target !== document.body && target !== document.documentElement) {
              clearSelection();
              addSelection(target);
              updateTags();
            }
          };

          pathEl.appendChild(seg);
        });

        cssContainer.appendChild(pathEl);
      });
    } else {
      panel.classList.add(`${NS}-hidden`);
    }

    // ── 收集区 ──
    const clearAllBtn = candidatePanel.querySelector(`.${NS}-clear-all`);
    if (candidateElements.length > 0) {
      candidatePanel.classList.remove(`${NS}-hidden`);
      if (clearAllBtn) clearAllBtn.onclick = (e) => { e.stopPropagation(); clearAllCandidates(); };

      candidateElements.forEach((el, i) => {
        const aiId = el.getAttribute(AI_ID);
        const tag = document.createElement("span");
        tag.className = `${NS}-tag ${NS}-tag-block`;
        const hasNote = annotations.has(aiId);
        tag.innerHTML = `<span class="${NS}-tag-num">${i + 1}</span><span class="${NS}-tag-label">${escapeHtml(elementLabel(el))}${hasNote ? ' ✎' : ''}</span><button class="${NS}-tag-x" data-aiid="${escapeHtml(aiId)}" title="移除">×</button>`;
        candidateList.appendChild(tag);
      });

      candidateList.querySelectorAll(`.${NS}-tag-x`).forEach((btn) => {
        btn.addEventListener("click", (e) => {
          e.stopPropagation();
          const el = byAiId(btn.dataset.aiid);
          if (el) removeFromCandidates(el);
        }, true);
      });
    } else {
      candidatePanel.classList.add(`${NS}-hidden`);
    }

    updateCopyButton();
  }

  // ── Copy with button feedback ──────────────────────────────
  let copyTimer = null;
  function showCopyFeedback(msg) {
    const btn = chatPanel.querySelector(`.${NS}-copy-btn`);
    if (copyTimer) clearTimeout(copyTimer);
    btn.classList.add(`${NS}-copy-done`);
    btn.innerHTML = `<svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6L9 17l-5-5"/></svg> ${msg}`;
    copyTimer = setTimeout(() => {
      btn.classList.remove(`${NS}-copy-done`);
      btn.textContent = "复制提示词";
      copyTimer = null;
    }, 2000);
  }

  function copyPrompt() {
    const text = buildPromptText();
    if (!text) return;

    const domElements = [...candidateElements];
    selectedElements.forEach(el => {
      if (!domElements.includes(el)) domElements.push(el);
    });
    const serializedElements = domElements.map(el => serializeElementContext(el));
    writeToClipboard(text, serializedElements, domElements);
    showCopyFeedback("已复制");
  }

  // ── Prompt building ────────────────────────────────────────
  function buildPromptText() {
    const allElements = [...candidateElements];
    selectedElements.forEach(el => {
      if (!allElements.includes(el)) allElements.push(el);
    });

    if (allElements.length === 0 && capturedErrors.length === 0) return "";

    const lines = ["Page: " + location.pathname, ""];

    if (allElements.length > 0) {
      allElements.forEach((el, i) => {
        const ctx = buildElementContext(el, i + 1);
        lines.push(`${i + 1}. ${elementLabel(el)} <${ctx.tag}>`);
        if (ctx.selector)  lines.push(`   selector: ${ctx.selector}`);
        if (ctx.source)    lines.push(`   source: ${ctx.source}`);
        if (ctx.component) lines.push(`   component: ${ctx.component}`);
        if (ctx.text)      lines.push(`   text: "${ctx.text}"`);
        Object.entries(ctx.dataAttrs).forEach(([k, v]) => lines.push(`   ${k}: ${v}`));
        if (ctx.outerHTML)  lines.push(`   html: ${ctx.outerHTML}`);

        const aiId = el.getAttribute(AI_ID);
        const note = annotations.get(aiId);
        if (note) lines.push(`   instruction: ${note}`);
      });
    }

    if (capturedErrors.length > 0) {
      lines.push("");
      lines.push(`## Errors (${capturedErrors.length})`);
      capturedErrors.forEach((err, i) => {
        lines.push(`${i + 1}. ${err.message}`);
        if (err.stack) {
          const stackLines = err.stack.split("\n").filter(l => l.trim()).slice(0, 3);
          stackLines.forEach(l => lines.push(`   ${l.trim()}`));
        }
      });
    }

    return lines.join("\n");
  }

  function writeToClipboard(text, serializedElements, domElements) {
    if (navigator.clipboard) {
      navigator.clipboard.writeText(text).then(() => {
        if (serializedElements) saveToHistory(text, serializedElements, domElements);
      }).catch(() => {
        fallbackCopy(text, serializedElements, domElements);
      });
    } else {
      fallbackCopy(text, serializedElements, domElements);
    }
  }

  function fallbackCopy(text, serializedElements, domElements) {
    const ta = document.createElement("textarea");
    ta.value = text;
    ta.style.cssText = "position:fixed;opacity:0;top:0;left:0";
    document.body.appendChild(ta);
    ta.focus(); ta.select();
    try {
      const ok = document.execCommand("copy");
      if (ok && serializedElements) saveToHistory(text, serializedElements, domElements);
    } catch (_) {}
    ta.remove();
  }

  // ── Framework debug info (React + Vue) ─────────────────────
  const SKIP_COMPONENT = new Set([
    "ClientPageRoot","LinkComponent","ServerComponent","AppRouter",
    "Router","HotReload","ReactDevOverlay","InnerLayoutRouter",
    "OuterLayoutRouter","RedirectBoundary","NotFoundBoundary",
    "ErrorBoundary","LoadingBoundary","TemplateContext",
    "ScrollAndFocusHandler","RenderFromTemplateContext",
    "PathnameContextProviderAdapter","Hot","Inner","Forward","Root",
    "VNode","Transition","TransitionGroup","KeepAlive","Teleport",
    "Suspense","BaseTransition","Fragment",
  ]);

  function isUserComponent(name) {
    if (!name || name.length < 2) return false;
    if (SKIP_COMPONENT.has(name)) return false;
    if (/^[a-z]/.test(name)) return false;
    if (name.startsWith("_")) return false;
    return true;
  }

  function getReactDebug(el) {
    try {
      const fiberKey = Object.keys(el).find(k =>
        k.startsWith("__reactFiber") || k.startsWith("__reactInternalInstance")
      );
      if (!fiberKey) return {};

      const result = {};
      let f = el[fiberKey];

      let walker = f;
      while (walker) {
        if (walker._debugSource) {
          const s = walker._debugSource;
          const file = s.fileName.replace(/^.*?\/src\//, "src/");
          result.source = `${file}:${s.lineNumber}`;
          break;
        }
        walker = walker.return;
      }

      const components = [];
      walker = f;
      while (walker) {
        if (walker.type && typeof walker.type === "function") {
          const name = walker.type.displayName || walker.type.name;
          if (isUserComponent(name) && !components.includes(name)) {
            components.push(name);
            if (components.length >= 3) break;
          }
        }
        walker = walker.return;
      }
      if (components.length) result.component = components.reverse().join(" › ");

      return result;
    } catch (_) {
      return {};
    }
  }

  function getVueDebug(el) {
    try {
      // Vue 3: walk up via __vue_parent_component
      let vue3 = el.__vue_parent_component;
      if (vue3) {
        const result = {};
        const components = [];
        let cur = vue3;
        while (cur) {
          const name = cur.type?.displayName || cur.type?.name || cur.type?.__name;
          if (isUserComponent(name) && !components.includes(name)) {
            components.push(name);
            if (components.length >= 3) break;
          }
          if (!result.source && cur.type?.__file) {
            const file = cur.type.__file.replace(/^.*?\/src\//, "src/");
            result.source = file;
          }
          cur = cur.parent;
        }
        if (components.length) result.component = components.reverse().join(" › ");
        if (Object.keys(result).length) return result;
      }

      // Vue 2: __vue__ on the element or ancestors
      let node = el;
      while (node && node !== document.body) {
        const vm = node.__vue__;
        if (vm) {
          const name = vm.$options.name || vm.$options._componentTag;
          if (isUserComponent(name)) {
            const result = { component: name };
            if (vm.$options.__file) {
              result.source = vm.$options.__file.replace(/^.*?\/src\//, "src/");
            }
            return result;
          }
        }
        node = node.parentElement;
      }

      return {};
    } catch (_) {
      return {};
    }
  }

  function getFrameworkDebug(el) {
    const react = getReactDebug(el);
    if (react.component) return react;
    return getVueDebug(el);
  }

  // ── Element context ────────────────────────────────────────
  function buildElementContext(el, index) {
    const dataAttrs = {};
    for (const attr of el.attributes) {
      if (attr.name.startsWith("data-") && attr.name !== AI_ID) {
        dataAttrs[attr.name] = attr.value;
      }
    }
    const frameworkInfo = getFrameworkDebug(el);
    return {
      index,
      aiId: el.getAttribute(AI_ID),
      selector: buildSelector(el),
      tag: el.tagName.toLowerCase(),
      text: truncate(el.textContent, 80),
      outerHTML: el.outerHTML.replace(/\s*data-ai-id="[^"]*"/g, "").slice(0, MAX_HTML_LENGTH),
      dataAttrs,
      ...frameworkInfo,
    };
  }

  function buildSelector(el) {
    if (el.id) return `#${el.id}`;
    const parts = [];
    let node = el;
    while (node && node !== document.body && node !== document.documentElement) {
      let seg = node.tagName.toLowerCase();
      if (node.id) { parts.unshift(`#${node.id}`); break; }
      const p = node.parentElement;
      if (p) {
        let idx = 0;
        for (let s = p.firstElementChild; s; s = s.nextElementSibling) {
          idx++;
          if (s === node) break;
        }
        if (p.children.length > 1) seg += `:nth-child(${idx})`;
      }
      parts.unshift(seg);
      node = node.parentElement;
    }
    return parts.join(" > ");
  }

  function truncate(s, max) {
    if (!s) return "";
    s = s.replace(/\s+/g, " ").trim();
    return s.length > max ? s.slice(0, max) + "…" : s;
  }

  function escapeHtml(s) {
    return s.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
  }

  // ── Boot ───────────────────────────────────────────────────
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
  else init();
  window.__selectorDestroy = destroy;
})();

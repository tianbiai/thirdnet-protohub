/**
 * ProtoPick — visual element picker with per-element annotations.
 * Inject via bookmarklet. Click = select, Drag = marquee.
 */
(function () {
  "use strict";

  if (document.querySelector(".ai-editor-root")) return;

  const NS = "ai-editor";
  const AI_ID = "data-ai-id";

  let selectedElements = [];
  let chatPanel = null;
  let hoverBox = null;
  let aiIdCounter = 0;
  let rafPending = false;
  let lastMoveTarget = null;
  const selOverlays = new Map();
  const annotations = new Map();
  const listeners = [];
  let dragState = null;
  let wasJustDragging = false;
  let activePopover = null;
  const capturedErrors = [];
  let originalConsoleError = null;
  let errorsVisible = false;
  let errorToastTimer = null;

  function on(target, type, fn, capture) {
    target.addEventListener(type, fn, capture);
    listeners.push({ target, type, fn, capture });
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
    on(document, "mouseup", handleMouseUp, true);
    on(document, "mouseleave", () => { showHover(null); cancelDrag(); }, true);
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

  function showErrorToast(count) {
    let toast = chatPanel.querySelector(`.${NS}-error-toast`);
    if (!toast) {
      toast = document.createElement("div");
      toast.className = `${NS}-root ${NS}-error-toast`;
      chatPanel.appendChild(toast);
    }
    toast.textContent = `${count} error${count > 1 ? "s" : ""} captured`;
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
    return s.display !== "none" && s.visibility !== "hidden" && s.opacity !== "0";
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
    if (dragState) {
      const dx = e.clientX - dragState.startX;
      const dy = e.clientY - dragState.startY;

      if (!dragState.isDragging && (Math.abs(dx) > 5 || Math.abs(dy) > 5)) {
        dragState.isDragging = true;
        dragState.marquee = document.createElement("div");
        dragState.marquee.className = `${NS}-marquee`;
        document.body.appendChild(dragState.marquee);
        showHover(null);
      }

      if (dragState.isDragging) {
        const left = Math.min(e.clientX, dragState.startX);
        const top = Math.min(e.clientY, dragState.startY);
        dragState.marquee.style.left = left + "px";
        dragState.marquee.style.top = top + "px";
        dragState.marquee.style.width = Math.abs(dx) + "px";
        dragState.marquee.style.height = Math.abs(dy) + "px";
        return;
      }
    }

    lastMoveTarget = resolveTarget(e.target);
    if (!rafPending) {
      rafPending = true;
      requestAnimationFrame(() => { showHover(lastMoveTarget); rafPending = false; });
    }
  }

  function handleMouseDown(e) {
    if (isEditorElement(e.target)) return;
    if (e.button !== 0) return;

    dragState = {
      startX: e.clientX,
      startY: e.clientY,
      isDragging: false,
      marquee: null,
    };
  }

  function handleMouseUp(e) {
    if (!dragState || !dragState.isDragging) {
      dragState = null;
      return;
    }

    wasJustDragging = true;

    const mRect = dragState.marquee.getBoundingClientRect();
    dragState.marquee.remove();
    dragState = null;

    clearSelection();

    document.querySelectorAll(`[${AI_ID}]`).forEach((el) => {
      if (isEditorElement(el)) return;
      if (!isVisible(el)) return;
      if (!isMeaningful(el)) return;
      const r = el.getBoundingClientRect();
      if (rectsIntersect(mRect, r)) addSelection(el);
    });

    updateTags();
    setTimeout(() => { wasJustDragging = false; }, 0);
  }

  function cancelDrag() {
    if (dragState && dragState.marquee) dragState.marquee.remove();
    dragState = null;
  }

  function rectsIntersect(a, b) {
    return !(a.right < b.left || a.left > b.right || a.bottom < b.top || a.top > b.bottom);
  }

  function handleClick(e) {
    if (isEditorElement(e.target)) return;
    if (wasJustDragging) return;

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
    annotateBtn.title = "Add instruction";
    annotateBtn.innerHTML = '<svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M12 20h9"/><path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4Z"/></svg>';
    annotateBtn.onclick = (e) => {
      e.stopPropagation();
      e.preventDefault();
      showAnnotationPopover(el, annotateBtn);
    };

    document.body.appendChild(box);
    document.body.appendChild(label);
    document.body.appendChild(annotateBtn);
    selOverlays.set(aiId, { box, corners, label, annotateBtn });
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

    ov.label.style.top = (r.top - pad - 20) + "px";
    ov.label.style.left = (r.left - pad) + "px";

    ov.annotateBtn.style.top = (r.top - pad - 22) + "px";
    ov.annotateBtn.style.left = (r.right + pad + 4) + "px";

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
      annotations.delete(aiId);
    }
  }

  function clearSelection() {
    destroyAllOverlays();
    selectedElements = [];
    annotations.clear();
    removeAnnotationPopover();
  }

  function handleKeyDown(e) {
    if (isEditorElement(e.target) && (e.target.tagName === "INPUT" || e.target.tagName === "TEXTAREA")) return;
    const mod = e.metaKey || e.ctrlKey;

    if (e.key === "Escape") {
      if (activePopover) { removeAnnotationPopover(); }
      else { clearSelection(); updateTags(); }
      return;
    }
    if (mod && e.key.toLowerCase() === "c" && !e.shiftKey && (selectedElements.length > 0 || capturedErrors.length > 0)) {
      e.preventDefault();
      copyPrompt();
      return;
    }
  }

  // ── Annotation popover ─────────────────────────────────────
  function showAnnotationPopover(el, btn) {
    removeAnnotationPopover();

    const aiId = el.getAttribute(AI_ID);
    const popover = document.createElement("div");
    popover.className = `${NS}-root ${NS}-annotate-popover`;

    const textarea = document.createElement("textarea");
    textarea.className = `${NS}-annotate-input`;
    textarea.value = annotations.get(aiId) || "";
    textarea.placeholder = "Instruction for this element\u2026";
    textarea.rows = 2;

    const actions = document.createElement("div");
    actions.className = `${NS}-annotate-actions`;

    const clearNoteBtn = document.createElement("button");
    clearNoteBtn.className = `${NS}-annotate-clear`;
    clearNoteBtn.textContent = "Clear";

    const doneBtn = document.createElement("button");
    doneBtn.className = `${NS}-annotate-done`;
    doneBtn.textContent = "Done";

    const save = () => {
      const val = textarea.value.trim();
      if (val) annotations.set(aiId, val);
      else annotations.delete(aiId);
      removeAnnotationPopover();
      positionSelOverlay(el);
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
          <span class="${NS}-status-label">Selecting</span>
          <span class="${NS}-error-badge ${NS}-hidden">0</span>
        </span>
        <div class="${NS}-panel-actions">
          <button class="${NS}-panel-btn" data-action="close" title="Close">
            <svg width="10" height="10" viewBox="0 0 10 10" fill="none">
              <line x1="1" y1="1" x2="9" y2="9" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
              <line x1="9" y1="1" x2="1" y2="9" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
            </svg>
          </button>
        </div>
      </div>
      <div class="${NS}-panel-body">
        <div class="${NS}-chat-tags ${NS}-hidden"></div>
        <div class="${NS}-error-section ${NS}-hidden"></div>
        <div class="${NS}-shortcuts">
          <span><kbd>Click</kbd> Select</span>
          <span><kbd>Drag</kbd> Multi</span>
          <span><kbd>\u2318C</kbd> Copy</span>
          <span><kbd>Esc</kbd> Clear</span>
        </div>
        <button class="${NS}-copy-btn" disabled>Copy Prompt</button>
      </div>
    `;
    document.body.appendChild(chatPanel);

    chatPanel.querySelector(`.${NS}-copy-btn`).onclick = () => copyPrompt();
    chatPanel.querySelector('[data-action="close"]').onclick = destroy;
    chatPanel.querySelector(`.${NS}-error-badge`).onclick = (e) => {
      e.stopPropagation();
      errorsVisible = !errorsVisible;
      updateErrorList();
    };

    makeDraggable(chatPanel, chatPanel.querySelector(`.${NS}-drag-handle`));
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
    header.innerHTML = `<span>Errors (${capturedErrors.length})</span>`;
    const clearBtn = document.createElement("button");
    clearBtn.className = `${NS}-error-clear`;
    clearBtn.textContent = "Clear";
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
    copyBtn.disabled = selectedElements.length === 0 && capturedErrors.length === 0;
  }

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
      };
      const up = () => {
        document.removeEventListener("mousemove", move);
        document.removeEventListener("mouseup", up);
      };
      document.addEventListener("mousemove", move);
      document.addEventListener("mouseup", up);
    });
  }

  // ── Element label ──────────────────────────────────────────
  function elementLabel(el) {
    if (el.id) return `#${el.id}`;
    if (el.classList.length) return `.${el.classList[0]}`;
    const tag = el.tagName.toLowerCase();
    const text = (el.textContent || "").trim();
    if (text) {
      const preview = text.length > 20 ? text.slice(0, 20) + "\u2026" : text;
      return `${tag} "${preview}"`;
    }
    return `<${tag}>`;
  }

  // ── Tags ───────────────────────────────────────────────────
  function updateTags() {
    const container = chatPanel.querySelector(`.${NS}-chat-tags`);
    container.innerHTML = "";

    if (selectedElements.length > 0) {
      container.classList.remove(`${NS}-hidden`);
      updateCopyButton();

      for (let i = 0; i < selectedElements.length; i++) {
        const el = selectedElements[i];
        const aiId = el.getAttribute(AI_ID);
        const tag = document.createElement("span");
        tag.className = `${NS}-tag`;
        const hasNote = annotations.has(aiId);
        tag.innerHTML = `<span class="${NS}-tag-num">${i + 1}</span><span class="${NS}-tag-label">${elementLabel(el)}${hasNote ? ' \u270e' : ''}</span><button class="${NS}-tag-x" data-aiid="${aiId}" title="Remove">\u00d7</button>`;
        container.appendChild(tag);
      }

      container.querySelectorAll(`.${NS}-tag-x`).forEach((btn) => {
        btn.addEventListener("click", (e) => {
          e.stopPropagation();
          const el = byAiId(btn.dataset.aiid);
          if (el) removeSelection(el);
          updateTags();
        }, true);
      });

      const clearAllBtn = document.createElement("button");
      clearAllBtn.className = `${NS}-tags-action`;
      clearAllBtn.title = "Clear all";
      clearAllBtn.innerHTML = `<svg width="8" height="8" viewBox="0 0 8 8" fill="none"><line x1="1" y1="1" x2="7" y2="7" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/><line x1="7" y1="1" x2="1" y2="7" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg> Clear`;
      clearAllBtn.onclick = (e) => { e.stopPropagation(); clearSelection(); updateTags(); };
      container.appendChild(clearAllBtn);
    } else {
      container.classList.add(`${NS}-hidden`);
      updateCopyButton();
    }
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
      btn.textContent = "Copy Prompt";
      copyTimer = null;
    }, 2000);
  }

  function copyPrompt() {
    const text = buildPromptText();
    if (!text) return;
    writeToClipboard(text);
    showCopyFeedback("Copied");
  }

  // ── Prompt building ────────────────────────────────────────
  function buildPromptText() {
    if (selectedElements.length === 0 && capturedErrors.length === 0) return "";

    const lines = ["Page: " + location.pathname, ""];

    if (selectedElements.length > 0) {
      selectedElements.forEach((el, i) => {
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

  function writeToClipboard(text) {
    if (navigator.clipboard) {
      navigator.clipboard.writeText(text).catch(() => fallbackCopy(text));
    } else {
      fallbackCopy(text);
    }
  }

  function fallbackCopy(text) {
    const ta = document.createElement("textarea");
    ta.value = text;
    ta.style.cssText = "position:fixed;opacity:0;top:0;left:0";
    document.body.appendChild(ta);
    ta.focus(); ta.select();
    try { document.execCommand("copy"); } catch (_) {}
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
      if (components.length) result.component = components.reverse().join(" \u203a ");

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
      outerHTML: el.outerHTML.slice(0, 200),
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
        const s = Array.from(p.children).filter(c => c.tagName === node.tagName);
        if (s.length > 1) seg += `:nth-of-type(${s.indexOf(node) + 1})`;
      }
      parts.unshift(seg);
      node = node.parentElement;
    }
    return parts.join(" > ");
  }

  function truncate(s, max) {
    if (!s) return "";
    s = s.replace(/\s+/g, " ").trim();
    return s.length > max ? s.slice(0, max) + "\u2026" : s;
  }

  // ── Boot ───────────────────────────────────────────────────
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
  else init();
  window.__selectorDestroy = destroy;
})();

<template>
  <div class="doc-viewer">
    <!-- 加载状态 -->
    <div v-if="loading" class="loading-state">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <span>加载文档中...</span>
    </div>

    <!-- 错误状态 -->
    <div v-else-if="error" class="error-state">
      <el-icon :size="48" color="var(--danger-color)"><WarningFilled /></el-icon>
      <h3>文档加载失败</h3>
      <p>{{ error }}</p>
      <el-button type="primary" @click="reload">重新加载</el-button>
    </div>

    <!-- 文档内容 -->
    <article v-else class="doc-content" v-html="docHtml"></article>
  </div>
</template>

<script setup>
import { watch, onMounted } from 'vue'
import { useDoc } from '@/composables/useDoc'
import 'highlight.js/styles/github.css'

const props = defineProps({
  url: {
    type: String,
    required: true
  }
})

const { docHtml, loading, error, loadDoc } = useDoc()

// 重新加载
function reload() {
  if (props.url) {
    loadDoc(props.url)
  }
}

// 监听 URL 变化
watch(() => props.url, (newUrl) => {
  if (newUrl) {
    loadDoc(newUrl)
  }
}, { immediate: true })

onMounted(() => {
  if (props.url) {
    loadDoc(props.url)
  }
})
</script>

<style lang="scss" scoped>
.doc-viewer {
  height: 100%;
  overflow-y: auto;
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
}

.loading-state,
.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  gap: 16px;
  color: var(--text-secondary);

  h3 {
    font-size: 18px;
    font-weight: 500;
    color: var(--text-primary);
  }

  p {
    font-size: var(--font-size-sm);
    color: var(--text-tertiary);
  }
}

.doc-content {
  max-width: 900px;
  margin: 0 auto;
  padding: 48px 32px;
  line-height: 1.7;

  // Markdown 样式 - 苹果风格
  :deep(h1) {
    font-size: 32px;
    font-weight: 700;
    color: var(--text-primary);
    margin-bottom: 24px;
    padding-bottom: 16px;
    border-bottom: 1px solid var(--border-light);
  }

  :deep(h2) {
    font-size: 24px;
    font-weight: 600;
    color: var(--text-primary);
    margin: 32px 0 16px;
  }

  :deep(h3) {
    font-size: 20px;
    font-weight: 600;
    color: var(--text-primary);
    margin: 24px 0 12px;
  }

  :deep(h4) {
    font-size: 17px;
    font-weight: 600;
    color: var(--text-primary);
    margin: 20px 0 10px;
  }

  :deep(p) {
    margin-bottom: 16px;
    color: var(--text-secondary);
  }

  :deep(a) {
    color: var(--primary-color);

    &:hover {
      text-decoration: underline;
    }
  }

  :deep(ul),
  :deep(ol) {
    padding-left: 24px;
    margin-bottom: 16px;

    li {
      margin-bottom: 8px;
      color: var(--text-secondary);
    }
  }

  :deep(blockquote) {
    margin: 16px 0;
    padding: 12px 16px;
    background: var(--bg-secondary);
    border-left: 4px solid var(--primary-color);
    border-radius: 0 var(--radius-sm) var(--radius-sm) 0;

    p {
      margin: 0;
      color: var(--text-secondary);
    }
  }

  :deep(pre) {
    margin: 16px 0;
    padding: 16px;
    background: #F6F8FA;
    border: 1px solid var(--border-light);
    border-radius: var(--radius-md);
    overflow-x: auto;
  }

  :deep(code) {
    font-family: var(--font-mono);
    font-size: 13px;
  }

  :deep(pre code) {
    background: none;
    padding: 0;
  }

  :deep(p code) {
    background: var(--bg-tertiary);
    padding: 2px 6px;
    border-radius: 4px;
  }

  :deep(table) {
    width: 100%;
    margin: 16px 0;
    border-collapse: collapse;

    th,
    td {
      padding: 12px;
      text-align: left;
      border: 1px solid var(--border-light);
    }

    th {
      background: var(--bg-secondary);
      font-weight: 500;
      color: var(--text-primary);
    }

    td {
      color: var(--text-secondary);
    }
  }

  :deep(img) {
    max-width: 100%;
    border-radius: var(--radius-md);
  }

  :deep(hr) {
    margin: 24px 0;
    border: none;
    border-top: 1px solid var(--border-light);
  }
}
</style>

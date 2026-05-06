/**
 * Markdown 文档加载和渲染 composable
 */
import { ref } from 'vue'
import { Marked } from 'marked'
import hljs from 'highlight.js'
import DOMPurify from 'dompurify'

// 创建隔离的 marked 实例，避免污染全局
const markedInstance = new Marked()

markedInstance.setOptions({ breaks: true, gfm: true })

markedInstance.use({
  renderer: {
    code(code: string, language: string | undefined) {
      if (language && hljs.getLanguage(language)) {
        try {
          const highlighted = hljs.highlight(code, { language }).value
          return `<pre><code class="hljs language-${language}">${highlighted}</code></pre>`
        } catch (e) {
          console.error('代码高亮失败:', e)
        }
      }
      return `<pre><code class="hljs">${code}</code></pre>`
    }
  }
})

export function useDoc() {
  /** 原始 Markdown 内容 */
  const docContent = ref('')

  /** 渲染后的 HTML */
  const docHtml = ref('')

  /** 加载状态 */
  const loading = ref(false)

  /** 错误信息 */
  const error = ref<string | null>(null)

  /** 加载文档 */
  async function loadDoc(docPath: string) {
    if (!docPath) {
      error.value = '文档路径不能为空'
      return
    }

    loading.value = true
    error.value = null

    try {
      const response = await fetch(docPath)
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }
      const content = await response.text()
      docContent.value = content
      const rawHtml = markedInstance.parse(docContent.value)
      const htmlStr = typeof rawHtml === 'string' ? rawHtml : await rawHtml
      docHtml.value = DOMPurify.sanitize(htmlStr)
    } catch (e) {
      const err = e instanceof Error ? e : new Error(String(e))
      if (err.name === 'TypeError' && err.message.includes('fetch')) {
        error.value = '无法加载文档：CORS 跨域限制\n\n请尝试：\n• 点击「使用 iframe 加载」查看原始内容\n• 点击「新窗口打开」直接访问'
      } else {
        error.value = err.message
      }
      console.error('文档加载失败:', e)
    } finally {
      loading.value = false
    }
  }

  /** 加载项目 spec.md */
  async function loadProjectSpec(projectPath: string) {
    return loadDoc(`/${projectPath}/spec.md`)
  }

  /** 加载项目 changelog.md */
  async function loadProjectChangelog(projectPath: string) {
    return loadDoc(`/${projectPath}/changelog.md`)
  }

  /** 加载页面规格文档 */
  async function loadPageSpec(projectPath: string, pageName: string) {
    return loadDoc(`/${projectPath}/specs/${pageName}.md`)
  }

  /** 解析 Markdown 文本 */
  function parseMarkdown(text: string): string {
    if (!text) return ''
    const rawHtml = markedInstance.parse(text)
    const htmlStr = typeof rawHtml === 'string' ? rawHtml : ''
    return DOMPurify.sanitize(htmlStr)
  }

  return {
    docContent,
    docHtml,
    loading,
    error,
    loadDoc,
    loadProjectSpec,
    loadProjectChangelog,
    loadPageSpec,
    parseMarkdown
  }
}

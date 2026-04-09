import { ref } from 'vue'
import { marked } from 'marked'
import hljs from 'highlight.js'
import DOMPurify from 'dompurify'

// 配置 marked - 使用自定义渲染器代替已废弃的 highlight 选项
const renderer = new marked.Renderer()
renderer.code = function(code, language) {
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

marked.setOptions({
  renderer,
  breaks: true,
  gfm: true
})

export function useDoc() {
  const docContent = ref('')
  const docHtml = ref('')
  const loading = ref(false)
  const error = ref(null)

  // 加载文档
  async function loadDoc(docPath) {
    if (!docPath) {
      error.value = '文档路径不能为空'
      return
    }

    loading.value = true
    error.value = null

    try {
      // 从网络加载
      const response = await fetch(docPath)
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }
      const content = await response.text()

      docContent.value = content
      // 使用 DOMPurify 进行 XSS 防护
      const rawHtml = marked.parse(docContent.value)
      docHtml.value = DOMPurify.sanitize(rawHtml)
    } catch (e) {
      // 判断是否为 CORS 错误
      if (e.name === 'TypeError' && e.message.includes('fetch')) {
        error.value = '无法加载文档：CORS 跨域限制\n\n请尝试：\n• 点击「使用 iframe 加载」查看原始内容\n• 点击「新窗口打开」直接访问'
      } else {
        error.value = e.message
      }
      console.error('文档加载失败:', e)
    } finally {
      loading.value = false
    }
  }

  // 加载项目 spec.md
  async function loadProjectSpec(projectPath) {
    return loadDoc(`/${projectPath}/spec.md`)
  }

  // 加载项目 changelog.md
  async function loadProjectChangelog(projectPath) {
    return loadDoc(`/${projectPath}/changelog.md`)
  }

  // 加载页面规格文档
  async function loadPageSpec(projectPath, pageName) {
    return loadDoc(`/${projectPath}/specs/${pageName}.md`)
  }

  // 解析 Markdown 文本
  function parseMarkdown(text) {
    if (!text) return ''
    const rawHtml = marked.parse(text)
    return DOMPurify.sanitize(rawHtml)
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

export default useDoc

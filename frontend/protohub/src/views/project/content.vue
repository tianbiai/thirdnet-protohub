<template>
  <div class="content-page">
    <!-- 加载中 -->
    <div v-if="loading" class="loading-state">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <span>加载中...</span>
    </div>

    <!-- 未找到菜单项 -->
    <div v-else-if="!item" class="empty-state">
      <el-empty description="未找到内容" />
    </div>

    <!-- 文档类型 -->
    <template v-else-if="item?.type === 'doc'">
      <div class="content-header doc-header">
        <div class="header-left">
          <el-button text @click="goBack">
            <el-icon><ArrowLeft /></el-icon>
            返回
          </el-button>
          <h2>{{ item?.name }}</h2>
          <el-tag v-if="item?.docFileName" type="success" size="small">
            {{ item.docFileName }}
          </el-tag>
          <el-tag v-else-if="item?.docDescription" type="info" size="small">
            {{ item.docDescription }}
          </el-tag>
        </div>
        <div class="header-right">
          <el-button v-if="item?.url && !item?.docFileId" type="primary" plain @click="openExternal">
            <el-icon><Link /></el-icon>
            新窗口打开
          </el-button>
          <el-button type="success" plain @click="refreshDoc">
            <el-icon><Refresh /></el-icon>
            刷新
          </el-button>
          <HelpBubble :items="[{ content: 'Markdown 文档查看器' }]" />
        </div>
      </div>
      <div class="content-body doc-body">
        <DocViewer ref="docViewerRef" :url="item?.url" />
      </div>
    </template>

    <!-- Swagger API 文档类型 -->
    <template v-else-if="item?.type === 'swagger'">
      <div class="content-header swagger-header">
        <div class="header-left">
          <el-button text @click="goBack">
            <el-icon><ArrowLeft /></el-icon>
            返回
          </el-button>
          <h2>{{ item?.name }}</h2>
          <el-tag v-if="item?.apiDescription" type="info" size="small">
            {{ item.apiDescription }}
          </el-tag>
        </div>
        <div class="header-right">
          <el-button v-if="item?.url" type="primary" plain @click="openExternal">
            <el-icon><Link /></el-icon>
            新窗口打开
          </el-button>
          <el-button type="success" plain @click="refresh">
            <el-icon><Refresh /></el-icon>
            刷新
          </el-button>
          <HelpBubble :items="[{ content: 'API 接口文档' }]" />
        </div>
      </div>
      <div class="content-body swagger-body">
        <iframe
          ref="swaggerIframeRef"
          :src="item?.url"
          class="swagger-iframe"
          frameborder="0"
          allowfullscreen
        ></iframe>
      </div>
    </template>

    <!-- Web/移动端类型 -->
    <template v-else>
      <!-- 侧边工具栏 -->
      <div class="sidebar-toolbar">
        <div class="sidebar-top">
          <el-tooltip content="返回" placement="right">
            <el-button circle @click="goBack">
              <el-icon><ArrowLeft /></el-icon>
            </el-button>
          </el-tooltip>
          <el-tooltip v-if="item?.url" content="新窗口打开" placement="right">
            <el-button circle type="primary" plain @click="openExternal">
              <el-icon><Link /></el-icon>
            </el-button>
          </el-tooltip>
          <el-tooltip content="刷新" placement="right">
            <el-button circle type="success" plain @click="refresh">
              <el-icon><Refresh /></el-icon>
            </el-button>
          </el-tooltip>
        </div>
        <div class="sidebar-bottom">
          <HelpBubble :items="[{ content: item?.type === 'mobile' ? '移动端应用预览' : 'Web 应用预览' }]" placement="right" />
        </div>
      </div>
      <div class="content-body">
        <IframeViewer
          ref="iframeRef"
          :src="item?.url"
          :type="item?.type"
          :viewport="item?.viewport"
        />
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useMenuStore } from '@/stores/menu'
import { Loading } from '@element-plus/icons-vue'
import DocViewer from '@/components/DocViewer/index.vue'
import IframeViewer from '@/components/IframeViewer/index.vue'
import HelpBubble from '@/components/HelpBubble/index.vue'

const route = useRoute()
const router = useRouter()
const menuStore = useMenuStore()

const iframeRef = ref(null)
const swaggerIframeRef = ref(null)
const docViewerRef = ref(null)
const loading = ref(true)

// 当前内容项
const item = computed(() => {
  const itemId = route.params.id
  for (const group of menuStore.groups) {
    const found = group.children?.find(i => i.id === itemId)
    if (found) return found
  }
  return null
})

// 返回
function goBack() {
  router.back()
}

// 在新窗口打开
function openExternal() {
  if (item.value?.url) {
    window.open(item.value.url, '_blank')
  }
}

// 刷新
function refresh() {
  if (item.value?.type === 'swagger' && swaggerIframeRef.value) {
    swaggerIframeRef.value.src = item.value.url
  } else if (iframeRef.value) {
    iframeRef.value.reload()
  }
}

// 刷新文档
function refreshDoc() {
  if (docViewerRef.value) {
    docViewerRef.value.reload()
  }
}

// 加载菜单配置
onMounted(async () => {
  if (menuStore.groups.length === 0) {
    await menuStore.loadMenuConfig()
  }
  loading.value = false
})
</script>

<style lang="scss" scoped>
.content-page {
  height: 100%;
  display: flex;
  flex-direction: column;
  background: var(--bg-primary);
  overflow: hidden;
  position: relative;
}

// 侧边工具栏
.sidebar-toolbar {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 56px;
  background: var(--bg-primary);
  border-right: 1px solid var(--border-light);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 16px 8px;
  z-index: 10;

  .sidebar-top,
  .sidebar-bottom {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 12px;
  }

  .el-button.is-circle {
    width: 40px;
    height: 40px;
  }
}

.content-body {
  flex: 1;
  min-height: 0;
  margin-left: 56px;
}

// 文档类型样式
.doc-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 24px;
  background: var(--bg-primary);
  border-bottom: 1px solid var(--border-light);

  .header-left {
    display: flex;
    align-items: center;
    gap: 12px;

    h2 {
      font-size: 16px;
      font-weight: 600;
      color: var(--text-primary);
    }
  }

  .header-right {
    display: flex;
    align-items: center;
    gap: 12px;
  }
}

.doc-body {
  margin-left: 0;
  overflow: hidden;
}

// Swagger API 文档样式
.swagger-header {
  position: relative;
  opacity: 1;
  transform: none;

  .header-left {
    display: flex;
    align-items: center;
    gap: 12px;

    h2 {
      font-size: 16px;
      font-weight: 600;
      color: var(--text-primary);
    }
  }

  .header-right {
    display: flex;
    align-items: center;
    gap: 12px;
  }
}

.swagger-body {
  flex: 1;
  min-height: 0;
  overflow: hidden;
}

.swagger-iframe {
  width: 100%;
  height: 100%;
  border: none;
}

// 加载状态
.loading-state {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  color: var(--text-secondary);
}

// 空状态
.empty-state {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>

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

    <!-- 超链接类型 -->
    <template v-else-if="item?.type === 'link'">
      <div class="content-header link-header">
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
          <HelpBubble :items="[{ content: '超链接' }]" />
        </div>
      </div>
      <div class="content-body link-body">
        <iframe
          ref="linkIframeRef"
          :src="linkSrc"
          class="link-iframe"
          frameborder="0"
          sandbox="allow-scripts allow-same-origin allow-popups allow-forms"
          allowfullscreen
        ></iframe>
      </div>
    </template>

    <!-- 变更日志类型 -->
    <template v-else-if="item?.type === 'changelog'">
      <div class="content-header changelog-header">
        <div class="header-left">
          <el-button text @click="goBack">
            <el-icon><ArrowLeft /></el-icon>
            返回
          </el-button>
          <h2>{{ item?.name }}</h2>
          <el-tag v-if="item?.changelogDescription" type="info" size="small">
            {{ item.changelogDescription }}
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
          <HelpBubble :items="[{ content: '变更日志' }]" />
        </div>
      </div>
      <div class="content-body changelog-body">
        <iframe
          ref="changelogIframeRef"
          :src="changelogSrc"
          class="changelog-iframe"
          frameborder="0"
          sandbox="allow-scripts allow-same-origin allow-popups allow-forms"
          allowfullscreen
        ></iframe>
      </div>
    </template>

    <!-- Web/小程序类型 -->
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
import IframeViewer from '@/components/IframeViewer/index.vue'
import HelpBubble from '@/components/HelpBubble/index.vue'
import { cacheBustUrl } from '@/utils/menu'

const route = useRoute()
const router = useRouter()
const menuStore = useMenuStore()

const iframeRef = ref(null)
const linkIframeRef = ref(null)
const changelogIframeRef = ref(null)
const loading = ref(true)
const linkCacheKey = ref(Date.now())
const changelogCacheKey = ref(Date.now())

// 当前内容项
const item = computed(() => menuStore.findItemById(route.params.id))

// 超链接 iframe 带缓存破坏的 URL
const linkSrc = computed(() => cacheBustUrl(item.value?.url, linkCacheKey.value))

// Changelog iframe 带缓存破坏的 URL
const changelogSrc = computed(() => cacheBustUrl(item.value?.url, changelogCacheKey.value))

// 返回
function goBack() {
  router.back()
}

// 在新窗口打开
function openExternal() {
  if (item.value?.url) {
    window.open(item.value.url, '_blank', 'noopener,noreferrer')
  }
}

// 刷新
function refresh() {
  if (item.value?.type === 'link') {
    linkCacheKey.value = Date.now()
  } else if (item.value?.type === 'changelog') {
    changelogCacheKey.value = Date.now()
  } else if (iframeRef.value) {
    iframeRef.value.reload()
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

// 变更日志类型样式
.changelog-header {
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

.changelog-body {
  flex: 1;
  min-height: 0;
  overflow: hidden;
}

.changelog-iframe {
  width: 100%;
  height: 100%;
  border: none;
}

// 超链接类型样式
.link-header {
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

.link-body {
  flex: 1;
  min-height: 0;
  overflow: hidden;
}

.link-iframe {
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

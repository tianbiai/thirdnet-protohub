<template>
  <div class="project-detail-page">
    <div class="page-header">
      <el-button text @click="goBack">
        <el-icon><ArrowLeft /></el-icon>
        返回
      </el-button>
      <h2>{{ item?.name }}</h2>
    </div>

    <div class="detail-content">
      <!-- 项目信息 -->
      <div class="info-section">
        <h3>基本信息</h3>
        <div class="info-grid">
          <div class="info-item">
            <span class="label">图标</span>
            <span class="value icon-value"><img src="@/assets/logo.png" alt="" class="detail-logo-img" /></span>
          </div>
          <div class="info-item">
            <span class="label">类型</span>
            <el-tag :type="getTypeConfig(item?.type).type">
              {{ getTypeConfig(item?.type).label }}
            </el-tag>
          </div>
          <div class="info-item full">
            <span class="label">描述</span>
            <span class="value">{{ item?.description || '暂无描述' }}</span>
          </div>
          <div v-if="item?.url" class="info-item full">
            <span class="label">URL</span>
            <span class="value link" @click="openUrl(item.url)">{{ item.url }}</span>
          </div>
        </div>
      </div>

      <!-- 快速操作 -->
      <div class="info-section">
        <h3>快速操作</h3>
        <div class="action-list">
          <el-button type="primary" @click="openProject">
            <el-icon><View /></el-icon>
            打开项目
          </el-button>
          <el-button v-if="item?.url" @click="openExternal">
            <el-icon><Link /></el-icon>
            新窗口打开
          </el-button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useMenuStore } from '@/stores/menu'
import { getTypeConfig } from '@/utils/menu'

const route = useRoute()
const router = useRouter()
const menuStore = useMenuStore()

// 当前项目
const item = computed(() => menuStore.findItemById(route.params.id))

// 返回
function goBack() {
  router.back()
}

// 打开项目
function openProject() {
  if (item.value) {
    router.push(`/content/${item.value.type}/${item.value.id}`)
  }
}

// 在新窗口打开
function openExternal() {
  if (item.value?.url) {
    window.open(item.value.url, '_blank', 'noopener,noreferrer')
  }
}

// 打开 URL
function openUrl(url) {
  window.open(url, '_blank', 'noopener,noreferrer')
}

// 加载菜单配置
onMounted(() => {
  if (menuStore.groups.length === 0) {
    menuStore.loadMenuConfig()
  }
})
</script>

<style lang="scss" scoped>
.project-detail-page {
  max-width: 800px;
}

.page-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 24px;

  h2 {
    font-size: 24px;
    font-weight: 600;
    color: var(--text-primary);
  }
}

.info-section {
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  padding: 24px;
  margin-bottom: 24px;

  h3 {
    font-size: 16px;
    font-weight: 600;
    color: var(--text-primary);
    margin-bottom: 16px;
  }
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.info-item {
  display: flex;
  align-items: center;

  &.full {
    grid-column: 1 / -1;
  }

  .label {
    width: 80px;
    font-size: var(--font-size-sm);
    color: var(--text-tertiary);
  }

  .value {
    flex: 1;
    font-size: var(--font-size-base);
    color: var(--text-primary);

    &.icon-value {
      .detail-logo-img {
        width: 32px;
        height: 32px;
        object-fit: contain;
      }
    }

    &.link {
      color: var(--primary-color);
      cursor: pointer;

      &:hover {
        text-decoration: underline;
      }
    }
  }
}

.action-list {
  display: flex;
  gap: 12px;
}
</style>

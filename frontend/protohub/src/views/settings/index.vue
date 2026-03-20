<template>
  <div class="settings-page">
    <!-- 页面头部 -->
    <div class="page-header">
      <h2>系统设置</h2>
      <HelpBubble
        title="设置帮助"
        :items="[
          { content: '配置系统显示和行为' },
          { content: '修改后需点击保存生效' }
        ]"
      />
    </div>

    <!-- 设置内容 -->
    <div class="settings-content">
      <!-- 外观设置 -->
      <div class="settings-section">
        <h3>外观设置</h3>

        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-label">主题模式</span>
            <span class="setting-desc">切换亮色/暗色主题</span>
          </div>
          <el-radio-group v-model="settings.theme" @change="handleThemeChange">
            <el-radio-button label="light">亮色</el-radio-button>
            <el-radio-button label="dark">暗色</el-radio-button>
          </el-radio-group>
        </div>

        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-label">侧边栏默认状态</span>
            <span class="setting-desc">页面加载时侧边栏的展开状态</span>
          </div>
          <el-switch v-model="settings.sidebarExpanded" />
        </div>
      </div>

      <!-- 菜单设置 -->
      <div class="settings-section">
        <h3>菜单设置</h3>

        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-label">重置菜单配置</span>
            <span class="setting-desc">将菜单配置恢复为默认值</span>
          </div>
          <el-button type="danger" plain @click="resetMenuConfig">
            重置菜单
          </el-button>
        </div>

        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-label">导出菜单配置</span>
            <span class="setting-desc">将当前菜单配置导出为 JSON 文件</span>
          </div>
          <el-button @click="exportMenuConfig">
            导出配置
          </el-button>
        </div>
      </div>

      <!-- 用户信息 -->
      <div class="settings-section">
        <h3>用户信息</h3>

        <div class="user-info-card">
          <el-avatar :size="64" class="user-avatar">
            {{ userStore.nickname?.charAt(0)?.toUpperCase() || 'U' }}
          </el-avatar>
          <div class="user-details">
            <h4>{{ userStore.nickname }}</h4>
            <p>@{{ userStore.username }}</p>
            <el-tag size="small">{{ userStore.role === 'admin' ? '管理员' : '操作员' }}</el-tag>
          </div>
        </div>
      </div>

      <!-- 关于 -->
      <div class="settings-section">
        <h3>关于</h3>

        <div class="about-card">
          <div class="about-row">
            <span class="about-label">版本</span>
            <span class="about-value">v1.0.0</span>
          </div>
          <div class="about-row">
            <span class="about-label">技术栈</span>
            <span class="about-value">Vue 3.4 + Vite 5 + Element Plus 2</span>
          </div>
          <div class="about-row">
            <span class="about-label">设计风格</span>
            <span class="about-value">Apple Design Guidelines</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { useUserStore } from '@/stores/user'
import { useMenuStore } from '@/stores/menu'
import { ElMessage, ElMessageBox } from 'element-plus'
import HelpBubble from '@/components/HelpBubble/index.vue'

const appStore = useAppStore()
const userStore = useUserStore()
const menuStore = useMenuStore()

const settings = reactive({
  theme: 'light',
  sidebarExpanded: true
})

// 初始化设置
onMounted(() => {
  settings.theme = appStore.theme
  settings.sidebarExpanded = !appStore.sidebarCollapsed
})

// 处理主题变更
function handleThemeChange(theme) {
  appStore.setTheme(theme)
  ElMessage.success('主题已切换')
}

// 重置菜单配置
async function resetMenuConfig() {
  try {
    await ElMessageBox.confirm('确定要重置菜单配置吗？当前修改将丢失。', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    menuStore.resetToDefault()
    ElMessage.success('菜单配置已重置')
  } catch {
    // 取消操作
  }
}

// 导出菜单配置
function exportMenuConfig() {
  const config = JSON.stringify(menuStore.menuConfig, null, 2)
  const blob = new Blob([config], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = 'menus.json'
  a.click()
  URL.revokeObjectURL(url)
  ElMessage.success('配置已导出')
}
</script>

<style lang="scss" scoped>
.settings-page {
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

.settings-section {
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  padding: 24px;
  margin-bottom: 24px;

  h3 {
    font-size: 16px;
    font-weight: 600;
    color: var(--text-primary);
    margin-bottom: 20px;
  }
}

.setting-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 0;
  border-bottom: 1px solid var(--border-light);

  &:last-child {
    border-bottom: none;
  }

  .setting-info {
    .setting-label {
      display: block;
      font-size: var(--font-size-base);
      font-weight: 500;
      color: var(--text-primary);
      margin-bottom: 4px;
    }

    .setting-desc {
      font-size: var(--font-size-sm);
      color: var(--text-tertiary);
    }
  }
}

.user-info-card {
  display: flex;
  align-items: center;
  gap: 20px;
  padding: 20px;
  background: var(--bg-secondary);
  border-radius: var(--radius-md);

  .user-avatar {
    background: var(--primary-color);
    color: white;
    font-size: 24px;
    font-weight: 500;
  }

  .user-details {
    h4 {
      font-size: 18px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 4px;
    }

    p {
      font-size: var(--font-size-sm);
      color: var(--text-tertiary);
      margin-bottom: 8px;
    }
  }
}

.about-card {
  padding: 4px 0;
}

.about-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 0;
  border-bottom: 1px solid var(--border-light);

  &:last-child {
    border-bottom: none;
  }

  .about-label {
    color: var(--text-secondary);
  }

  .about-value {
    color: var(--text-primary);
    font-weight: 500;
  }
}
</style>

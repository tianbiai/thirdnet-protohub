<template>
  <ManagePageLayout title="系统设置" subtitle="配置系统外观和行为">
    <template #actions>
      <HelpBubble
        title="设置帮助"
        :items="[
          { content: '配置系统显示和行为' },
          { content: '修改后立即生效，无需保存' }
        ]"
      />
    </template>

    <!-- 设置内容 -->
    <div class="settings-content">
      <!-- 用户信息卡 -->
      <div class="settings-section user-profile-section">
        <div class="user-profile-card">
          <div class="profile-left">
            <el-avatar :size="64" class="user-avatar">
              {{ userStore.nickname?.charAt(0)?.toUpperCase() || 'U' }}
            </el-avatar>
            <div class="profile-info">
              <h3 class="profile-name">{{ userStore.nickname }}</h3>
              <span class="profile-username">@{{ userStore.username }}</span>
              <div class="profile-roles">
                <el-tag v-for="role in userStore.roleNames" :key="role" size="small" effect="light">
                  {{ role }}
                </el-tag>
              </div>
            </div>
          </div>
          <div class="profile-meta">
            <div class="meta-item">
              <span class="meta-label">用户 ID</span>
              <span class="meta-value">{{ userStore.userInfo?.id || '-' }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 外观设置 -->
      <div class="settings-section">
        <div class="section-header">
          <el-icon :size="18" class="section-icon"><Sunny /></el-icon>
          <h3>外观设置</h3>
        </div>

        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-label">主题模式</span>
            <span class="setting-desc">切换亮色或暗色主题</span>
          </div>
          <el-radio-group v-model="settings.theme" @change="handleThemeChange" size="small">
            <el-radio-button label="light">
              <el-icon style="margin-right: 4px"><Sunny /></el-icon>亮色
            </el-radio-button>
            <el-radio-button label="dark">
              <el-icon style="margin-right: 4px"><Moon /></el-icon>暗色
            </el-radio-button>
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
        <div class="section-header">
          <el-icon :size="18" class="section-icon"><Menu /></el-icon>
          <h3>菜单设置</h3>
        </div>

        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-label">重置菜单配置</span>
            <span class="setting-desc">将菜单配置恢复为默认值</span>
          </div>
          <el-button type="danger" plain size="small" @click="resetMenuConfig">
            重置菜单
          </el-button>
        </div>

        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-label">导出菜单配置</span>
            <span class="setting-desc">将当前菜单配置导出为 JSON 文件</span>
          </div>
          <el-button size="small" @click="exportMenuConfig">
            导出配置
          </el-button>
        </div>
      </div>

      <!-- 关于 -->
      <div class="settings-section">
        <div class="section-header">
          <el-icon :size="18" class="section-icon"><InfoFilled /></el-icon>
          <h3>关于</h3>
        </div>

        <div class="about-list">
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
            <span class="about-value">Apple Design + Glassmorphism</span>
          </div>
        </div>
      </div>
    </div>
  </ManagePageLayout>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { useUserStore } from '@/stores/user'
import { useMenuStore } from '@/stores/menu'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Sunny, Moon, Menu, InfoFilled } from '@element-plus/icons-vue'
import ManagePageLayout from '@/components/ManagePageLayout/index.vue'
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
    menuStore.resetMenu()
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
.settings-content {
  max-width: 720px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.settings-section {
  background: var(--bg-primary);
  border-radius: var(--radius-xl);
  border: 1px solid var(--border-light);
  padding: 24px;
  transition: var(--transition-fast);

  &:hover {
    box-shadow: var(--shadow-sm);
  }

  .section-header {
    display: flex;
    align-items: center;
    gap: 10px;
    margin-bottom: 20px;
    padding-bottom: 16px;
    border-bottom: 1px solid var(--border-light);

    .section-icon {
      color: var(--primary-color);
    }

    h3 {
      font-size: var(--font-size-md);
      font-weight: var(--font-weight-semibold);
      color: var(--text-primary);
      margin: 0;
    }
  }
}

// 用户信息卡
.user-profile-section {
  background: linear-gradient(135deg, var(--bg-primary), var(--bg-secondary));
}

.user-profile-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;

  .profile-left {
    display: flex;
    align-items: center;
    gap: 20px;
  }

  .user-avatar {
    background: linear-gradient(135deg, var(--primary-color), var(--primary-600));
    color: white;
    font-size: 24px;
    font-weight: var(--font-weight-semibold);
    box-shadow: var(--shadow-primary);
    flex-shrink: 0;
  }

  .profile-info {
    .profile-name {
      font-size: var(--font-size-xl);
      font-weight: var(--font-weight-bold);
      color: var(--text-primary);
      margin: 0 0 4px;
      letter-spacing: -0.02em;
    }

    .profile-username {
      font-size: var(--font-size-sm);
      color: var(--text-tertiary);
      display: block;
      margin-bottom: 10px;
    }

    .profile-roles {
      display: flex;
      gap: 6px;
      flex-wrap: wrap;
    }
  }

  .profile-meta {
    .meta-item {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 2px;

      .meta-label {
        font-size: var(--font-size-xs);
        color: var(--text-quaternary);
      }

      .meta-value {
        font-size: var(--font-size-sm);
        font-weight: var(--font-weight-medium);
        color: var(--text-secondary);
        font-family: var(--font-mono);
      }
    }
  }
}

// 设置项
.setting-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 0;
  border-bottom: 1px solid var(--border-lighter);

  &:last-child {
    border-bottom: none;
    padding-bottom: 0;
  }

  &:first-of-type {
    padding-top: 0;
  }

  .setting-info {
    .setting-label {
      display: block;
      font-size: var(--font-size-base);
      font-weight: var(--font-weight-medium);
      color: var(--text-primary);
      margin-bottom: 4px;
    }

    .setting-desc {
      font-size: var(--font-size-sm);
      color: var(--text-tertiary);
    }
  }
}

// 关于
.about-list {
  .about-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 14px 0;
    border-bottom: 1px solid var(--border-lighter);

    &:last-child {
      border-bottom: none;
      padding-bottom: 0;
    }

    &:first-child {
      padding-top: 0;
    }

    .about-label {
      font-size: var(--font-size-sm);
      color: var(--text-secondary);
    }

    .about-value {
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-medium);
      color: var(--text-primary);
      font-family: var(--font-mono);
    }
  }
}

// 深色主题
[data-theme="dark"] {
  .settings-section {
    background: var(--bg-secondary);
    border-color: var(--border-light);
  }

  .user-profile-section {
    background: linear-gradient(135deg, var(--bg-secondary), var(--bg-tertiary));
  }
}

// 响应式
@media (max-width: 600px) {
  .user-profile-card {
    flex-direction: column;
    align-items: flex-start;

    .profile-meta {
      .meta-item {
        align-items: flex-start;
      }
    }
  }
}
</style>

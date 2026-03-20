<template>
  <header class="header">
    <div class="header-left">
      <!-- 面包屑 -->
      <el-breadcrumb separator="/">
        <el-breadcrumb-item :to="{ path: '/' }">
          <el-icon><HomeFilled /></el-icon>
        </el-breadcrumb-item>
        <el-breadcrumb-item v-for="item in breadcrumbs" :key="item.path">
          {{ item.title }}
        </el-breadcrumb-item>
      </el-breadcrumb>
    </div>

    <div class="header-right">
      <!-- 帮助按钮 -->
      <el-tooltip content="帮助" placement="bottom">
        <div class="header-btn" @click="showHelp">
          <el-icon :size="20"><QuestionFilled /></el-icon>
        </div>
      </el-tooltip>

      <!-- 主题切换 -->
      <el-tooltip :content="appStore.isDarkTheme ? '切换亮色主题' : '切换暗色主题'" placement="bottom">
        <div class="header-btn" @click="appStore.toggleTheme">
          <el-icon :size="20">
            <component :is="appStore.isDarkTheme ? 'Sunny' : 'Moon'" />
          </el-icon>
        </div>
      </el-tooltip>

      <!-- 用户头像（点击退出登录） -->
      <el-tooltip content="退出登录" placement="bottom">
        <div class="user-avatar-btn" @click="handleLogout">
          <el-avatar :size="32" class="avatar">
            {{ userStore.nickname?.charAt(0)?.toUpperCase() || 'U' }}
          </el-avatar>
        </div>
      </el-tooltip>
    </div>

    <!-- 帮助抽屉 -->
    <el-drawer
      v-model="helpVisible"
      title="帮助说明"
      direction="rtl"
      size="400px"
    >
      <div class="help-content">
        <h3>项目管理聚合基座</h3>
        <p>统一管理多个前端项目的聚合平台，提供项目预览、文档查看、菜单管理等功能。</p>

        <h4>核心功能</h4>
        <ul>
          <li><strong>项目聚合</strong>：通过 iframe 加载多个子项目，统一管理</li>
          <li><strong>文档查看</strong>：支持查看项目的 spec.md、changelog.md 等文档</li>
          <li><strong>菜单编辑</strong>：可视化编辑菜单配置，实时预览效果</li>
          <li><strong>移动端模拟</strong>：模拟手机外观展示移动端项目</li>
        </ul>

        <h4>快捷键</h4>
        <ul>
          <li><kbd>Ctrl/Cmd</kbd> + <kbd>B</kbd>：切换侧边栏</li>
        </ul>
      </div>
    </el-drawer>
  </header>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { useUserStore } from '@/stores/user'
import { useLogout } from '@/composables/useLogout'

import { QuestionFilled, HomeFilled, Sunny, Moon } from '@element-plus/icons-vue'

import HelpBubble from '@/components/HelpBubble/index.vue'

const route = useRoute()
const appStore = useAppStore()
const userStore = useUserStore()
const { handleLogout } = useLogout()

const helpVisible = ref(false)

// 面包屑
const breadcrumbs = computed(() => {
  const matched = route.matched.filter(item => item.meta?.title)
  return matched.map(item => ({
    path: item.path,
    title: item.meta.title
  })).slice(1) // 去掉首页
})

// 显示帮助
function showHelp() {
  helpVisible.value = true
}
</script>

<style lang="scss" scoped>
.header {
  height: var(--topbar-height);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  background: var(--bg-primary);
  border-bottom: 1px solid var(--border-light);
}

.header-left {
  display: flex;
  align-items: center;

  :deep(.el-breadcrumb) {
    font-size: var(--font-size-sm);

    .el-breadcrumb__item {
      display: flex;
      align-items: center;
    }

    .el-breadcrumb__inner {
      display: flex;
      align-items: center;
      color: var(--text-secondary);

      &.is-link:hover {
        color: var(--primary-color);
      }
    }
  }
}

.header-right {
  display: flex;
  align-items: center;
  gap: 8px;
}

.header-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  cursor: pointer;
  color: var(--text-secondary);
  transition: all 0.2s;

  &:hover {
    background: var(--bg-hover);
    color: var(--text-primary);
  }
}

.user-avatar-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 4px;
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: background-color 0.2s;

  &:hover {
    background: var(--bg-hover);
  }

  .avatar {
    background: var(--primary-color);
    color: white;
    font-weight: 500;
  }
}

.help-content {
  h3 {
    font-size: 18px;
    font-weight: 600;
    margin-bottom: 12px;
    color: var(--text-primary);
  }

  h4 {
    font-size: 15px;
    font-weight: 500;
    margin: 24px 0 12px;
    color: var(--text-primary);
  }

  p {
    line-height: 1.6;
    color: var(--text-secondary);
  }

  ul {
    padding-left: 20px;

    li {
      margin-bottom: 8px;
      line-height: 1.6;
      color: var(--text-secondary);

      strong {
        color: var(--text-primary);
      }
    }
  }

  kbd {
    display: inline-block;
    padding: 2px 6px;
    font-size: 12px;
    font-family: var(--font-mono);
    background: var(--bg-tertiary);
    border: 1px solid var(--border-color);
    border-radius: 4px;
  }
}
</style>

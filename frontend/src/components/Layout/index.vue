<template>
  <div class="layout" :class="{ 'sidebar-collapsed': appStore.sidebarCollapsed }">
    <!-- 背景装饰 -->
    <div class="background-decoration">
      <div class="gradient-orb orb-1"></div>
      <div class="gradient-orb orb-2"></div>
      <div class="grid-pattern"></div>
    </div>

    <!-- 侧边栏 -->
    <Sidebar />

    <!-- 主内容区 -->
    <div class="main-container">
      <!-- 内容区 -->
      <main class="content-wrapper">
        <div class="content">
          <router-view v-slot="{ Component, route }">
            <transition name="page-fade" mode="out-in">
              <div :key="route.path" class="page-container">
                <component :is="Component" />
              </div>
            </transition>
          </router-view>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue'
import { useAppStore } from '@/stores/app'
import { useMenuStore } from '@/stores/menu'
import Sidebar from './Sidebar.vue'

const appStore = useAppStore()
const menuStore = useMenuStore()

// 键盘快捷键
function handleKeydown(e) {
  // Ctrl/Cmd + B 切换侧边栏
  if ((e.ctrlKey || e.metaKey) && e.key === 'b') {
    e.preventDefault()
    appStore.toggleSidebar()
  }
}

onMounted(async () => {
  appStore.initTheme()
  await menuStore.loadMenuConfig()
  window.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown)
})
</script>

<style lang="scss" scoped>
.layout {
  position: relative;
  display: flex;
  width: 100%;
  height: 100vh;
  overflow: hidden;
  background: var(--bg-secondary);
}

// 背景装饰
.background-decoration {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
  z-index: 0;
  overflow: hidden;
}

.gradient-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  opacity: 0.4;
  animation: float 20s ease-in-out infinite;

  &.orb-1 {
    width: 600px;
    height: 600px;
    background: linear-gradient(135deg, var(--primary-100), var(--primary-200));
    top: -200px;
    right: -100px;
    animation-delay: 0s;
  }

  &.orb-2 {
    width: 400px;
    height: 400px;
    background: linear-gradient(135deg, var(--info-light), var(--primary-100));
    bottom: -100px;
    left: 30%;
    animation-delay: -10s;
  }
}

.grid-pattern {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-image:
    linear-gradient(rgba(0, 0, 0, 0.02) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0, 0, 0, 0.02) 1px, transparent 1px);
  background-size: 40px 40px;
}

[data-theme="dark"] {
  .gradient-orb {
    opacity: 0.15;

    &.orb-1 {
      background: linear-gradient(135deg, var(--primary-600), var(--primary-800));
    }

    &.orb-2 {
      background: linear-gradient(135deg, var(--primary-700), var(--info-color));
    }
  }

  .grid-pattern {
    background-image:
      linear-gradient(rgba(255, 255, 255, 0.02) 1px, transparent 1px),
      linear-gradient(90deg, rgba(255, 255, 255, 0.02) 1px, transparent 1px);
  }
}

// 主容器
.main-container {
  position: relative;
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
  margin-left: var(--sidebar-width);
  transition: margin-left var(--duration-slow) var(--ease-out);
  z-index: 1;

  .sidebar-collapsed & {
    margin-left: var(--sidebar-collapsed-width);
  }
}

.content-wrapper {
  flex: 1;
  overflow: hidden;
  position: relative;
}

.content {
  height: 100%;
  padding: 24px;
  overflow-y: auto;
  overflow-x: hidden;
}

// 页面容器
.page-container {
  height: 100%;
  animation: pageEnter var(--duration-normal) var(--ease-out);
}

// 页面切换动画
.page-fade-enter-active {
  transition: all var(--duration-normal) var(--ease-out);
}

.page-fade-leave-active {
  transition: all var(--duration-fast) var(--ease-out);
}

.page-fade-enter-from {
  opacity: 0;
  transform: translateY(12px);
}

.page-fade-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

@keyframes pageEnter {
  from {
    opacity: 0;
    transform: translateY(12px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes float {
  0%, 100% {
    transform: translate(0, 0) rotate(0deg);
  }
  25% {
    transform: translate(20px, -20px) rotate(5deg);
  }
  50% {
    transform: translate(-10px, 20px) rotate(-5deg);
  }
  75% {
    transform: translate(-20px, -10px) rotate(3deg);
  }
}

// 响应式
@media (max-width: 768px) {
  .content {
    padding: 16px;
  }

  .gradient-orb {
    display: none;
  }
}
</style>

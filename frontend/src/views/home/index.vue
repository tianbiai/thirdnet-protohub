<template>
  <div class="home-page">
    <!-- Hero 欢迎区域 -->
    <section class="welcome-hero">
      <div class="ambient-bg"></div>
      <div class="welcome-content">
        <div class="welcome-badge">
          <div class="badge-dot"></div>
          <span>管理控制台</span>
        </div>
        <h1 class="welcome-title">
          你好，<span class="user-name">{{ userStore.nickname }}</span>
        </h1>
        <p class="welcome-desc">欢迎使用 ProtoHub 原型视界，在这里管理你的项目和系统配置。</p>
      </div>
    </section>

    <!-- 统计概览 -->
    <section v-if="homeCards.length" class="stats-section">
      <div
        v-for="(card, index) in homeCards"
        :key="card.permission"
        class="stat-card"
        :class="card.iconClass"
        tabindex="0"
        role="link"
        :style="{ animationDelay: `${index * 60}ms` }"
        @click="$router.push(card.path)"
        @keydown.enter="$router.push(card.path)"
      >
        <div class="stat-icon-wrapper">
          <div class="stat-icon-bg"></div>
          <el-icon :size="22"><component :is="card.icon" /></el-icon>
        </div>
        <div class="stat-content">
          <h4 class="stat-title">{{ card.title }}</h4>
          <p class="stat-desc">{{ card.desc }}</p>
        </div>
        <div class="stat-arrow">
          <el-icon :size="16"><ArrowRight /></el-icon>
        </div>
      </div>
    </section>

    <!-- 快速操作 -->
    <section v-if="quickLinks.length" class="quick-links-section">
      <h3 class="section-title">
        <el-icon :size="18" class="section-icon"><Promotion /></el-icon>
        快速访问
      </h3>
      <div class="quick-links-grid">
        <div
          v-for="(link, index) in quickLinks"
          :key="link.path"
          class="quick-link-card"
          :style="{ animationDelay: `${(index + homeCards.length) * 60}ms` }"
          @click="$router.push(link.path)"
        >
          <span class="quick-link-name">{{ link.name }}</span>
          <el-icon :size="14" class="quick-link-arrow"><ArrowRight /></el-icon>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useUserStore } from '@/stores/user'
import { useMenuStore } from '@/stores/menu'
import { Folder, User, UserFilled, Lock, Menu, Setting, ArrowRight, Promotion } from '@element-plus/icons-vue'

const userStore = useUserStore()
const menuStore = useMenuStore()

const allCards = [
  { permission: 'projects:view', path: '/projects', icon: Folder, iconClass: 'project-card', title: '项目管理', desc: '管理项目、子项和成员' },
  { permission: 'user-manage:view', path: '/system/users', icon: User, iconClass: 'user-card', title: '用户管理', desc: '管理系统用户账号' },
  { permission: 'role-manage:view', path: '/system/roles', icon: UserFilled, iconClass: 'role-card', title: '角色管理', desc: '配置角色和权限' },
  { permission: 'permission-manage:view', path: '/system/permissions', icon: Lock, iconClass: 'permission-card', title: '权限管理', desc: '查看系统权限' },
  { permission: 'system-menu-manage:view', path: '/system/menus', icon: Menu, iconClass: 'menu-card', title: '系统菜单', desc: '配置功能菜单' },
  { permission: 'settings', path: '/settings', icon: Setting, iconClass: 'settings-card', title: '系统设置', desc: '个性化配置' },
]

const homeCards = computed(() => allCards.filter(card => userStore.hasPermission(card.permission)))

// 从菜单配置中提取快速链接
const quickLinks = computed(() => {
  const links = []
  for (const group of menuStore.groups) {
    if (group.children) {
      for (const item of group.children) {
        links.push({
          name: item.name,
          path: `/content/${item.type}/${item.id}`,
          type: item.type
        })
      }
    }
  }
  return links.slice(0, 8) // 最多展示8个
})
</script>

<style lang="scss" scoped>
.home-page {
  display: flex;
  flex-direction: column;
  gap: 28px;
  min-height: 100%;
  animation: pageEnter var(--duration-slow) var(--ease-out);
}

@keyframes pageEnter {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
}

// ===== Hero 区域 =====
.welcome-hero {
  position: relative;
  padding: 40px 36px 36px;
  border-radius: var(--radius-2xl);
  background: var(--bg-primary);
  border: 1px solid var(--border-light);
  overflow: hidden;
}

.ambient-bg {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse 50% 80% at 10% 0%, var(--primary-light), transparent 60%),
    radial-gradient(ellipse 40% 60% at 90% 100%, var(--info-light), transparent 60%);
  opacity: 0.6;
  pointer-events: none;
}

.welcome-content {
  position: relative;
  z-index: 1;
}

.welcome-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 6px 14px;
  background: var(--bg-secondary);
  border: 1px solid var(--border-light);
  border-radius: var(--radius-full);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  color: var(--text-secondary);
  margin-bottom: 20px;

  .badge-dot {
    width: 7px;
    height: 7px;
    border-radius: 50%;
    background: var(--success-color);
    box-shadow: 0 0 8px var(--success-color);
    animation: pulse 2s ease-in-out infinite;
  }
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.welcome-title {
  font-size: var(--font-size-3xl);
  font-weight: var(--font-weight-bold);
  color: var(--text-primary);
  margin: 0 0 10px;
  letter-spacing: -0.03em;
  line-height: 1.2;

  .user-name {
    background: linear-gradient(135deg, var(--primary-color), var(--primary-600));
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
  }
}

.welcome-desc {
  font-size: var(--font-size-base);
  color: var(--text-secondary);
  margin: 0;
  max-width: 480px;
  line-height: 1.7;
}

// ===== 统计卡片网格 =====
.stats-section {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 16px;
}

.stat-card {
  position: relative;
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px 22px;
  background: var(--bg-primary);
  border-radius: var(--radius-xl);
  border: 1px solid var(--border-light);
  cursor: pointer;
  transition: transform 0.2s ease-out, box-shadow 0.2s ease-out, border-color 0.2s ease-out;
  outline: none;
  animation: cardEnter 0.4s var(--ease-out) both;

  &:hover,
  &:focus-visible {
    transform: translateY(-2px);
    box-shadow: var(--shadow-md);
    border-color: var(--primary-light);

    .stat-arrow {
      opacity: 1;
      transform: translateX(0);
      color: var(--primary-color);
    }
  }

  &:focus-visible {
    box-shadow: 0 0 0 2px var(--primary-light);
  }

  &:active {
    transform: translateY(0) scale(0.99);
    box-shadow: var(--shadow-sm);
  }
}

@keyframes cardEnter {
  from {
    opacity: 0;
    transform: translateY(12px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.stat-icon-wrapper {
  position: relative;
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-lg);
  flex-shrink: 0;

  .stat-icon-bg {
    position: absolute;
    inset: 0;
    border-radius: inherit;
    opacity: 0.12;
  }

  .el-icon {
    position: relative;
    z-index: 1;
  }

  .project-card & {
    color: var(--primary-color);
    .stat-icon-bg { background: var(--primary-color); }
  }
  .user-card & {
    color: var(--info-color);
    .stat-icon-bg { background: var(--info-color); }
  }
  .role-card & {
    color: var(--success-color);
    .stat-icon-bg { background: var(--success-color); }
  }
  .permission-card & {
    color: var(--warning-color);
    .stat-icon-bg { background: var(--warning-color); }
  }
  .menu-card & {
    color: #8B5CF6;
    .stat-icon-bg { background: #8B5CF6; }
  }
  .settings-card & {
    color: var(--text-secondary);
    .stat-icon-bg { background: var(--text-secondary); }
  }
}

.stat-content {
  flex: 1;
  min-width: 0;

  .stat-title {
    font-size: var(--font-size-md);
    font-weight: var(--font-weight-semibold);
    color: var(--text-primary);
    margin: 0 0 4px;
  }

  .stat-desc {
    font-size: var(--font-size-sm);
    color: var(--text-tertiary);
    margin: 0;
    line-height: 1.4;
  }
}

.stat-arrow {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: var(--radius-md);
  background: var(--bg-secondary);
  color: var(--text-quaternary);
  flex-shrink: 0;
  opacity: 0;
  transform: translateX(-4px);
  transition: all 0.2s ease-out;
}

// ===== 快速访问 =====
.quick-links-section {
  .section-title {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: var(--font-size-base);
    font-weight: var(--font-weight-semibold);
    color: var(--text-secondary);
    margin: 0 0 16px;

    .section-icon {
      color: var(--primary-color);
    }
  }
}

.quick-links-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 10px;
}

.quick-link-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-light);
  cursor: pointer;
  transition: var(--transition-fast);
  animation: cardEnter 0.4s var(--ease-out) both;

  &:hover {
    background: var(--bg-hover);
    border-color: var(--primary-light);
    transform: translateX(4px);

    .quick-link-arrow {
      color: var(--primary-color);
      opacity: 1;
    }
  }

  &:active {
    transform: translateX(2px) scale(0.99);
  }

  .quick-link-name {
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: var(--text-primary);
  }

  .quick-link-arrow {
    color: var(--text-quaternary);
    opacity: 0;
    transition: var(--transition-fast);
  }
}

// ===== 深色主题 =====
[data-theme="dark"] {
  .welcome-hero {
    background: var(--bg-secondary);
  }

  .ambient-bg {
    opacity: 0.25;
  }

  .stat-card {
    background: var(--bg-secondary);
    border-color: var(--border-light);

    &:hover {
      background: var(--bg-tertiary);
    }
  }

  .quick-link-card {
    background: var(--bg-secondary);
    border-color: var(--border-light);

    &:hover {
      background: var(--bg-tertiary);
    }
  }
}

// ===== 尊重用户减少动画偏好 =====
@media (prefers-reduced-motion: reduce) {
  .home-page,
  .stat-card,
  .quick-link-card {
    animation: none;
  }

  .welcome-badge .badge-dot {
    animation: none;
  }

  .stat-card,
  .quick-link-card {
    transition: none;
  }
}

// ===== 响应式 =====
@media (max-width: 768px) {
  .welcome-hero {
    padding: 28px 20px;
  }

  .welcome-title {
    font-size: var(--font-size-2xl);
  }

  .stats-section {
    grid-template-columns: 1fr;
    gap: 12px;
  }

  .quick-links-grid {
    grid-template-columns: 1fr;
  }
}
</style>

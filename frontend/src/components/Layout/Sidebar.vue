<template>
  <aside class="sidebar" :class="{ 'is-collapsed': appStore.sidebarCollapsed }">
    <!-- Logo 区域 -->
    <div class="sidebar-header" @click="goHome">
      <div class="logo">
        <img src="@/assets/logo.png" alt="ProtoHub Logo" class="logo-img" />
      </div>
      <transition name="fade">
        <span v-if="!appStore.sidebarCollapsed" class="logo-text">ProtoHub</span>
      </transition>
    </div>

    <!-- 菜单列表 -->
    <el-scrollbar class="sidebar-menu">
      <!-- 动态项目菜单 -->
      <div class="menu-list">
        <template v-for="(group, groupIndex) in menuStore.groups" :key="group.id">
          <!-- 分组 -->
          <div
            class="menu-group"
            :class="{ 'is-expanded': group.expanded }"
            :style="{ animationDelay: `${groupIndex * 50}ms` }"
          >
            <div
              class="group-header"
              @click="toggleGroup(group)"
            >
              <span class="group-icon"><img src="@/assets/logo.png" alt="" class="group-logo-img" /></span>
              <transition name="fade">
                <span v-if="!appStore.sidebarCollapsed" class="group-name">{{ group.name }}</span>
              </transition>
              <el-icon
                v-if="!appStore.sidebarCollapsed && group.children?.length"
                class="expand-icon"
                :class="{ 'is-expanded': group.expanded }"
              >
                <ArrowRight />
              </el-icon>
            </div>

            <!-- 菜单项 -->
            <transition name="slide">
              <div
                v-if="group.expanded && !appStore.sidebarCollapsed"
                class="group-items"
              >
                <div
                  v-for="(item, itemIndex) in group.children"
                  :key="item.id"
                  class="menu-item"
                  :class="{ 'is-active': String(activeItemId) === String(item.id) }"
                  :style="{ animationDelay: `${itemIndex * 30}ms` }"
                  @click="handleItemClick(item)"
                >
                  <TypeIcon :type="item.type" />
                  <span class="item-name">{{ item.name }}</span>
                </div>
              </div>
            </transition>
          </div>
        </template>
      </div>
    </el-scrollbar>

    <!-- 底部区域 -->
    <div class="sidebar-footer">
      <!-- 用户头像 -->
      <el-popover
        ref="userPopoverRef"
        :width="160"
        placement="right-start"
        trigger="click"
        :teleported="true"
        :popper-style="{ padding: '4px' }"
      >
        <template #reference>
          <div class="user-section">
            <el-avatar :size="34" class="user-avatar">
              {{ userStore.nickname?.charAt(0)?.toUpperCase() || 'U' }}
            </el-avatar>
            <transition name="fade">
              <div v-if="!appStore.sidebarCollapsed" class="user-info">
                <span class="user-name">{{ userStore.nickname || '用户' }}</span>
                <span class="user-role">{{ userStore.roleNames.join('、') || '访客' }}</span>
              </div>
            </transition>
          </div>
        </template>
        <div class="user-menu-list">
          <div class="user-menu-item" @click="openChangePassword">
            <el-icon><Lock /></el-icon><span>修改密码</span>
          </div>
          <div class="user-menu-divider" />
          <div class="user-menu-item danger" @click="handleLogoutClick">
            <el-icon><SwitchButton /></el-icon><span>退出登录</span>
          </div>
        </div>
      </el-popover>

      <!-- 功能按钮 -->
      <div class="footer-actions">
        <el-tooltip :content="appStore.isDarkTheme ? '切换亮色' : '切换暗色'" placement="right">
          <div class="action-btn" @click="appStore.toggleTheme">
            <el-icon :size="18">
              <component :is="appStore.isDarkTheme ? 'Sunny' : 'Moon'" />
            </el-icon>
          </div>
        </el-tooltip>

        <el-tooltip content="帮助" placement="right">
          <div class="action-btn" @click="showHelp">
            <el-icon :size="18"><QuestionFilled /></el-icon>
          </div>
        </el-tooltip>

        <el-tooltip :content="appStore.sidebarCollapsed ? '展开' : '收起'" placement="right">
          <div class="action-btn" @click="appStore.toggleSidebar">
            <el-icon :size="18">
              <component :is="appStore.sidebarCollapsed ? 'Expand' : 'Fold'" />
            </el-icon>
          </div>
        </el-tooltip>
      </div>
    </div>

    <!-- 帮助抽屉 -->
    <el-drawer
      v-model="helpVisible"
      title="帮助说明"
      direction="rtl"
      size="400px"
    >
      <div class="help-content">
        <div class="help-header">
          <div class="help-icon">
            <img src="@/assets/logo.png" alt="ProtoHub Logo" class="help-logo-img" />
          </div>
          <h3>ProtoHub 原型视界</h3>
          <p>原型与文档的聚合视界</p>
        </div>

        <div class="help-section">
          <h4>核心功能</h4>
          <div class="feature-grid">
            <div class="feature-item">
              <span class="feature-icon">🎯</span>
              <span class="feature-text">项目聚合</span>
            </div>
            <div class="feature-item">
              <span class="feature-icon">📄</span>
              <span class="feature-text">文档查看</span>
            </div>
            <div class="feature-item">
              <span class="feature-icon">🎨</span>
              <span class="feature-text">菜单编辑</span>
            </div>
            <div class="feature-item">
              <span class="feature-icon">📱</span>
              <span class="feature-text">移动端模拟</span>
            </div>
          </div>
        </div>

        <div class="help-section">
          <h4>快捷键</h4>
          <div class="shortcut-list">
            <div class="shortcut-item">
              <kbd>Ctrl/Cmd</kbd> + <kbd>B</kbd>
              <span class="shortcut-desc">切换侧边栏</span>
            </div>
          </div>
        </div>
      </div>
    </el-drawer>
  </aside>

  <!-- 修改密码弹窗 -->
  <el-dialog v-model="passwordDialogVisible" title="修改密码" width="420px" @close="resetPasswordForm">
    <el-form ref="passwordFormRef" :model="passwordForm" :rules="passwordRules" label-width="80px">
      <el-form-item label="原密码" prop="oldPassword">
        <el-input v-model="passwordForm.oldPassword" type="password" show-password placeholder="请输入原密码" />
      </el-form-item>
      <el-form-item label="新密码" prop="newPassword">
        <el-input v-model="passwordForm.newPassword" type="password" show-password placeholder="请输入新密码" />
      </el-form-item>
      <el-form-item label="确认密码" prop="confirmPassword">
        <el-input v-model="passwordForm.confirmPassword" type="password" show-password placeholder="请再次输入新密码" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="passwordDialogVisible = false">取消</el-button>
      <el-button type="primary" :loading="passwordLoading" @click="submitChangePassword">确定</el-button>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useAppStore } from '@/stores/app'
import { useMenuStore } from '@/stores/menu'
import { useUserStore } from '@/stores/user'
import { useLogout } from '@/composables/useLogout'
import { changePassword } from '@/api/auth'
import { QuestionFilled, Sunny, Moon, ArrowRight, Expand, Fold, Lock, SwitchButton } from '@element-plus/icons-vue'
import TypeIcon from '@/components/TypeIcon/index.vue'

const router = useRouter()
const appStore = useAppStore()
const menuStore = useMenuStore()
const userStore = useUserStore()
const { handleLogout } = useLogout()

const activeItemId = ref(null)
const helpVisible = ref(false)

// 修改密码相关
const passwordDialogVisible = ref(false)
const passwordLoading = ref(false)
const passwordFormRef = ref(null)
const passwordForm = reactive({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const validateConfirmPassword = (rule, value, callback) => {
  if (value !== passwordForm.newPassword) {
    callback(new Error('两次输入的密码不一致'))
  } else {
    callback()
  }
}

const passwordRules = {
  oldPassword: [{ required: true, message: '请输入原密码', trigger: 'blur' }],
  newPassword: [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    { min: 6, message: '密码长度不能少于6个字符', trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, message: '请再次输入新密码', trigger: 'blur' },
    { validator: validateConfirmPassword, trigger: 'blur' }
  ]
}

const userPopoverRef = ref(null)

function openChangePassword() {
  userPopoverRef.value?.hide()
  passwordDialogVisible.value = true
}

function handleLogoutClick() {
  userPopoverRef.value?.hide()
  handleLogout()
}

function resetPasswordForm() {
  passwordForm.oldPassword = ''
  passwordForm.newPassword = ''
  passwordForm.confirmPassword = ''
  passwordFormRef.value?.resetFields()
}

async function submitChangePassword() {
  const formEl = passwordFormRef.value
  if (!formEl) return
  await formEl.validate()

  passwordLoading.value = true
  try {
    await changePassword(passwordForm.oldPassword, passwordForm.newPassword)
    ElMessage.success('密码修改成功，请重新登录')
    passwordDialogVisible.value = false
    // 修改密码后强制重新登录
    userStore.forceLogout()
    menuStore.resetMenu()
    router.push({ name: 'Login' })
  } catch (error) {
    ElMessage.error(error.message || '密码修改失败')
  } finally {
    passwordLoading.value = false
  }
}

// 切换分组展开状态
function toggleGroup(group) {
  if (appStore.sidebarCollapsed) {
    appStore.setSidebarCollapsed(false)
  }
  menuStore.toggleGroupExpanded(group.id)
}

// 处理菜单项点击
function handleItemClick(item) {
  activeItemId.value = item.id

  // 所有类型都通过 iframe 加载到内容查看页
  router.push(`/content/${item.type}/${item.id}`)
}

// 显示帮助
function showHelp() {
  helpVisible.value = true
}

// 回到首页
function goHome() {
  activeItemId.value = null
  router.push('/')
}

onMounted(() => {
  // 恢复展开状态
  menuStore.restoreExpandedState()
})
</script>

<style lang="scss" scoped>
.sidebar {
  position: fixed;
  top: 0;
  left: 0;
  width: var(--sidebar-width);
  height: 100vh;
  background: var(--sidebar-glass-bg);
  backdrop-filter: blur(var(--glass-blur));
  -webkit-backdrop-filter: blur(var(--glass-blur));
  border-right: 1px solid var(--sidebar-glass-border);
  display: flex;
  flex-direction: column;
  transition: width var(--duration-slow) var(--ease-out);
  z-index: var(--z-fixed);
  box-shadow: var(--shadow-lg);

  &.is-collapsed {
    width: var(--sidebar-collapsed-width);

    .group-name,
    .item-name,
    .expand-icon,
    .user-info {
      display: none;
    }

    .menu-item {
      justify-content: center;
      padding: 12px;
    }

    .user-section {
      justify-content: center;
    }
  }
}

// Logo 区域
.sidebar-header {
  height: var(--topbar-height);
  display: flex;
  align-items: center;
  padding: 0 16px;
  border-bottom: 1px solid var(--border-lighter);
  cursor: pointer;
  transition: var(--transition-fast);

  &:hover {
    background: var(--bg-hover);

    .logo {
      transform: scale(1.05);
    }
  }

  &:active {
    background: var(--primary-light);

    .logo {
      transform: scale(0.98);
    }
  }

  .logo {
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, var(--primary-light), transparent);
    border-radius: var(--radius-md);
    flex-shrink: 0;
    transition: transform var(--transition-spring);

    .logo-img {
      width: 28px;
      height: 28px;
      object-fit: contain;
    }
  }

  .logo-text {
    margin-left: 12px;
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-semibold);
    color: var(--text-primary);
    white-space: nowrap;
    letter-spacing: -0.02em;
  }
}

// 菜单区域
.sidebar-menu {
  flex: 1;
  overflow: hidden;
}

.menu-list {
  padding: 12px 10px;
}

// 菜单分组
.menu-group {
  margin-bottom: 6px;
  animation: fadeInUp var(--duration-slow) var(--ease-out) both;
}

.group-header {
  display: flex;
  align-items: center;
  padding: 10px 12px;
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: var(--transition-fast);

  &:hover {
    background: var(--bg-hover);
  }

  &:active {
    background: var(--bg-active);
    transform: scale(0.99);
  }

  .group-icon {
    width: 18px;
    height: 18px;
    flex-shrink: 0;
    transition: transform var(--transition-spring);

    .group-logo-img {
      width: 18px;
      height: 18px;
      object-fit: contain;
    }
  }

  &:hover .group-icon {
    transform: scale(1.1);
  }

  .group-name {
    flex: 1;
    margin-left: 10px;
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: var(--text-primary);
  }

  .expand-icon {
    color: var(--text-tertiary);
    transition: transform var(--duration-normal) var(--ease-spring);

    &.is-expanded {
      transform: rotate(90deg);
    }
  }
}

// 菜单项容器
.group-items {
  margin-top: 4px;
  padding-left: 18px;
}

// 菜单项
.menu-item {
  display: flex;
  align-items: center;
  padding: 10px 12px;
  margin-bottom: 2px;
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: var(--transition-fast);
  animation: fadeIn var(--duration-fast) var(--ease-out) both;

  &:hover {
    background: var(--bg-hover);

    .item-type-icon {
      transform: scale(1.1);
    }
  }

  &:active {
    background: var(--bg-active);
    transform: scale(0.98);
  }

  &.is-active {
    background: var(--primary-light);
    color: var(--primary-color);

    .item-name {
      color: var(--primary-color);
      font-weight: var(--font-weight-medium);
    }

    .item-type-icon {
      transform: scale(1.1);
    }
  }

  .item-type-icon {
    flex-shrink: 0;
  }

  .item-name {
    flex: 1;
    margin-left: 10px;
    font-size: var(--font-size-sm);
    color: var(--text-secondary);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .item-badge {
    flex-shrink: 0;
    font-size: 9px;
    font-weight: var(--font-weight-semibold);
    padding: 2px 6px;
    border-radius: var(--radius-xs);
    margin-left: 6px;
    letter-spacing: 0.02em;
    border: 1px solid;
    background: transparent;
  }
}

// 底部区域
.sidebar-footer {
  padding: 12px;
  border-top: 1px solid var(--border-lighter);
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.user-section {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px;
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: var(--transition-fast);
  outline: none;

  &:hover {
    background: var(--bg-hover);
  }

  &:active {
    background: var(--bg-active);
  }

  .user-avatar {
    background: linear-gradient(135deg, var(--primary-color), var(--primary-600));
    color: white;
    font-weight: var(--font-weight-semibold);
    flex-shrink: 0;
    box-shadow: var(--shadow-primary);
  }

  .user-info {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .user-name {
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: var(--text-primary);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .user-role {
    font-size: var(--font-size-xs);
    color: var(--text-tertiary);
  }
}

.footer-actions {
  display: flex;
  justify-content: space-around;
}

// 用户菜单
.user-menu-list {
  margin: -4px;
}

.user-menu-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-size: var(--font-size-sm);
  color: var(--text-primary);
  transition: background 0.2s;

  &:hover {
    background: var(--bg-hover);
  }

  &.danger {
    color: var(--el-color-danger);
  }
}

.user-menu-divider {
  height: 1px;
  background: var(--border-lighter);
  margin: 4px 0;
}

.action-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 38px;
  height: 38px;
  border-radius: var(--radius-md);
  cursor: pointer;
  color: var(--text-secondary);
  transition: var(--transition-fast);

  &:hover {
    background: var(--bg-hover);
    color: var(--text-primary);
    transform: translateY(-1px);
  }

  &:active {
    transform: scale(0.95);
  }
}

// 帮助内容
.help-content {
  .help-header {
    text-align: center;
    padding: 24px;
    margin-bottom: 24px;
    background: linear-gradient(135deg, var(--primary-light), transparent);
    border-radius: var(--radius-lg);

    .help-icon {
      width: 64px;
      height: 64px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--bg-primary);
      border-radius: var(--radius-lg);
      margin: 0 auto 16px;
      box-shadow: var(--shadow-md);

      .help-logo-img {
        width: 48px;
        height: 48px;
        object-fit: contain;
      }
    }

    h3 {
      font-size: var(--font-size-xl);
      font-weight: var(--font-weight-semibold);
      color: var(--text-primary);
      margin-bottom: 8px;
    }

    p {
      font-size: var(--font-size-sm);
      color: var(--text-secondary);
    }
  }

  .help-section {
    margin-bottom: 24px;

    h4 {
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
      color: var(--text-tertiary);
      text-transform: uppercase;
      letter-spacing: 0.05em;
      margin-bottom: 12px;
    }
  }

  .feature-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 12px;
  }

  .feature-item {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 12px;
    background: var(--bg-secondary);
    border-radius: var(--radius-md);
    transition: var(--transition-fast);

    &:hover {
      background: var(--bg-hover);
      transform: translateY(-2px);
    }

    .feature-icon {
      font-size: 20px;
    }

    .feature-text {
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-medium);
      color: var(--text-primary);
    }
  }

  .shortcut-list {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .shortcut-item {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: var(--font-size-sm);
    color: var(--text-secondary);

    kbd {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 28px;
      height: 24px;
      padding: 0 8px;
      font-family: var(--font-mono);
      font-size: var(--font-size-xs);
      font-weight: var(--font-weight-medium);
      background: var(--bg-tertiary);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xs);
      box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
    }

    .shortcut-desc {
      margin-left: auto;
      color: var(--text-tertiary);
    }
  }
}

// 动画
.fade-enter-active,
.fade-leave-active {
  transition: opacity var(--duration-fast) var(--ease-out);
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.slide-enter-active,
.slide-leave-active {
  transition: all var(--duration-normal) var(--ease-out);
  overflow: hidden;
}

.slide-enter-from,
.slide-leave-to {
  opacity: 0;
  max-height: 0;
  transform: translateY(-8px);
}

.slide-enter-to,
.slide-leave-from {
  max-height: 500px;
}
</style>

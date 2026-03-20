<template>
  <div class="menu-preview" :class="{ 'is-mobile': mode === 'mobile' }">
    <div class="preview-container">
      <!-- 模拟侧边栏 -->
      <div class="mock-sidebar">
        <div class="mock-header">
          <div class="mock-logo">
            <el-icon :size="20" color="var(--primary-color)"><Grid /></el-icon>
          </div>
          <span class="mock-title">{{ config.title || '项目管理基座' }}</span>
        </div>

        <el-scrollbar class="mock-menu">
          <div class="mock-menu-list">
            <template v-for="group in config.groups" :key="group.id">
              <div class="mock-group">
                <div class="mock-group-header" @click="toggleGroup(group.id)">
                  <span class="mock-group-icon">{{ group.icon }}</span>
                  <span class="mock-group-name">{{ group.name }}</span>
                  <el-icon class="mock-expand-icon" :class="{ 'is-expanded': expandedGroups.has(group.id) }">
                    <ArrowRight />
                  </el-icon>
                </div>

                <transition name="slide">
                  <div v-if="expandedGroups.has(group.id)" class="mock-group-items">
                    <div
                      v-for="item in group.children"
                      :key="item.id"
                      class="mock-item"
                      :class="{ 'is-active': selectedItemId === item.id }"
                      @click="handleItemClick(item)"
                    >
                      <span class="mock-item-icon">{{ item.icon }}</span>
                      <span class="mock-item-name">{{ item.name }}</span>
                    </div>
                  </div>
                </transition>
              </div>
            </template>
          </div>
        </el-scrollbar>
      </div>

      <!-- 模拟内容区 -->
      <div class="mock-content">
        <div v-if="selectedItem" class="mock-content-info">
          <div class="info-icon">{{ selectedItem.icon }}</div>
          <h3>{{ selectedItem.name }}</h3>
          <p>{{ selectedItem.description || '暂无描述' }}</p>
          <el-tag size="small" :type="getTypeTag(selectedItem.type).type">
            {{ getTypeTag(selectedItem.type).label }}
          </el-tag>
        </div>
        <div v-else class="mock-content-placeholder">
          <el-icon :size="48" color="var(--text-quaternary)"><Document /></el-icon>
          <p>选择菜单项查看详情</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'

const props = defineProps({
  config: {
    type: Object,
    required: true
  },
  mode: {
    type: String,
    default: 'desktop'
  },
  selectedItemId: {
    type: String,
    default: null
  }
})

const emit = defineEmits(['select'])

const expandedGroups = ref(new Set())
const selectedItem = ref(null)

// 初始化展开状态
onMounted(() => {
  if (props.config.groups) {
    props.config.groups.forEach(group => {
      if (group.expanded) {
        expandedGroups.value.add(group.id)
      }
    })
  }
})

// 切换分组展开
function toggleGroup(groupId) {
  if (expandedGroups.value.has(groupId)) {
    expandedGroups.value.delete(groupId)
  } else {
    expandedGroups.value.add(groupId)
  }
}

// 处理菜单项点击
function handleItemClick(item) {
  selectedItem.value = item
  emit('select', item.id)
}

// 获取类型标签
function getTypeTag(type) {
  const types = {
    web: { label: 'Web 应用', type: '' },
    mobile: { label: '移动端', type: 'success' },
    doc: { label: '文档', type: 'info' },
    internal: { label: '内部页面', type: 'warning' }
  }
  return types[type] || types.web
}
</script>

<style lang="scss" scoped>
.menu-preview {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;

  &.is-mobile {
    .preview-container {
      width: 375px;
      height: 667px;
      border-radius: 40px;
      box-shadow:
        0 0 0 12px #1C1C1E,
        0 0 0 14px #3A3A3C,
        0 25px 50px rgba(0, 0, 0, 0.3);
    }
  }
}

.preview-container {
  display: flex;
  width: 100%;
  height: 100%;
  background: var(--bg-primary);
  border: 1px solid var(--border-light);
  border-radius: var(--radius-md);
  overflow: hidden;
}

.mock-sidebar {
  width: 200px;
  border-right: 1px solid var(--border-light);
  display: flex;
  flex-direction: column;
}

.mock-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px;
  border-bottom: 1px solid var(--border-light);
}

.mock-logo {
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--primary-light);
  border-radius: var(--radius-sm);
}

.mock-title {
  font-size: 13px;
  font-weight: 500;
  color: var(--text-primary);
}

.mock-menu {
  flex: 1;
  overflow: hidden;
}

.mock-menu-list {
  padding: 8px;
}

.mock-group {
  margin-bottom: 4px;
}

.mock-group-header {
  display: flex;
  align-items: center;
  padding: 8px;
  border-radius: var(--radius-sm);
  cursor: pointer;

  &:hover {
    background: var(--bg-hover);
  }

  .mock-group-icon {
    font-size: 14px;
  }

  .mock-group-name {
    flex: 1;
    margin-left: 8px;
    font-size: 12px;
    font-weight: 500;
    color: var(--text-primary);
  }

  .mock-expand-icon {
    font-size: 10px;
    color: var(--text-tertiary);
    transition: transform 0.2s;

    &.is-expanded {
      transform: rotate(90deg);
    }
  }
}

.mock-group-items {
  padding-left: 12px;
}

.mock-item {
  display: flex;
  align-items: center;
  padding: 8px;
  border-radius: var(--radius-sm);
  cursor: pointer;

  &:hover {
    background: var(--bg-hover);
  }

  &.is-active {
    background: var(--primary-light);

    .mock-item-name {
      color: var(--primary-color);
    }
  }

  .mock-item-icon {
    font-size: 12px;
  }

  .mock-item-name {
    margin-left: 8px;
    font-size: 12px;
    color: var(--text-secondary);
  }
}

.mock-content {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg-secondary);
}

.mock-content-placeholder {
  text-align: center;
  color: var(--text-tertiary);

  p {
    margin-top: 12px;
    font-size: var(--font-size-sm);
  }
}

.mock-content-info {
  text-align: center;
  padding: 24px;

  .info-icon {
    font-size: 48px;
    margin-bottom: 16px;
  }

  h3 {
    font-size: 16px;
    font-weight: 500;
    color: var(--text-primary);
    margin-bottom: 8px;
  }

  p {
    font-size: var(--font-size-sm);
    color: var(--text-secondary);
    margin-bottom: 12px;
  }
}

// 动画
.slide-enter-active,
.slide-leave-active {
  transition: all 0.2s ease;
  overflow: hidden;
}

.slide-enter-from,
.slide-leave-to {
  opacity: 0;
  max-height: 0;
}

.slide-enter-to,
.slide-leave-from {
  max-height: 300px;
}
</style>

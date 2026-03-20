<template>
  <div class="menu-editor">
    <!-- 左侧编辑面板 -->
    <div class="editor-panel">
      <div class="panel-header">
        <div class="header-left">
          <h3>菜单配置</h3>
          <span class="item-count">{{ menuStore.groups.length }} 个分组</span>
        </div>
        <el-button type="primary" size="small" class="add-btn" @click="handleAddGroup">
          <el-icon><Plus /></el-icon>
          <span>添加分组</span>
        </el-button>
      </div>

      <el-scrollbar class="panel-content">
        <div v-if="menuStore.groups.length === 0" class="empty-state">
          <div class="empty-icon">📋</div>
          <h4>暂无菜单配置</h4>
          <p>点击上方按钮添加第一个分组</p>
          <el-button type="primary" @click="handleAddGroup">
            <el-icon><Plus /></el-icon>
            添加分组
          </el-button>
        </div>

        <draggable
          v-else
          :list="menuStore.groups"
          item-key="id"
          handle=".drag-handle"
          animation="200"
          ghost-class="ghost-item"
          @end="onGroupDragEnd"
        >
          <template #item="{ element: group, index }">
            <div
              class="group-wrapper"
              :style="{ animationDelay: `${index * 50}ms` }"
            >
              <GroupEditor
                :group="group"
                :selected="selectedGroupId === group.id"
                @select="selectGroup(group.id)"
                @update="updateGroup(group.id, $event)"
                @delete="deleteGroup(group.id)"
                @add-item="addMenuItem(group.id, $event)"
                @update-item="updateMenuItem(group.id, $event)"
                @delete-item="deleteMenuItem($event)"
              />
            </div>
          </template>
        </draggable>
      </el-scrollbar>
    </div>

  </div>
</template>

<script setup>
import { ref } from 'vue'
import draggable from 'vuedraggable'
import { useMenuStore } from '@/stores/menu'
import { ElMessage, ElMessageBox } from 'element-plus'
import GroupEditor from './GroupEditor.vue'

const menuStore = useMenuStore()

const selectedGroupId = ref(null)

// 选择分组
function selectGroup(groupId) {
  selectedGroupId.value = groupId
}

// 添加分组
function handleAddGroup() {
  const newGroup = menuStore.addGroup({
    name: '新分组',
    icon: '📁'
  })
  selectedGroupId.value = newGroup.id
  ElMessage.success('已添加新分组')
}

// 更新分组
function updateGroup(groupId, updates) {
  menuStore.updateGroup(groupId, updates)
}

// 删除分组
async function deleteGroup(groupId) {
  try {
    await ElMessageBox.confirm('确定要删除此分组及其所有菜单项吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    menuStore.deleteGroup(groupId)
    if (selectedGroupId.value === groupId) {
      selectedGroupId.value = null
    }
    ElMessage.success('已删除分组')
  } catch {
    // 取消操作
  }
}

// 添加菜单项
function addMenuItem(groupId, item) {
  menuStore.addMenuItem(groupId, item)
  ElMessage.success('已添加菜单项')
}

// 更新菜单项
function updateMenuItem(groupId, { itemId, updates }) {
  menuStore.updateMenuItem(itemId, updates)
}

// 删除菜单项
async function deleteMenuItem(itemId) {
  try {
    await ElMessageBox.confirm('确定要删除此菜单项吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    menuStore.deleteMenuItem(itemId)
    ElMessage.success('已删除菜单项')
  } catch {
    // 取消操作
  }
}

// 分组拖拽结束
function onGroupDragEnd(event) {
  menuStore.reorderGroups(event.oldIndex, event.newIndex)
}
</script>

<style lang="scss" scoped>
.menu-editor {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.editor-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

// 面板头部
.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--border-lighter);
  background: var(--bg-primary);

  .header-left {
    display: flex;
    align-items: center;
    gap: 12px;
  }

  h3 {
    font-size: var(--font-size-md);
    font-weight: var(--font-weight-semibold);
    color: var(--text-primary);
    letter-spacing: -0.01em;
  }

  .item-count {
    font-size: var(--font-size-xs);
    font-weight: var(--font-weight-medium);
    color: var(--text-tertiary);
    background: var(--bg-secondary);
    padding: 4px 10px;
    border-radius: var(--radius-full);
  }
}

// 添加按钮
.add-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 14px;
  border-radius: var(--radius-md);
  font-weight: var(--font-weight-medium);
  transition: var(--transition-spring);

  &:hover {
    transform: translateY(-1px);
    box-shadow: var(--shadow-primary);
  }

  &:active {
    transform: scale(0.98);
  }
}

// 面板内容
.panel-content {
  flex: 1;
  padding: 16px;
}

// 分组包装器动画
.group-wrapper {
  animation: fadeInUp var(--duration-normal) var(--ease-out) both;
}

// 空状态
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px 24px;
  text-align: center;

  .empty-icon {
    font-size: 48px;
    margin-bottom: 16px;
    opacity: 0.8;
  }

  h4 {
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-medium);
    color: var(--text-primary);
    margin-bottom: 8px;
  }

  p {
    font-size: var(--font-size-sm);
    color: var(--text-tertiary);
    margin-bottom: 20px;
  }
}

// 拖拽占位符
.ghost-item {
  opacity: 0.5;
  background: var(--primary-light);
  border-radius: var(--radius-md);
}

// 动画
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(12px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

// 响应式
@media (max-width: 1024px) {
  .menu-editor {
    flex-direction: column;
  }

  .editor-panel {
    flex: none;
    height: 50%;
  }
}
</style>

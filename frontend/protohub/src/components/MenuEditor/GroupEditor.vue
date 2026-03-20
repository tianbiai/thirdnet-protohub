<template>
  <div class="group-editor" :class="{ 'is-selected': selected, 'is-expanded': true }">
    <!-- 分组头部 -->
    <div class="group-header" @click="$emit('select')">
      <div class="header-left">
        <el-icon class="drag-handle"><Rank /></el-icon>
        <div class="group-icon-input">
          <el-input
            v-model="localGroup.icon"
            placeholder="📁"
            size="small"
            class="icon-input"
            @change="handleUpdate"
            @click.stop
          />
        </div>
        <el-input
          v-model="localGroup.name"
          placeholder="分组名称"
          size="small"
          class="name-input"
          @change="handleUpdate"
          @click.stop
        />
      </div>
      <div class="header-right">
        <span class="item-count">{{ localGroup.children?.length || 0 }} 项</span>
        <el-button
          type="danger"
          text
          size="small"
          class="delete-btn"
          @click.stop="$emit('delete')"
        >
          <el-icon><Delete /></el-icon>
        </el-button>
      </div>
    </div>

    <!-- 菜单项列表 -->
    <div class="group-items">
      <draggable
        :list="localGroup.children"
        item-key="id"
        handle=".item-drag-handle"
        animation="200"
        group="menu-items"
        ghost-class="ghost-item"
        @end="onItemDragEnd"
      >
        <template #item="{ element: item, index }">
          <div
            class="item-wrapper"
            :style="{ animationDelay: `${index * 30}ms` }"
          >
            <ItemEditor
              :item="item"
              @update="handleItemUpdate(index, $event)"
              @delete="$emit('delete-item', item.id)"
            />
          </div>
        </template>
      </draggable>

      <!-- 添加菜单项按钮 -->
      <div class="add-item-wrapper">
        <el-button
          class="add-item-btn"
          size="small"
          @click="showAddDialog = true"
        >
          <el-icon class="add-icon"><Plus /></el-icon>
          <span>添加菜单项</span>
        </el-button>
      </div>
    </div>

    <!-- 添加菜单项对话框 -->
    <el-dialog
      v-model="showAddDialog"
      title="添加菜单项"
      width="520px"
      destroy-on-close
      append-to-body
      center
      :modal-append-to-body="true"
      class="add-item-dialog"
    >
      <div class="dialog-content">
        <!-- 类型选择卡片 -->
        <div class="type-selector">
          <label class="type-label">选择类型</label>
          <div class="type-cards">
            <div
              v-for="typeOption in typeOptions"
              :key="typeOption.value"
              class="type-card"
              :class="{ 'is-active': newItem.type === typeOption.value }"
              @click="newItem.type = typeOption.value"
            >
              <span class="type-icon">{{ typeOption.icon }}</span>
              <span class="type-name">{{ typeOption.label }}</span>
            </div>
          </div>
        </div>

        <el-form
          ref="itemFormRef"
          :model="newItem"
          :rules="itemRules"
          label-width="100px"
          class="item-form"
        >
          <el-form-item label="名称" prop="name">
            <el-input v-model="newItem.name" placeholder="菜单项名称" />
          </el-form-item>
          <el-form-item label="图标" prop="icon">
            <el-input v-model="newItem.icon" placeholder="如：📱" />
          </el-form-item>

          <!-- Web/小程序配置 -->
          <template v-if="newItem.type === 'web' || newItem.type === 'miniprogram'">
            <el-form-item label="URL" prop="url">
              <el-input v-model="newItem.url" placeholder="http://localhost:3000" />
            </el-form-item>
            <el-form-item v-if="newItem.type === 'miniprogram'" label="视口尺寸">
              <div class="viewport-inputs">
                <el-input-number v-model="newItem.viewport.width" :min="200" :max="1000" placeholder="宽度" />
                <span class="viewport-sep">×</span>
                <el-input-number v-model="newItem.viewport.height" :min="400" :max="2000" placeholder="高度" />
              </div>
            </el-form-item>
          </template>

          <!-- Swagger API 文档配置 -->
          <template v-if="newItem.type === 'swagger'">
            <el-form-item label="API URL" prop="url">
              <el-input v-model="newItem.url" placeholder="http://localhost:8080/swagger-ui.html" />
            </el-form-item>
            <el-form-item label="接口描述">
              <el-input v-model="newItem.apiDescription" placeholder="接口服务描述（可选）" />
            </el-form-item>
            <el-form-item label="Basic 认证">
              <el-switch v-model="newItem.useAuth" />
            </el-form-item>
            <template v-if="newItem.useAuth">
              <el-form-item label="用户名">
                <el-input v-model="newItem.authUsername" placeholder="输入用户名" />
              </el-form-item>
              <el-form-item label="密码">
                <el-input v-model="newItem.authPassword" type="password" placeholder="输入密码" show-password />
              </el-form-item>
            </template>
          </template>

          <!-- 文档配置 -->
          <template v-if="newItem.type === 'doc'">
            <el-form-item label="上传文档" required>
              <div class="upload-area">
                <el-upload
                  ref="uploadRef"
                  :auto-upload="false"
                  :show-file-list="false"
                  accept=".md,.markdown"
                  :on-change="handleFileSelect"
                >
                  <el-button type="primary" plain>
                    <el-icon><Upload /></el-icon>
                    选择 Markdown 文件
                  </el-button>
                </el-upload>
                <div v-if="newItem.docFileName" class="file-info">
                  <el-icon><Document /></el-icon>
                  <span class="file-name">{{ newItem.docFileName }}</span>
                  <el-button type="danger" text size="small" @click="clearDocFile">
                    <el-icon><Close /></el-icon>
                  </el-button>
                </div>
                <div v-else class="upload-tip">
                  <el-icon><WarningFilled /></el-icon>
                  <span>请上传 .md 或 .markdown 文件</span>
                </div>
              </div>
            </el-form-item>
            <el-form-item label="文档描述">
              <el-input v-model="newItem.docDescription" placeholder="文档描述（可选）" />
            </el-form-item>
          </template>

          <el-form-item label="描述">
            <el-input
              v-model="newItem.description"
              type="textarea"
              :rows="2"
              placeholder="菜单项描述（可选）"
            />
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <el-button @click="showAddDialog = false">取消</el-button>
        <el-button type="primary" @click="handleAddItem">添加</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, watch } from 'vue'
import draggable from 'vuedraggable'
import { ElMessage } from 'element-plus'
import { useMenuStore } from '@/stores/menu'
import { useFileUpload } from '@/composables/useFileUpload'
import { TYPE_OPTIONS, DEFAULT_VIEWPORT, createEmptyMenuItem } from '@/utils/menu'
import ItemEditor from './ItemEditor.vue'

const props = defineProps({
  group: {
    type: Object,
    required: true
  },
  selected: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['select', 'update', 'delete', 'add-item', 'update-item', 'delete-item'])

const menuStore = useMenuStore()
const { pendingFile, handleFileSelect: onSelectFile, clearFile, uploadFile } = useFileUpload()

const localGroup = reactive({ ...props.group })
const showAddDialog = ref(false)
const itemFormRef = ref(null)
const uploadRef = ref(null)

// 使用统一的类型选项
const typeOptions = TYPE_OPTIONS

const newItem = reactive(createEmptyMenuItem())

const itemRules = {
  name: [{ required: true, message: '请输入名称', trigger: 'blur' }],
  type: [{ required: true, message: '请选择类型', trigger: 'change' }],
  url: [{ required: true, message: '请输入URL', trigger: 'blur' }],
  docPath: [{ required: true, message: '请输入文档路径', trigger: 'blur' }],
  route: [{ required: true, message: '请输入路由路径', trigger: 'blur' }]
}

// 监听 group 变化，同步到 localGroup
watch(() => props.group, (newGroup) => {
  Object.assign(localGroup, newGroup)
}, { deep: true })

// 处理分组更新
function handleUpdate() {
  emit('update', {
    name: localGroup.name,
    icon: localGroup.icon
  })
}

// 处理菜单项更新
function handleItemUpdate(index, updates) {
  const item = localGroup.children[index]
  emit('update-item', {
    itemId: item.id,
    updates
  })
}

// 处理菜单项拖拽结束
function onItemDragEnd() {
  menuStore.saveMenuConfig()
}

// 添加菜单项
async function handleAddItem() {
  try {
    await itemFormRef.value.validate()

    const item = {
      name: newItem.name,
      icon: newItem.icon,
      type: newItem.type,
      description: newItem.description
    }

    if (newItem.type === 'web' || newItem.type === 'miniprogram') {
      item.url = newItem.url
      if (newItem.type === 'miniprogram') {
        item.viewport = { ...newItem.viewport }
      }
    } else if (newItem.type === 'swagger') {
      item.url = newItem.url
      item.apiDescription = newItem.apiDescription
      item.useAuth = newItem.useAuth
      item.authUsername = newItem.authUsername
      item.authPassword = newItem.authPassword
    } else if (newItem.type === 'doc') {
      // 文档类型必须上传文件
      if (!pendingFile.value) {
        ElMessage.error('请上传 Markdown 文档文件')
        return
      }

      // 使用 composable 上传文件
      const result = await uploadFile()
      if (result) {
        item.docFileId = result.fileId
        item.docFileName = result.originalName
        item.url = result.url
        item.docDescription = newItem.docDescription
      } else {
        return // 上传失败，composable 已显示错误
      }
    } else if (newItem.type === 'internal') {
      item.route = newItem.route
    }

    emit('add-item', item)
    showAddDialog.value = false

    // 重置表单
    Object.assign(newItem, createEmptyMenuItem())
    clearFile()
  } catch (validationError) {
    console.warn('表单验证失败:', validationError)
    // 验证失败时 Element Plus 会自动显示错误提示
  }
}

// 处理文件选择
function handleFileSelect(uploadFile) {
  onSelectFile(uploadFile)
  if (pendingFile.value) {
    newItem.docFileName = pendingFile.value.name
  }
}

// 清除文档文件
function clearDocFile() {
  clearFile()
  newItem.docFileName = ''
}
</script>

<style lang="scss" scoped>
.group-editor {
  margin-bottom: 12px;
  background: var(--bg-primary);
  border: 1px solid var(--border-light);
  border-radius: var(--radius-lg);
  overflow: hidden;
  transition: var(--transition-fast);

  &:hover {
    border-color: var(--border-color);
  }

  &.is-selected {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 3px var(--primary-glow);
  }
}

.group-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 16px;
  background: var(--bg-secondary);
  cursor: pointer;
  transition: var(--transition-fast);

  &:hover {
    background: var(--bg-tertiary);
  }

  .header-left {
    display: flex;
    align-items: center;
    gap: 10px;
    flex: 1;
    min-width: 0;
  }

  .header-right {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .drag-handle {
    cursor: grab;
    color: var(--text-tertiary);
    transition: color var(--transition-fast);

    &:hover {
      color: var(--text-secondary);
    }

    &:active {
      cursor: grabbing;
    }
  }

  .group-icon-input {
    flex-shrink: 0;

    .icon-input {
      width: 52px;

      :deep(.el-input__wrapper) {
        background: var(--bg-primary);
        text-align: center;
      }

      :deep(.el-input__inner) {
        text-align: center;
        font-size: 16px;
      }
    }
  }

  .name-input {
    flex: 1;
    min-width: 0;
  }

  .item-count {
    font-size: var(--font-size-xs);
    color: var(--text-tertiary);
    background: var(--bg-primary);
    padding: 4px 10px;
    border-radius: var(--radius-full);
  }

  .delete-btn {
    opacity: 0;
    transition: opacity var(--transition-fast);
  }

  &:hover .delete-btn {
    opacity: 1;
  }
}

.group-items {
  padding: 12px;
}

.item-wrapper {
  animation: fadeIn var(--duration-fast) var(--ease-out) both;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(4px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.add-item-wrapper {
  margin-top: 8px;
}

.add-item-btn {
  width: 100%;
  border-style: dashed;
  border-color: var(--border-color);
  color: var(--text-secondary);
  background: transparent;
  transition: var(--transition-fast);

  .add-icon {
    font-size: 14px;
  }

  &:hover {
    border-color: var(--primary-color);
    color: var(--primary-color);
    background: var(--primary-light);
  }
}

// 对话框样式
.dialog-content {
  .type-selector {
    margin-bottom: 24px;

    .type-label {
      display: block;
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-medium);
      color: var(--text-secondary);
      margin-bottom: 12px;
    }
  }

  .type-cards {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 12px;
  }

  .type-card {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
    padding: 12px 8px;
    background: var(--bg-secondary);
    border: 2px solid transparent;
    border-radius: var(--radius-md);
    cursor: pointer;
    transition: var(--transition-fast);

    &:hover {
      background: var(--bg-hover);
      border-color: var(--border-color);
    }

    &.is-active {
      background: var(--primary-light);
      border-color: var(--primary-color);

      .type-name {
        color: var(--primary-color);
        font-weight: var(--font-weight-medium);
      }
    }

    .type-icon {
      font-size: 24px;
    }

    .type-name {
      font-size: var(--font-size-xs);
      color: var(--text-secondary);
      text-align: center;
    }
  }

  .viewport-inputs {
    display: flex;
    align-items: center;
    gap: 12px;

    .viewport-sep {
      color: var(--text-tertiary);
      font-weight: var(--font-weight-medium);
    }
  }
}

// 拖拽占位符
.ghost-item {
  opacity: 0.5;
  background: var(--primary-light);
  border-radius: var(--radius-md);
}

// 上传区域样式
.upload-area {
  display: flex;
  flex-direction: column;
  gap: 12px;
  width: 100%;
}

.file-info {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background: var(--bg-secondary);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-sm);

  .file-name {
    flex: 1;
    color: var(--text-primary);
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
}

.upload-tip {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background: var(--bg-tertiary);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-sm);
  color: var(--text-tertiary);
}
</style>

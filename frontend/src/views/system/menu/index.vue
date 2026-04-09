<template>
  <ManagePageLayout title="系统功能菜单" subtitle="管理系统导航菜单结构">
    <template #actions>
      <el-button type="primary" v-if="userStore.hasPermission('system-menu-manage:create')" @click="showCreateDialog(null)">
        <el-icon><Plus /></el-icon>
        添加菜单
      </el-button>
    </template>

    <!-- 搜索栏 -->
    <div class="search-section">
      <el-icon :size="16" style="color: var(--text-tertiary)"><Search /></el-icon>
      <el-input
        v-model="searchKeyword"
        placeholder="搜索菜单名称/编码"
        clearable
        style="flex: 1"
      />
    </div>

    <!-- 菜单树形表格 -->
    <div class="table-card">
      <el-table
        :data="filteredMenuList"
        v-loading="loading"
        row-key="id"
        default-expand-all
        border
      >
        <el-table-column prop="name" label="菜单名称" min-width="180">
          <template #default="{ row }">
            <span style="font-weight: var(--font-weight-medium)">{{ row.name }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="code" label="菜单编码" min-width="120" show-overflow-tooltip />
        <el-table-column prop="icon" label="图标" min-width="70" align="center" show-overflow-tooltip />
        <el-table-column prop="path" label="路由路径" min-width="140" show-overflow-tooltip />
        <el-table-column label="排序" min-width="60" align="center">
          <template #default="{ row }">
            {{ row.order }}
          </template>
        </el-table-column>
        <el-table-column label="可见" min-width="60" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isVisible ? 'success' : 'info'" size="small" effect="light">
              {{ row.isVisible ? '是' : '否' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="permission" label="所需权限" min-width="120" show-overflow-tooltip />
        <el-table-column label="操作" min-width="100" align="center" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" size="small" v-if="userStore.hasPermission('system-menu-manage:update')" @click="showEditDialog(row)">编辑</el-button>
            <el-button link type="danger" size="small" v-if="userStore.hasPermission('system-menu-manage:delete')" :loading="deleteLock.isLoading(row.id)" :disabled="deleteLock.isLoading(row.id)" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 添加/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑菜单' : '添加菜单'"
      width="600px"
      @close="resetForm"
    >
      <el-form ref="formRef" :model="formData" :rules="formRules" label-width="100px">
        <el-form-item label="上级菜单" prop="parentId">
          <el-tree-select
            v-model="formData.parentId"
            :data="menuTreeData"
            :props="{ label: 'name', value: 'id' }"
            check-strictly
            placeholder="请选择上级菜单"
            style="width: 100%"
            clearable
          />
        </el-form-item>
        <el-form-item label="菜单名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入菜单名称" />
        </el-form-item>
        <el-form-item label="菜单编码" prop="code">
          <el-input v-model="formData.code" placeholder="请输入菜单编码" />
        </el-form-item>
        <el-form-item label="图标" prop="icon">
          <el-input v-model="formData.icon" placeholder="请输入图标名称" />
        </el-form-item>
        <el-form-item label="路由路径" prop="path">
          <el-input v-model="formData.path" placeholder="请输入路由路径" />
        </el-form-item>
        <el-form-item label="排序" prop="order">
          <el-input-number v-model="formData.order" :min="0" :max="999" />
        </el-form-item>
        <el-form-item label="是否可见">
          <el-switch v-model="formData.isVisible" />
        </el-form-item>
        <el-form-item label="所需权限" prop="permission">
          <el-select v-model="formData.permission" placeholder="请选择所需权限" clearable>
            <el-option
              v-for="perm in permissions"
              :key="perm.code"
              :label="perm.name"
              :value="perm.code"
            />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitLoading" :disabled="submitLoading" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>

    <el-empty v-if="!loading && menuList.length === 0" description="暂无数据" />
  </ManagePageLayout>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search } from '@element-plus/icons-vue'
import ManagePageLayout from '@/components/ManagePageLayout/index.vue'
import { getSystemMenuTree, createSystemMenu, updateSystemMenu, deleteSystemMenu } from '@/api/system-menu'
import { useUserStore } from '@/stores/user'
import { useAsyncLock } from '@/composables/useAsyncLock'

const userStore = useUserStore()

const deleteLock = useAsyncLock()

import { getPermissionList } from '@/api/permission'

// 数据状态
const loading = ref(false)
const menuList = ref([])
const permissions = ref([])
const searchKeyword = ref('')

// 过滤后的菜单列表
const filteredMenuList = computed(() => {
  if (!searchKeyword.value) {
    return menuList.value
  }
  const keyword = searchKeyword.value.toLowerCase()
  return filterTree(menuList.value, keyword)
})

// 递归过滤树
function filterTree(tree, keyword) {
  const result = []
  for (const node of tree) {
    const match = node.name.toLowerCase().includes(keyword) ||
      node.code.toLowerCase().includes(keyword)
    if (match) {
      result.push(node)
    } else if (node.children && node.children.length > 0) {
      const filteredChildren = filterTree(node.children, keyword)
      if (filteredChildren.length > 0) {
        result.push({ ...node, children: filteredChildren })
      }
    }
  }
  return result
}

// 树形选择器数据
const menuTreeData = computed(() => {
  return [{ id: 0, name: '顶级菜单', children: menuList.value }]
})

// 对话框状态
const dialogVisible = ref(false)
const isEdit = ref(false)
const submitLoading = ref(false)
const formRef = ref(null)

const formData = reactive({
  id: null,
  parentId: null,
  name: '',
  code: '',
  icon: '',
  path: '',
  order: 0,
  isVisible: true,
  permission: ''
})

// 表单验证规则
const formRules = {
  name: [{ required: true, message: '请输入菜单名称', trigger: 'blur' }],
  code: [{ required: true, message: '请输入菜单编码', trigger: 'blur' }]
}

// 加载菜单列表
async function loadMenus() {
  loading.value = true
  try {
    const res = await getSystemMenuTree()
    menuList.value = res || []
  } catch (error) {
    ElMessage.error(error.message || '加载菜单列表失败')
  } finally {
    loading.value = false
  }
}

// 加载权限列表
async function loadPermissions() {
  try {
    const res = await getPermissionList()
    permissions.value = res.list || res || []
  } catch (error) {
    console.error('加载权限列表失败:', error)
  }
}

// 显示创建对话框
function showCreateDialog(parent) {
  isEdit.value = false
  resetForm()
  if (parent) {
    formData.parentId = parent.id
  }
  dialogVisible.value = true
}

// 显示编辑对话框
function showEditDialog(row) {
  isEdit.value = true
  Object.assign(formData, {
    id: row.id,
    parentId: row.parentId,
    name: row.name,
    code: row.code,
    icon: row.icon,
    path: row.path,
    order: row.order,
    isVisible: row.isVisible,
    permission: row.permission
  })
  dialogVisible.value = true
}

// 重置表单
function resetForm() {
  formRef.value?.resetFields()
  Object.assign(formData, {
    id: null,
    parentId: null,
    name: '',
    code: '',
    icon: '',
    path: '',
    order: 0,
    isVisible: true,
    permission: ''
  })
}

// 提交表单
async function handleSubmit() {
  try {
    await formRef.value?.validate()
  } catch {
    return
  }

  submitLoading.value = true
  try {
    if (isEdit.value) {
      await updateSystemMenu(formData)
      ElMessage.success('菜单更新成功')
    } else {
      await createSystemMenu(formData)
      ElMessage.success('菜单创建成功')
    }
    dialogVisible.value = false
    loadMenus()
  } catch (error) {
    ElMessage.error(error.message || '操作失败')
  } finally {
    submitLoading.value = false
  }
}

// 删除菜单
async function handleDelete(row) {
  await deleteLock.forKey(row.id, async () => {
    try {
      await ElMessageBox.confirm(`确定要删除菜单 "${row.name}" 吗？`, '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
      await deleteSystemMenu(row.id)
      ElMessage.success('菜单删除成功')
      loadMenus()
    } catch (error) {
      if (error !== 'cancel') {
        ElMessage.error(error.message || '删除失败')
      }
    }
  })()
}

onMounted(() => {
  loadMenus()
  loadPermissions()
})
</script>

<style lang="scss" scoped>
.search-section {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 20px;
  padding: 12px 18px;
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-light);
}

[data-theme="dark"] {
  .search-section {
    background: var(--bg-secondary);
  }
}
</style>

<template>
  <ManagePageLayout title="角色管理" subtitle="配置系统角色与权限分配">
    <template #actions>
      <el-button type="primary" v-if="userStore.hasPermission('role-manage:create')" @click="showCreateDialog">
        <el-icon><Plus /></el-icon>
        添加角色
      </el-button>
    </template>

    <!-- 角色列表 -->
    <div class="table-card">
      <el-table :data="roles" v-loading="loading" border>
        <el-table-column prop="code" label="角色编码" width="160" />
        <el-table-column prop="name" label="角色名称" width="140" />
        <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
        <el-table-column label="系统内置" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isSystem ? 'info' : ''" size="small" effect="light">
              {{ row.isSystem ? '是' : '否' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createTime" label="创建时间" width="180">
          <template #default="{ row }">
            {{ formatTime(row.createTime) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" align="center">
          <template #default="{ row }">
            <el-button link type="primary" size="small" v-if="userStore.hasPermission('role-manage:assign-permission')" @click="showPermissionDialog(row)">分配权限</el-button>
            <el-button link type="primary" size="small" v-if="userStore.hasPermission('role-manage:update')" :disabled="row.isSystem" @click="showEditDialog(row)">编辑</el-button>
            <el-button link type="danger" size="small" v-if="userStore.hasPermission('role-manage:delete')" :disabled="row.isSystem || deleteLock.isLoading(row.id)" :loading="deleteLock.isLoading(row.id)" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 创建/编辑角色对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑角色' : '添加角色'"
      width="500px"
      @close="resetForm"
    >
      <el-form ref="formRef" :model="formData" :rules="formRules" label-width="80px">
        <el-form-item label="角色编码" prop="code">
          <el-input v-model="formData.code" :disabled="isEdit" placeholder="请输入角色编码" />
        </el-form-item>
        <el-form-item label="角色名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入角色名称" />
        </el-form-item>
        <el-form-item label="描述" prop="description">
          <el-input v-model="formData.description" type="textarea" :rows="3" placeholder="请输入描述" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitLoading" :disabled="submitLoading" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>

    <!-- 分配权限对话框 -->
    <el-dialog
      v-model="permissionDialogVisible"
      title="分配权限"
      width="600px"
    >
      <div class="permission-dialog-content">
        <div class="current-role-badge">
          <el-icon :size="16"><UserFilled /></el-icon>
          <span>当前角色：<strong>{{ currentRole?.name }}</strong></span>
        </div>

        <!-- 按分类显示权限 -->
        <div v-for="category in permissionCategories" :key="category" class="permission-category">
          <h4>{{ category }}</h4>
          <el-checkbox-group v-model="selectedPermissionIds">
            <el-checkbox
              v-for="permission in permissionsByCategory[category]"
              :key="permission.id"
              :label="permission.id"
            >
              {{ permission.name }} ({{ permission.code }})
            </el-checkbox>
          </el-checkbox-group>
        </div>
      </div>
      <template #footer>
        <el-button @click="permissionDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="permissionLoading" :disabled="permissionLoading" @click="handleAssignPermissions">确定</el-button>
      </template>
    </el-dialog>
  </ManagePageLayout>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, UserFilled } from '@element-plus/icons-vue'
import ManagePageLayout from '@/components/ManagePageLayout/index.vue'
import * as roleApi from '@/api/role'
import * as permissionApi from '@/api/permission'
import { useUserStore } from '@/stores/user'
import { useAsyncLock } from '@/composables/useAsyncLock'

const userStore = useUserStore()

const deleteLock = useAsyncLock()

// 数据状态
const loading = ref(false)
const roles = ref([])

// 对话框状态
const dialogVisible = ref(false)
const isEdit = ref(false)
const submitLoading = ref(false)
const formRef = ref(null)

// 表单数据
const formData = reactive({
  id: null,
  code: '',
  name: '',
  description: ''
})

// 表单验证规则
const formRules = {
  code: [
    { required: true, message: '请输入角色编码', trigger: 'blur' },
    { pattern: /^[a-z][a-z0-9-]*$/, message: '编码只能包含小写字母、数字和横线，且以字母开头', trigger: 'blur' }
  ],
  name: [
    { required: true, message: '请输入角色名称', trigger: 'blur' }
  ]
}

// 权限分配
const permissionDialogVisible = ref(false)
const permissionLoading = ref(false)
const currentRole = ref(null)
const allPermissions = ref([])
const selectedPermissionIds = ref([])

// 按分类分组的权限
const permissionCategories = computed(() => {
  return [...new Set(allPermissions.value.map(p => p.category))]
})

const permissionsByCategory = computed(() => {
  const result = {}
  for (const category of permissionCategories.value) {
    result[category] = allPermissions.value.filter(p => p.category === category)
  }
  return result
})

// 格式化时间
function formatTime(time) {
  if (!time) return '-'
  return new Date(time).toLocaleString('zh-CN')
}

// 加载角色列表
async function loadRoles() {
  loading.value = true
  try {
    const res = await roleApi.getRoleList()
    roles.value = res.list || res || []
  } catch (error) {
    ElMessage.error(error.message || '加载角色列表失败')
  } finally {
    loading.value = false
  }
}

// 加载所有权限
async function loadPermissions() {
  try {
    const res = await permissionApi.getPermissionList()
    allPermissions.value = res.list || res || []
  } catch (error) {
    ElMessage.error('加载权限列表失败')
  }
}

// 显示创建对话框
function showCreateDialog() {
  isEdit.value = false
  dialogVisible.value = true
}

// 显示编辑对话框
function showEditDialog(row) {
  isEdit.value = true
  Object.assign(formData, {
    id: row.id,
    code: row.code,
    name: row.name,
    description: row.description
  })
  dialogVisible.value = true
}

// 重置表单
function resetForm() {
  formRef.value?.resetFields()
  Object.assign(formData, {
    id: null,
    code: '',
    name: '',
    description: ''
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
      await roleApi.updateRole(formData)
      ElMessage.success('角色更新成功')
    } else {
      await roleApi.createRole(formData)
      ElMessage.success('角色创建成功')
    }
    dialogVisible.value = false
    loadRoles()
  } catch (error) {
    ElMessage.error(error.message || '操作失败')
  } finally {
    submitLoading.value = false
  }
}

// 删除角色
async function handleDelete(row) {
  await deleteLock.forKey(row.id, async () => {
    try {
      await ElMessageBox.confirm(`确定要删除角色 "${row.name}" 吗？`, '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
      await roleApi.deleteRole(row.id)
      ElMessage.success('角色删除成功')
      loadRoles()
    } catch (error) {
      if (error !== 'cancel') {
        ElMessage.error(error.message || '删除失败')
      }
    }
  })()
}

// 显示权限分配对话框
async function showPermissionDialog(row) {
  currentRole.value = row
  permissionDialogVisible.value = true

  // 加载所有权限
  if (allPermissions.value.length === 0) {
    await loadPermissions()
  }

  // 加载角色当前权限
  try {
    const res = await roleApi.getRolePermissions(row.id)
    selectedPermissionIds.value = res.map(p => p.id)
  } catch (error) {
    selectedPermissionIds.value = []
  }
}

// 分配权限
async function handleAssignPermissions() {
  permissionLoading.value = true
  try {
    await roleApi.assignRolePermissions(currentRole.value.id, selectedPermissionIds.value)
    ElMessage.success('权限分配成功')
    permissionDialogVisible.value = false
  } catch (error) {
    ElMessage.error(error.message || '权限分配失败')
  } finally {
    permissionLoading.value = false
  }
}

onMounted(() => {
  loadRoles()
})
</script>

<style lang="scss" scoped>
.permission-dialog-content {
  .current-role-badge {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 8px 14px;
    background: var(--bg-secondary);
    border-radius: var(--radius-md);
    color: var(--text-secondary);
    font-size: var(--font-size-sm);
    margin-bottom: 20px;

    strong {
      color: var(--primary-color);
    }
  }

  .permission-category {
    margin-bottom: 20px;

    h4 {
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
      color: var(--text-primary);
      margin-bottom: 12px;
      padding-bottom: 8px;
      border-bottom: 1px solid var(--border-light);
    }

    .el-checkbox-group {
      display: flex;
      flex-wrap: wrap;
      gap: 12px;
    }
  }
}
</style>

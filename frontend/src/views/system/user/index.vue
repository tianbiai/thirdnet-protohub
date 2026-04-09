<template>
  <ManagePageLayout title="用户管理" subtitle="管理系统用户账号与角色分配">
    <template #actions>
      <el-input
        v-model="searchKeyword"
        placeholder="搜索用户名/昵称"
        clearable
        style="width: 220px"
        @keyup.enter="loadUsers"
      >
        <template #prefix>
          <el-icon><Search /></el-icon>
        </template>
      </el-input>
      <el-button type="primary" v-if="userStore.hasPermission('user-manage:create')" @click="showCreateDialog">
        <el-icon><Plus /></el-icon>
        添加用户
      </el-button>
    </template>

    <!-- 用户列表 -->
    <div class="table-card">
      <el-table :data="users" v-loading="loading" border>
        <el-table-column prop="userName" label="用户名" width="140" />
        <el-table-column prop="nickName" label="昵称" width="140" />
        <el-table-column prop="email" label="邮箱" min-width="200" show-overflow-tooltip />
        <el-table-column label="状态" width="80" align="center">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'danger'" size="small" effect="light">
              {{ row.status === 1 ? '启用' : '禁用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="description" label="描述" min-width="150" show-overflow-tooltip />
        <el-table-column label="操作" width="200" align="center">
          <template #default="{ row }">
            <el-button link type="primary" size="small" v-if="userStore.hasPermission('user-manage:assign-role')" @click="showRoleDialog(row)">分配角色</el-button>
            <el-button link type="primary" size="small" v-if="userStore.hasPermission('user-manage:update')" @click="showEditDialog(row)">编辑</el-button>
            <el-button link type="danger" size="small" v-if="userStore.hasPermission('user-manage:delete')" :loading="deleteLock.isLoading(row.id)" :disabled="deleteLock.isLoading(row.id)" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 分页 -->
    <div class="pagination-wrapper">
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :page-sizes="[10, 20, 50]"
        :total="total"
        layout="total, sizes, prev, pager, next"
        @size-change="loadUsers"
        @current-change="loadUsers"
      />
    </div>

    <!-- 创建/编辑用户对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑用户' : '添加用户'"
      width="500px"
      @close="resetForm"
    >
      <el-form ref="formRef" :model="formData" :rules="formRules" label-width="80px">
        <el-form-item label="用户名" prop="userName">
          <el-input v-model="formData.userName" :disabled="isEdit" placeholder="请输入用户名" />
        </el-form-item>
        <el-form-item v-if="!isEdit" label="密码" prop="password">
          <el-input v-model="formData.password" type="password" placeholder="请输入密码" show-password />
        </el-form-item>
        <el-form-item label="昵称" prop="nickName">
          <el-input v-model="formData.nickName" placeholder="请输入昵称" />
        </el-form-item>
        <el-form-item label="邮箱" prop="email">
          <el-input v-model="formData.email" placeholder="请输入邮箱" />
        </el-form-item>
        <el-form-item label="状态" prop="status">
          <el-radio-group v-model="formData.status">
            <el-radio :label="1">启用</el-radio>
            <el-radio :label="0">禁用</el-radio>
          </el-radio-group>
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

    <!-- 分配角色对话框 -->
    <el-dialog
      v-model="roleDialogVisible"
      title="分配角色"
      width="500px"
    >
      <div class="role-dialog-content">
        <div class="current-user-badge">
          <el-icon :size="16"><User /></el-icon>
          <span>当前用户：<strong>{{ currentUser?.userName }}</strong></span>
        </div>
        <el-checkbox-group v-model="selectedRoleIds">
          <el-checkbox v-for="role in allRoles" :key="role.id" :label="role.id">
            {{ role.name }} ({{ role.code }})
          </el-checkbox>
        </el-checkbox-group>
      </div>
      <template #footer>
        <el-button @click="roleDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="roleLoading" :disabled="roleLoading" @click="handleAssignRoles">确定</el-button>
      </template>
    </el-dialog>
  </ManagePageLayout>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Plus, User } from '@element-plus/icons-vue'
import ManagePageLayout from '@/components/ManagePageLayout/index.vue'
import * as userApi from '@/api/user'
import { getRoleList } from '@/api/role'
import { useUserStore } from '@/stores/user'
import { useAsyncLock } from '@/composables/useAsyncLock'

const userStore = useUserStore()

const deleteLock = useAsyncLock()

// 数据状态
const loading = ref(false)
const users = ref([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(10)
const searchKeyword = ref('')

// 对话框状态
const dialogVisible = ref(false)
const isEdit = ref(false)
const submitLoading = ref(false)
const formRef = ref(null)

// 表单数据
const formData = reactive({
  id: null,
  userName: '',
  password: '',
  nickName: '',
  email: '',
  status: 1,
  description: ''
})

// 表单验证规则
const formRules = {
  userName: [
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 3, max: 50, message: '用户名长度为3-50个字符', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, max: 50, message: '密码长度为6-50个字符', trigger: 'blur' }
  ],
  email: [
    { type: 'email', message: '请输入正确的邮箱格式', trigger: 'blur' }
  ]
}

// 角色分配
const roleDialogVisible = ref(false)
const roleLoading = ref(false)
const currentUser = ref(null)
const allRoles = ref([])
const selectedRoleIds = ref([])

// 加载用户列表
async function loadUsers() {
  loading.value = true
  try {
    const res = await userApi.getUserList({
      page: currentPage.value,
      pageSize: pageSize.value,
      keyword: searchKeyword.value
    })
    users.value = res.list || []
    total.value = res.total || 0
  } catch (error) {
    ElMessage.error(error.message || '加载用户列表失败')
  } finally {
    loading.value = false
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
    userName: row.userName,
    nickName: row.nickName,
    email: row.email,
    status: row.status,
    description: row.description
  })
  dialogVisible.value = true
}

// 重置表单
function resetForm() {
  formRef.value?.resetFields()
  Object.assign(formData, {
    id: null,
    userName: '',
    password: '',
    nickName: '',
    email: '',
    status: 1,
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
      await userApi.updateUser(formData)
      ElMessage.success('用户更新成功')
    } else {
      await userApi.createUser(formData)
      ElMessage.success('用户创建成功')
    }
    dialogVisible.value = false
    loadUsers()
  } catch (error) {
    ElMessage.error(error.message || '操作失败')
  } finally {
    submitLoading.value = false
  }
}

// 删除用户
async function handleDelete(row) {
  await deleteLock.forKey(row.id, async () => {
    try {
      await ElMessageBox.confirm(`确定要删除用户 "${row.userName}" 吗？`, '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
      await userApi.deleteUser(row.id)
      ElMessage.success('用户删除成功')
      loadUsers()
    } catch (error) {
      if (error !== 'cancel') {
        ElMessage.error(error.message || '删除失败')
      }
    }
  })()
}

// 显示角色分配对话框
async function showRoleDialog(row) {
  currentUser.value = row
  roleDialogVisible.value = true

  // 加载所有角色
  if (allRoles.value.length === 0) {
    try {
      const res = await getRoleList()
      allRoles.value = res.list || res || []
    } catch (error) {
      ElMessage.error('加载角色列表失败')
    }
  }

  // 加载用户当前角色
  try {
    const res = await userApi.getUserRoles(row.id)
    selectedRoleIds.value = res.map(r => r.id)
  } catch (error) {
    selectedRoleIds.value = []
  }
}

// 分配角色
async function handleAssignRoles() {
  roleLoading.value = true
  try {
    await userApi.assignUserRoles(currentUser.value.id, selectedRoleIds.value)
    ElMessage.success('角色分配成功')
    roleDialogVisible.value = false
  } catch (error) {
    ElMessage.error(error.message || '角色分配失败')
  } finally {
    roleLoading.value = false
  }
}

onMounted(() => {
  loadUsers()
})
</script>

<style lang="scss" scoped>
.role-dialog-content {
  .current-user-badge {
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

  .el-checkbox-group {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }
}
</style>

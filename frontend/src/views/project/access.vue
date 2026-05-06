<template>
  <ManagePageLayout title="项目成员管理" subtitle="管理各项目的访问成员">
    <template #actions>
      <el-select v-model="selectedProjectId" placeholder="选择项目筛选" clearable style="width: 200px">
        <el-option
          v-for="project in projects"
          :key="project.id"
          :label="project.name"
          :value="project.id"
        />
      </el-select>
      <el-button type="primary" v-if="userStore.hasPermission('project-access:grant')" @click="showGrantDialog">
        <el-icon><Plus /></el-icon>
        添加成员
      </el-button>
    </template>

    <!-- 成员列表 -->
    <div class="table-card">
      <el-table :data="filteredAccessList" v-loading="loading" border>
        <el-table-column prop="userName" label="用户名" width="140" />
        <el-table-column prop="nickName" label="昵称" width="120" />
        <el-table-column prop="projectName" label="项目" min-width="180" show-overflow-tooltip />
        <el-table-column label="访问类型" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.accessType === 'manage' ? 'success' : ''" size="small">
              {{ row.accessType === 'manage' ? '管理' : '查看' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="grantedByName" label="授权人" width="100" />
        <el-table-column prop="createTime" label="授权时间" width="180" />
        <el-table-column label="操作" width="80" align="center">
          <template #default="{ row }">
            <el-button link type="danger" size="small" v-if="userStore.hasPermission('project-access:revoke')" :loading="revokeLock.isLoading(row.id)" :disabled="revokeLock.isLoading(row.id)" @click="handleRevoke(row)">移除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 添加成员对话框 -->
    <el-dialog
      v-model="grantDialogVisible"
      title="添加项目成员"
      width="500px"
    >
      <el-form ref="grantFormRef" :model="grantFormData" :rules="grantRules" label-width="80px">
        <el-form-item label="选择用户" prop="userId">
          <el-select v-model="grantFormData.userId" placeholder="请选择用户" filterable style="width: 100%">
            <el-option
              v-for="user in userList"
              :key="user.id"
              :label="user.nickName || user.userName"
              :value="user.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="选择项目" prop="projectId">
          <el-select v-model="grantFormData.projectId" placeholder="请选择项目" style="width: 100%">
            <el-option
              v-for="project in projects"
              :key="project.id"
              :label="project.name"
              :value="project.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="访问类型" prop="accessType">
          <el-radio-group v-model="grantFormData.accessType">
            <el-radio label="view">查看</el-radio>
            <el-radio label="manage">管理</el-radio>
          </el-radio-group>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="grantDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="grantLoading" :disabled="grantLoading" @click="handleGrant">确定</el-button>
      </template>
    </el-dialog>
  </ManagePageLayout>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import ManagePageLayout from '@/components/ManagePageLayout/index.vue'
import { projectAccessApi } from '@/api/modules/manager/project-access'
import { managerMenuApi } from '@/api/modules/manager/menu'
import { userApi } from '@/api/modules/manager/user'
import { useUserStore } from '@/stores/user'
import { useAsyncLock } from '@/composables/useAsyncLock'

const userStore = useUserStore()

const revokeLock = useAsyncLock()

/** 访问授权项类型 */
interface AccessItem {
  id: number
  userId: number
  userName: string
  nickName: string
  projectId: number
  projectName: string
  accessType: string
  grantedByName: string
  createTime: string
}

/** 项目简要信息 */
interface ProjectBrief {
  id: number
  name: string
  [key: string]: unknown
}

/** 用户简要信息 */
interface UserBrief {
  id: number
  userName: string
  nickName: string
  [key: string]: unknown
}

// 数据状态
const loading = ref<boolean>(false)
const accessList = ref<AccessItem[]>([])
const projects = ref<ProjectBrief[]>([])
const selectedProjectId = ref<number | null>(null)

// 用户列表
const userList = ref<UserBrief[]>([])

/** 过滤后的访问列表 */
const filteredAccessList = computed<AccessItem[]>(() => {
  if (!selectedProjectId.value) {
    return accessList.value
  }
  return accessList.value.filter(a => a.projectId === selectedProjectId.value)
})

// 添加成员对话框
const grantDialogVisible = ref<boolean>(false)
const grantLoading = ref<boolean>(false)
const grantFormRef = ref<FormInstance>()
const grantFormData = reactive<{
  userId: number | null
  projectId: number | null
  accessType: string
}>({
  userId: null,
  projectId: null,
  accessType: 'view'
})

/** 表单验证规则 */
const grantRules: FormRules = {
  userId: [{ required: true, message: '请选择用户', trigger: 'change' }],
  projectId: [{ required: true, message: '请选择项目', trigger: 'change' }],
  accessType: [{ required: true, message: '请选择访问类型', trigger: 'change' }]
}

/** 加载项目列表 */
async function loadProjects(): Promise<void> {
  try {
    const res = await managerMenuApi.getGroupList()
    const groups = Array.isArray(res) ? res : []
    projects.value = groups
  } catch (error: unknown) {
    console.error('加载项目列表失败:', error)
  }
}

/** 加载用户列表 */
async function loadUsers(): Promise<void> {
  try {
    const res = await userApi.getUserList({ page: 1, pageSize: 100 })
    userList.value = res.list || []
  } catch (error: unknown) {
    console.error('加载用户列表失败:', error)
  }
}

/** 加载成员列表 */
async function loadAccessList(): Promise<void> {
  loading.value = true
  try {
    const res = await projectAccessApi.getAccessList({ page: 1, pageSize: 100 })
    accessList.value = res.list || []
  } catch (error: unknown) {
    const err = error as Error
    ElMessage.error(err.message || '加载成员列表失败')
  } finally {
    loading.value = false
  }
}

/** 显示添加成员对话框 */
function showGrantDialog(): void {
  grantFormData.projectId = selectedProjectId.value
  grantDialogVisible.value = true
}

/** 添加成员 */
async function handleGrant(): Promise<void> {
  try {
    await grantFormRef.value?.validate()
  } catch {
    return
  }

  grantLoading.value = true
  try {
    await projectAccessApi.grantAccess(grantFormData)
    ElMessage.success('添加成员成功')
    grantDialogVisible.value = false
    loadAccessList()
  } catch (error: unknown) {
    const err = error as Error
    ElMessage.error(err.message || '添加成员失败')
  } finally {
    grantLoading.value = false
  }
}

/** 移除成员 */
async function handleRevoke(row: AccessItem): Promise<void> {
  await revokeLock.forKey(row.id, async () => {
    try {
      await ElMessageBox.confirm(`确定要移除成员 "${row.userName}" 吗？`, '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
      await projectAccessApi.revokeAccess(row.id)
      ElMessage.success('移除成员成功')
      loadAccessList()
    } catch (error: unknown) {
      if (error !== 'cancel') {
        const err = error as Error
        ElMessage.error(err.message || '移除成员失败')
      }
    }
  })()
}

onMounted(() => {
  loadProjects()
  loadUsers()
  loadAccessList()
})
</script>

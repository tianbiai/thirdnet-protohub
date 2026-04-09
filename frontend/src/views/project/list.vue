<template>
  <ManagePageLayout title="项目管理" subtitle="管理项目、子项和成员配置">
    <template #actions>
      <el-input
        v-model="searchKeyword"
        placeholder="搜索项目名称"
        clearable
        style="width: 240px"
      >
        <template #prefix>
          <el-icon><Search /></el-icon>
        </template>
      </el-input>
      <el-button type="primary" v-if="userStore.hasPermission('projects:create')" @click="handleCreateProject">
        <el-icon><Plus /></el-icon>
        新建项目
      </el-button>
    </template>

    <!-- 项目卡片网格 -->
    <div v-loading="loading" class="project-grid">
      <div
        v-for="project in filteredProjects"
        :key="project.id"
        class="project-card"
      >
        <!-- 项目头部 -->
        <div class="project-header">
          <div class="project-icon"><img src="@/assets/logo.png" alt="" class="project-logo-img" /></div>
          <div class="project-info">
            <h3 class="project-name">{{ project.name }}</h3>
            <p class="project-desc">{{ project.description }}</p>
          </div>
          <div class="project-actions">
            <el-tag size="small" type="info">{{ project.items?.length || 0 }} 个子项</el-tag>
            <el-dropdown trigger="click" @command="(cmd) => handleProjectCommand(cmd, project)">
              <el-button type="primary" text size="small" class="action-btn">
                <el-icon><MoreFilled /></el-icon>
              </el-button>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="edit" v-if="userStore.hasPermission('projects:update')">
                    <el-icon><Edit /></el-icon>
                    编辑项目
                  </el-dropdown-item>
                  <el-dropdown-item command="members" v-if="userStore.hasPermission('project-access:grant')">
                    <el-icon><User /></el-icon>
                    成员管理
                  </el-dropdown-item>
                  <el-dropdown-item command="delete" divided v-if="userStore.hasPermission('projects:delete')">
                    <el-icon><Delete /></el-icon>
                    删除项目
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
        </div>

        <!-- 项目子项列表 -->
        <div class="project-items">
          <div class="items-header">
            <span>项目子项</span>
            <el-button type="primary" text size="small" v-if="userStore.hasPermission('projects:manage-item')" @click="handleCreateItem(project)">
              <el-icon><Plus /></el-icon>
              添加子项
            </el-button>
          </div>
          <div class="items-list">
            <div
              v-for="(item, index) in project.items"
              :key="item.id"
              class="item-row"
            >
              <div class="item-left">
                <TypeIcon :type="item.type" />
                <div class="item-info">
                  <span class="item-name">{{ item.name }}</span>
                  <span class="item-desc">{{ item.description }}</span>
                </div>
              </div>
              <div class="item-right">
                <el-button type="primary" text size="small" v-if="userStore.hasPermission('projects:manage-item') && index > 0" @click="handleMoveItem(project, item, index, 'up')">
                  <el-icon><Top /></el-icon>
                </el-button>
                <el-button type="primary" text size="small" v-if="userStore.hasPermission('projects:manage-item') && index < project.items.length - 1" @click="handleMoveItem(project, item, index, 'down')">
                  <el-icon><Bottom /></el-icon>
                </el-button>
                <el-button type="primary" text size="small" v-if="userStore.hasPermission('projects:manage-item')" @click="handleEditItem(project, item)">
                  <el-icon><Edit /></el-icon>
                </el-button>
                <el-button type="danger" text size="small" v-if="userStore.hasPermission('projects:manage-item')" :loading="deleteItemLock.isLoading(item.id)" :disabled="deleteItemLock.isLoading(item.id)" @click="handleDeleteItem(project, item)">
                  <el-icon><Delete /></el-icon>
                </el-button>
              </div>
            </div>
          </div>
        </div>

        <!-- 空子项提示 -->
        <div v-if="!project.items || project.items.length === 0" class="empty-items">
          <span>暂无项目子项，</span>
          <el-button type="primary" text size="small" v-if="userStore.hasPermission('projects:manage-item')" @click="handleCreateItem(project)">添加子项</el-button>
        </div>
      </div>
    </div>

    <!-- 空状态 -->
    <el-empty
      v-if="!loading && filteredProjects.length === 0"
      description="暂无项目"
    />

    <!-- 项目编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="editingProject ? '编辑项目' : '新建项目'"
      width="560px"
      destroy-on-close
    >
      <el-form
        ref="formRef"
        :model="projectForm"
        :rules="formRules"
        label-width="80px"
      >
        <el-form-item label="项目名称" prop="name">
          <el-input v-model="projectForm.name" placeholder="请输入项目名称" />
        </el-form-item>
        <el-form-item label="项目描述" prop="description">
          <el-input
            v-model="projectForm.description"
            type="textarea"
            :rows="3"
            placeholder="请输入项目描述"
          />
        </el-form-item>
        <el-form-item label="排序" prop="order">
          <el-input-number v-model="projectForm.order" :min="1" :max="999" />
        </el-form-item>

        <!-- 成员管理（仅新建时显示） -->
        <el-form-item v-if="!editingProject" label="项目成员" prop="members" required>
          <div class="members-section">
            <!-- 已添加的成员 -->
            <div class="member-list">
              <div
                v-for="(member, index) in projectForm.members"
                :key="member.userId"
                class="member-row"
              >
                <span class="member-name">{{ member.nickName || member.userName }}</span>
                <el-tag v-if="member.isCreator" size="small" type="warning">创建者</el-tag>
                <el-select
                  v-model="member.accessType"
                  size="small"
                  style="width: 90px"
                  :disabled="member.isCreator"
                >
                  <el-option label="管理" value="manage" />
                  <el-option label="查看" value="view" />
                </el-select>
                <el-button
                  v-if="!member.isCreator"
                  type="danger"
                  text
                  size="small"
                  @click="removeMember(index)"
                >
                  <el-icon><Delete /></el-icon>
                </el-button>
              </div>
            </div>
            <!-- 添加成员 -->
            <div class="add-member-row">
              <el-select
                v-model="newMemberUserId"
                placeholder="选择用户"
                size="small"
                filterable
                style="flex: 1"
              >
                <el-option
                  v-for="user in availableNewMembers"
                  :key="user.id"
                  :label="user.nickName || user.userName"
                  :value="user.id"
                />
              </el-select>
              <el-select v-model="newMemberAccessType" size="small" style="width: 90px">
                <el-option label="管理" value="manage" />
                <el-option label="查看" value="view" />
              </el-select>
              <el-button type="primary" size="small" @click="addMember">添加</el-button>
            </div>
          </div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitLock.loading" :disabled="submitLock.loading" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>

    <!-- 成员管理对话框 -->
    <el-dialog
      v-model="memberDialogVisible"
      :title="`${currentProject?.name || ''} - 成员管理`"
      width="700px"
      destroy-on-close
    >
      <div class="member-dialog-content">
        <!-- 添加成员 -->
        <div class="add-member-section">
          <el-form :inline="true" :model="addMemberForm" class="add-member-form">
            <el-form-item label="用户">
              <el-select v-model="addMemberForm.userId" placeholder="选择用户" style="width: 160px">
                <el-option
                  v-for="user in availableUsers"
                  :key="user.id"
                  :label="user.nickName"
                  :value="user.id"
                />
              </el-select>
            </el-form-item>
            <el-form-item label="权限">
              <el-select v-model="addMemberForm.accessType" style="width: 120px">
                <el-option label="查看" value="view" />
                <el-option label="管理" value="manage" />
              </el-select>
            </el-form-item>
            <el-form-item>
              <el-button type="primary" :loading="addMemberLock.loading" :disabled="addMemberLock.loading" @click="handleAddMember">添加</el-button>
            </el-form-item>
          </el-form>
        </div>

        <!-- 成员列表 -->
        <el-table :data="projectMembers" v-loading="memberLoading" border>
          <el-table-column prop="nickName" label="用户" width="120" />
          <el-table-column prop="userName" label="用户名" width="120" />
          <el-table-column label="权限类型" width="100" align="center">
            <template #default="{ row }">
              <el-tag :type="row.accessType === 'manage' ? 'success' : 'info'" size="small">
                {{ row.accessType === 'manage' ? '管理' : '查看' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="grantedByName" label="授权人" width="100" />
          <el-table-column prop="createTime" label="授权时间" min-width="160" />
          <el-table-column label="操作" width="80" align="center">
            <template #default="{ row }">
              <el-button type="danger" text size="small" :loading="removeMemberLock.isLoading(row.id)" :disabled="removeMemberLock.isLoading(row.id)" @click="handleRemoveMember(row)">
                移除
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </el-dialog>

    <!-- 子项编辑对话框 -->
    <el-dialog
      v-model="itemDialogVisible"
      :title="editingItem ? '编辑子项' : '新建子项'"
      width="560px"
      destroy-on-close
    >
      <el-form
        ref="itemFormRef"
        :model="itemForm"
        :rules="itemFormRules"
        label-width="80px"
      >
        <el-form-item label="名称" prop="name">
          <el-input v-model="itemForm.name" placeholder="请输入子项名称" />
        </el-form-item>
        <el-form-item label="类型" prop="type">
          <el-select v-model="itemForm.type" placeholder="请选择类型" style="width: 100%">
            <el-option label="Web 应用" value="web" />
            <el-option label="小程序应用" value="miniprogram" />
            <el-option label="超链接" value="link" />
            <el-option label="变更日志" value="changelog" />
          </el-select>
        </el-form-item>
        <el-form-item label="链接地址" prop="url">
          <el-input
            v-model="itemForm.url"
            :placeholder="itemForm.type === 'changelog' ? '请输入日志 URL 地址' : '请输入 URL 地址'"
          />
        </el-form-item>
        <el-form-item label="描述" prop="description">
          <el-input v-model="itemForm.description" type="textarea" :rows="2" placeholder="请输入描述" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="itemDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitItemLock.loading" :disabled="submitItemLock.loading" @click="handleSubmitItem">确定</el-button>
      </template>
    </el-dialog>
  </ManagePageLayout>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Plus, MoreFilled, Edit, Delete, User, Upload, Top, Bottom } from '@element-plus/icons-vue'
import ManagePageLayout from '@/components/ManagePageLayout/index.vue'
import { getMenuConfig, getGroupList as getProjectList, createGroup as createProject, updateGroup as updateProject, deleteGroup as deleteProject, createItem, updateItem, deleteItem, reorderItems } from '@/api/menu'
import { getAccessList, grantAccess, revokeAccess } from '@/api/project-access'
import { getUserList } from '@/api/user'
import { getTypeTag } from '@/utils/menu'
import TypeIcon from '@/components/TypeIcon/index.vue'
import { useUserStore } from '@/stores/user'
import { useMenuStore } from '@/stores/menu'
import { useAsyncLock } from '@/composables/useAsyncLock'

const userStore = useUserStore()
const menuStore = useMenuStore()

// 异步操作锁
const submitLock = useAsyncLock()
const submitItemLock = useAsyncLock()
const addMemberLock = useAsyncLock()
const removeMemberLock = useAsyncLock()
const deleteItemLock = useAsyncLock()
const deleteProjectLock = useAsyncLock()

// 数据状态
const loading = ref(false)
const projects = ref([])
const searchKeyword = ref('')
const allUsers = ref([])

// 对话框状态
const dialogVisible = ref(false)
const editingProject = ref(null)
const formRef = ref(null)

// 成员管理状态
const memberDialogVisible = ref(false)
const memberLoading = ref(false)
const currentProject = ref(null)
const projectMembers = ref([])
const addMemberForm = reactive({
  userId: null,
  accessType: 'view'
})

// 表单数据
const projectForm = reactive({
  name: '',
  description: '',
  order: 1,
  members: []
})

// 表单验证规则
const formRules = {
  name: [{ required: true, message: '请输入项目名称', trigger: 'blur' }],
  members: [
    {
      validator: (rule, value, callback) => {
        if (!value || value.length === 0) {
          callback(new Error('请至少添加一名项目成员'))
        } else {
          callback()
        }
      },
      trigger: 'change'
    }
  ]
}

// 新建成员选择状态
const newMemberUserId = ref(null)
const newMemberAccessType = ref('view')

// 可选用户（排除已在项目中的）
const availableUsers = computed(() => {
  const memberIds = projectMembers.value.map(m => m.userId)
  return allUsers.value.filter(u => !memberIds.includes(u.id))
})

// 新建项目时可选的新成员（排除已添加的）
const availableNewMembers = computed(() => {
  const addedIds = projectForm.members.map(m => m.userId)
  return allUsers.value.filter(u => !addedIds.includes(u.id))
})

// 过滤后的项目列表
const filteredProjects = computed(() => {
  if (!searchKeyword.value) {
    return projects.value
  }
  const keyword = searchKeyword.value.toLowerCase()
  return projects.value.filter(p =>
    p.name.toLowerCase().includes(keyword) ||
    p.description.toLowerCase().includes(keyword)
  )
})

// 加载项目列表
async function loadProjects() {
  loading.value = true
  try {
    const res = await getMenuConfig()
    // 应用端接口返回分组数组，包含嵌套的 items
    const groups = Array.isArray(res) ? res : (res.groups || [])
    projects.value = groups.map(g => ({ ...g, items: g.items || g.children || [] }))
  } catch (error) {
    ElMessage.error(error.message || '加载项目列表失败')
  } finally {
    loading.value = false
  }
}

// 加载所有用户
async function loadAllUsers() {
  try {
    const res = await getUserList({ page: 1, pageSize: 100 })
    allUsers.value = res.list || []
  } catch (error) {
    console.error('加载用户列表失败:', error)
  }
}

// 新建项目
function handleCreateProject() {
  editingProject.value = null
  // 默认将当前用户添加为创建者（管理员）
  const currentUser = allUsers.value.find(u => u.id === userStore.userInfo?.id)
  projectForm.members = currentUser
    ? [{ userId: currentUser.id, userName: currentUser.userName, nickName: currentUser.nickName, accessType: 'manage', isCreator: true }]
    : [{ userId: userStore.userInfo?.id, userName: userStore.userInfo?.username, nickName: userStore.userInfo?.nickname, accessType: 'manage', isCreator: true }]
  Object.assign(projectForm, {
    name: '',
    description: '',
    order: projects.value.length + 1
  })
  newMemberUserId.value = null
  newMemberAccessType.value = 'view'
  dialogVisible.value = true
}

// 添加成员
function addMember() {
  if (!newMemberUserId.value) {
    ElMessage.warning('请选择用户')
    return
  }
  const user = allUsers.value.find(u => u.id === newMemberUserId.value)
  if (!user) return
  if (projectForm.members.some(m => m.userId === user.id)) {
    ElMessage.warning('该用户已在成员列表中')
    return
  }
  projectForm.members.push({
    userId: user.id,
    userName: user.userName,
    nickName: user.nickName,
    accessType: newMemberAccessType.value,
    isCreator: false
  })
  newMemberUserId.value = null
  newMemberAccessType.value = 'view'
}

// 移除成员
function removeMember(index) {
  projectForm.members.splice(index, 1)
}

// 处理项目操作命令
function handleProjectCommand(command, project) {
  if (command === 'edit') {
    editingProject.value = project
    Object.assign(projectForm, {
      name: project.name,
      description: project.description,
      order: project.order
    })
    dialogVisible.value = true
  } else if (command === 'members') {
    openMemberDialog(project)
  } else if (command === 'delete') {
    deleteProjectLock.wrap(async () => {
      await ElMessageBox.confirm(
        `确定要删除项目"${project.name}"吗？删除后不可恢复。`,
        '删除确认',
        {
          confirmButtonText: '删除',
          cancelButtonText: '取消',
          type: 'warning'
        }
      )
      try {
        await deleteProject(project.id)
        ElMessage.success('删除成功')
        loadProjects()
      } catch (error) {
        ElMessage.error(error.message || '删除失败')
      }
    })().catch(() => {})
  }
}

// 打开成员管理对话框
async function openMemberDialog(project) {
  currentProject.value = project
  addMemberForm.userId = null
  addMemberForm.accessType = 'view'
  memberDialogVisible.value = true
  await loadProjectMembers()
}

// 加载项目成员
async function loadProjectMembers() {
  if (!currentProject.value) return
  memberLoading.value = true
  try {
    const res = await getAccessList({ projectId: currentProject.value.id })
    projectMembers.value = res.list || []
  } catch (error) {
    ElMessage.error(error.message || '加载成员列表失败')
  } finally {
    memberLoading.value = false
  }
}

// 添加成员
const handleAddMember = addMemberLock.wrap(async () => {
  if (!addMemberForm.userId) {
    ElMessage.warning('请选择用户')
    return
  }
  try {
    await grantAccess({
      userId: addMemberForm.userId,
      projectId: currentProject.value.id,
      accessType: addMemberForm.accessType
    })
    ElMessage.success('添加成功')
    addMemberForm.userId = null
    addMemberForm.accessType = 'view'
    loadProjectMembers()
  } catch (error) {
    ElMessage.error(error.message || '添加失败')
  }
})

// 移除成员
async function handleRemoveMember(member) {
  await removeMemberLock.forKey(member.id, async () => {
    await ElMessageBox.confirm(
      `确定要移除成员"${member.nickName}"吗？`,
      '移除确认',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    try {
      await revokeAccess(member.id)
      ElMessage.success('移除成功')
      loadProjectMembers()
    } catch (error) {
      ElMessage.error(error.message || '移除失败')
    }
  })()
}

// 提交表单
const handleSubmit = submitLock.wrap(async () => {
  try {
    await formRef.value.validate()
    if (editingProject.value) {
      await updateProject({ id: editingProject.value.id, ...projectForm })
      ElMessage.success('更新成功')
    } else {
      // 新建项目时带上成员列表
      const { members, ...projectData } = projectForm
      await createProject({
        ...projectData,
        members: members.map(m => ({ user_id: m.userId, access_type: m.accessType }))
      })
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    loadProjects()
    menuStore.loadMenuConfig(true)
  } catch (error) {
    if (error !== false) {
      ElMessage.error(error.message || '操作失败')
    }
  }
})

// 子项管理状态
const itemDialogVisible = ref(false)
const editingItem = ref(null)
const currentItemProject = ref(null)
const itemFormRef = ref(null)
const itemForm = reactive({
  name: '',
  type: 'web',
  url: '',
  description: ''
})

const itemFormRules = {
  name: [{ required: true, message: '请输入子项名称', trigger: 'blur' }],
  type: [{ required: true, message: '请选择类型', trigger: 'change' }]
}

// 新建子项
function handleCreateItem(project) {
  currentItemProject.value = project
  editingItem.value = null
  Object.assign(itemForm, {
    name: '',
    type: 'web',
    url: '',
    description: ''
  })
  itemDialogVisible.value = true
}

// 编辑子项
function handleEditItem(project, item) {
  currentItemProject.value = project
  editingItem.value = item
  Object.assign(itemForm, {
    name: item.name,
    type: item.type,
    url: item.url || '',
    description: item.description || ''
  })
  itemDialogVisible.value = true
}

// 删除子项
async function handleDeleteItem(project, item) {
  await deleteItemLock.forKey(item.id, async () => {
    await ElMessageBox.confirm(
      `确定要删除子项"${item.name}"吗？`,
      '删除确认',
      {
        confirmButtonText: '删除',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    try {
      await deleteItem(item.id)
      ElMessage.success('删除成功')
      loadProjects()
      menuStore.loadMenuConfig(true)
    } catch (error) {
      ElMessage.error(error.message || '删除失败')
    }
  })()
}

// 移动子项排序
async function handleMoveItem(project, item, index, direction) {
  const items = [...project.items]
  const targetIndex = direction === 'up' ? index - 1 : index + 1
  // 交换位置
  ;[items[index], items[targetIndex]] = [items[targetIndex], items[index]]
  try {
    await reorderItems(project.id, items)
    loadProjects()
    menuStore.loadMenuConfig(true)
  } catch (error) {
    ElMessage.error(error.message || '排序失败')
  }
}

// 提交子项表单
const handleSubmitItem = submitItemLock.wrap(async () => {
  try {
    await itemFormRef.value.validate()

    const submitData = { ...itemForm }

    if (editingItem.value) {
      await updateItem({
        id: editingItem.value.id,
        group_id: currentItemProject.value.id,
        ...submitData
      })
      ElMessage.success('更新成功')
    } else {
      await createItem({
        group_id: currentItemProject.value.id,
        ...submitData
      })
      ElMessage.success('创建成功')
    }
    itemDialogVisible.value = false
    loadProjects()
    menuStore.loadMenuConfig(true)
  } catch (error) {
    if (error !== false) {
      ElMessage.error(error.message || '操作失败')
    }
  }
})

onMounted(() => {
  loadProjects()
  loadAllUsers()
})
</script>

<style lang="scss" scoped>
:deep(.manage-page-wrapper) {
  padding: 24px;
}

.project-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(480px, 1fr));
  gap: 20px;
}

.project-card {
  background: var(--bg-primary);
  border-radius: var(--radius-xl);
  overflow: hidden;
  border: 1px solid var(--border-light);
  transition: all var(--transition-fast);

  &:hover {
    box-shadow: var(--shadow-lg);
    border-color: var(--primary-light);
  }
}

.project-header {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px 24px;
  background: linear-gradient(135deg, var(--bg-secondary), var(--bg-primary));
  border-bottom: 1px solid var(--border-lighter);

  .project-icon {
    width: 48px;
    height: 48px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--bg-primary);
    border-radius: var(--radius-lg);
    flex-shrink: 0;

    .project-logo-img {
      width: 28px;
      height: 28px;
      object-fit: contain;
    }
  }

  .project-info {
    flex: 1;
    min-width: 0;

    .project-name {
      font-size: 16px;
      font-weight: 600;
      color: var(--text-primary);
      margin: 0 0 4px 0;
    }

    .project-desc {
      font-size: var(--font-size-sm);
      color: var(--text-secondary);
      margin: 0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
  }

  .project-actions {
    display: flex;
    align-items: center;
    gap: 8px;
    flex-shrink: 0;

    .action-btn {
      padding: 4px;
    }
  }
}

.project-items {
  .items-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 12px 24px;
    font-size: var(--font-size-xs);
    font-weight: 500;
    color: var(--text-tertiary);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    background: var(--bg-tertiary);
  }

  .items-list {
    max-height: 280px;
    overflow-y: auto;
  }
}

.item-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 24px;
  border-bottom: 1px solid var(--border-lighter);

  &:last-child {
    border-bottom: none;
  }

  .item-left {
    display: flex;
    align-items: center;
    gap: 12px;
    flex: 1;
    min-width: 0;

    .item-info {
      display: flex;
      flex-direction: column;
      gap: 2px;
      min-width: 0;

      .item-name {
        font-size: var(--font-size-sm);
        font-weight: 500;
        color: var(--text-primary);
      }

      .item-desc {
        font-size: var(--font-size-xs);
        color: var(--text-tertiary);
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
    }
  }

  .item-right {
    display: flex;
    align-items: center;
    gap: 8px;
    flex-shrink: 0;
  }

  .item-badge {
    flex-shrink: 0;
    font-size: 9px;
    font-weight: var(--font-weight-semibold);
    padding: 2px 6px;
    border-radius: var(--radius-xs);
    letter-spacing: 0.02em;
    border: 1px solid;
    background: transparent;
  }
}

.empty-items {
  padding: 24px;
  text-align: center;
  color: var(--text-tertiary);
  font-size: var(--font-size-sm);
}

// 成员管理对话框
.member-dialog-content {
  .add-member-section {
    margin-bottom: 16px;
    padding-bottom: 16px;
    border-bottom: 1px solid var(--border-lighter);

    .add-member-form {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 8px;

      .el-form-item {
        margin-bottom: 0;
      }
    }
  }
}

// 新建项目 - 成员区域
.members-section {
  width: 100%;

  .member-list {
    margin-bottom: 8px;

    .member-row {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 6px 0;

      .member-name {
        flex: 1;
        font-size: var(--font-size-sm);
        color: var(--text-primary);
      }
    }
  }

  .add-member-row {
    display: flex;
    align-items: center;
    gap: 8px;
    padding-top: 8px;
    border-top: 1px dashed var(--border-lighter);
  }
}

// 响应式
@media (max-width: 1024px) {
  .project-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 600px) {
  :deep(.manage-page-wrapper) {
    padding: 16px;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;

    .header-actions {
      width: 100%;
      flex-direction: column;

      .el-input {
        width: 100% !important;
      }

      .el-button {
        width: 100%;
      }
    }
  }

  .project-header {
    flex-wrap: wrap;
    padding: 16px;

    .project-info {
      order: 3;
      width: 100%;
      margin-top: 8px;
    }
  }
}

// 深色主题
[data-theme="dark"] {
  .project-card {
    background: var(--bg-secondary);
  }

  .project-header {
    background: linear-gradient(135deg, var(--bg-tertiary), var(--bg-secondary));
  }
}
</style>

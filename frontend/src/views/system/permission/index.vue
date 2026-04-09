<template>
  <ManagePageLayout title="权限管理" subtitle="查看系统权限定义">
    <template #actions>
      <el-input
        v-model="searchKeyword"
        placeholder="搜索权限编码/名称"
        clearable
        style="width: 220px"
        @keyup.enter="loadPermissions"
      >
        <template #prefix>
          <el-icon><Search /></el-icon>
        </template>
      </el-input>
    </template>

    <!-- 权限列表 -->
    <div class="table-card">
      <el-table :data="filteredPermissions" v-loading="loading" border>
        <el-table-column prop="code" label="权限编码" width="200" />
        <el-table-column prop="name" label="权限名称" width="160" />
        <el-table-column prop="category" label="分类" width="120" align="center">
          <template #default="{ row }">
            <el-tag size="small" effect="light" type="info">{{ row.category }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="description" label="描述" min-width="240" show-overflow-tooltip />
      </el-table>
    </div>

    <el-empty v-if="!loading && filteredPermissions.length === 0" description="暂无权限数据" />
  </ManagePageLayout>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Search } from '@element-plus/icons-vue'
import ManagePageLayout from '@/components/ManagePageLayout/index.vue'
import { getPermissionList } from '@/api/permission'

// 数据状态
const loading = ref(false)
const permissions = ref([])
const searchKeyword = ref('')

// 过滤后的权限列表
const filteredPermissions = computed(() => {
  if (!searchKeyword.value) {
    return permissions.value
  }
  const keyword = searchKeyword.value.toLowerCase()
  return permissions.value.filter(p =>
    p.code.toLowerCase().includes(keyword) ||
    p.name.toLowerCase().includes(keyword)
  )
})

// 加载权限列表
async function loadPermissions() {
  loading.value = true
  try {
    const res = await getPermissionList()
    permissions.value = res.list || res || []
  } catch (error) {
    ElMessage.error(error.message || '加载权限列表失败')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadPermissions()
})
</script>

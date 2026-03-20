<template>
  <div class="project-list-page">
    <!-- 页面头部 -->
    <div class="page-header">
      <h2>项目列表</h2>
      <HelpBubble
        title="项目列表帮助"
        :items="[
          { content: '展示所有已配置的项目' },
          { content: '点击项目可查看详情或打开项目' }
        ]"
      />
    </div>

    <!-- 项目分组 -->
    <div v-for="group in menuStore.groups" :key="group.id" class="project-section">
      <div class="section-header">
        <span class="section-icon">{{ group.icon }}</span>
        <h3>{{ group.name }}</h3>
        <span class="section-count">{{ group.children?.length || 0 }} 个项目</span>
      </div>

      <div class="project-grid">
        <div
          v-for="item in group.children"
          :key="item.id"
          class="project-card"
          @click="handleItemClick(item)"
        >
          <div class="card-header">
            <span class="project-icon">{{ item.icon }}</span>
            <el-tag size="small" :type="getTypeTag(item.type).type">
              {{ getTypeTag(item.type).label }}
            </el-tag>
          </div>

          <h4 class="project-name">{{ item.name }}</h4>
          <p class="project-desc">{{ item.description || '暂无描述' }}</p>

          <div class="card-footer">
            <template v-if="item.type === 'web' || item.type === 'mobile'">
              <el-icon><Link /></el-icon>
              <span>{{ item.url }}</span>
            </template>
            <template v-else-if="item.type === 'doc'">
              <el-icon><Document /></el-icon>
              <span>{{ item.docPath }}</span>
            </template>
            <template v-else>
              <el-icon><Position /></el-icon>
              <span>{{ item.route }}</span>
            </template>
          </div>
        </div>
      </div>
    </div>

    <!-- 空状态 -->
    <el-empty
      v-if="menuStore.groups.length === 0"
      description="暂无项目配置"
    >
      <el-button type="primary" @click="$router.push('/menu-editor')">
        去配置菜单
      </el-button>
    </el-empty>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useMenuStore } from '@/stores/menu'
import HelpBubble from '@/components/HelpBubble/index.vue'

const router = useRouter()
const menuStore = useMenuStore()

// 获取类型标签
function getTypeTag(type) {
  const types = {
    web: { label: 'Web', type: '' },
    mobile: { label: '移动端', type: 'success' },
    doc: { label: '文档', type: 'info' },
    internal: { label: '内部', type: 'warning' }
  }
  return types[type] || types.web
}

// 处理点击
function handleItemClick(item) {
  if (item.type === 'internal' && item.route) {
    router.push(item.route)
  } else {
    router.push(`/content/${item.type}/${item.id}`)
  }
}
</script>

<style lang="scss" scoped>
.project-list-page {
  max-width: 1200px;
}

.page-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 24px;

  h2 {
    font-size: 24px;
    font-weight: 600;
    color: var(--text-primary);
  }
}

.project-section {
  margin-bottom: 32px;
}

.section-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;

  .section-icon {
    font-size: 24px;
  }

  h3 {
    font-size: 18px;
    font-weight: 600;
    color: var(--text-primary);
  }

  .section-count {
    font-size: var(--font-size-sm);
    color: var(--text-tertiary);
  }
}

.project-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
}

.project-card {
  padding: 20px;
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  cursor: pointer;
  transition: all 0.2s;

  &:hover {
    box-shadow: var(--shadow-md);
    transform: translateY(-2px);
  }

  .card-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 12px;

    .project-icon {
      font-size: 32px;
    }
  }

  .project-name {
    font-size: 16px;
    font-weight: 600;
    color: var(--text-primary);
    margin-bottom: 8px;
  }

  .project-desc {
    font-size: var(--font-size-sm);
    color: var(--text-secondary);
    margin-bottom: 16px;
    line-height: 1.5;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }

  .card-footer {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: var(--font-size-xs);
    color: var(--text-tertiary);

    span {
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
  }
}

@media (max-width: 1200px) {
  .project-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>

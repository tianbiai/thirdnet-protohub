<template>
  <div class="menu-editor-page">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-left">
        <h2>菜单编辑器</h2>
        <HelpBubble
          title="菜单编辑器帮助"
          :items="[
            { label: '功能', content: '可视化编辑侧边栏菜单配置' },
            { label: '操作', content: '支持添加、编辑、删除分组和菜单项' },
            { label: '排序', content: '拖拽分组或菜单项可调整顺序' },
            { label: '预览', content: '右侧实时预览菜单效果' }
          ]"
        />
      </div>
      <div class="header-right">
        <el-button @click="resetConfig">
          <el-icon><RefreshRight /></el-icon>
          重置
        </el-button>
      </div>
    </div>

    <!-- 编辑器主体 -->
    <MenuEditor />
  </div>
</template>

<script setup>
import { ElMessage, ElMessageBox } from 'element-plus'
import { useMenuStore } from '@/stores/menu'
import HelpBubble from '@/components/HelpBubble/index.vue'
import MenuEditor from '@/components/MenuEditor/index.vue'

const menuStore = useMenuStore()

// 重置配置
async function resetConfig() {
  try {
    await ElMessageBox.confirm('确定要重置为默认配置吗？当前修改将丢失。', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    menuStore.resetToDefault()
    ElMessage.success('已重置为默认配置')
  } catch {
    // 取消操作
  }
}
</script>

<style lang="scss" scoped>
.menu-editor-page {
  height: 100%;
  display: flex;
  flex-direction: column;
  background: var(--bg-primary);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid var(--border-light);

  .header-left {
    display: flex;
    align-items: center;
    gap: 12px;

    h2 {
      font-size: 20px;
      font-weight: 600;
      color: var(--text-primary);
    }
  }

  .header-right {
    display: flex;
    gap: 12px;
  }
}
</style>

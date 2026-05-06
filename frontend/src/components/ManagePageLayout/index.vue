<template>
  <div class="manage-page-wrapper">
    <div class="manage-page-header">
      <div class="header-left">
        <div class="back-btn" @click="goHome">
          <el-icon><HomeFilled /></el-icon>
          首页
        </div>
        <div class="page-title-area">
          <h2>{{ title }}</h2>
          <span v-if="subtitle" class="page-subtitle">{{ subtitle }}</span>
        </div>
      </div>
      <div class="header-actions">
        <slot name="actions" />
      </div>
    </div>
    <slot />
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { HomeFilled } from '@element-plus/icons-vue'

/** 管理页面布局组件属性 */
defineProps<{
  /** 页面标题 */
  title: string
  /** 页面副标题 */
  subtitle?: string
}>()

const router = useRouter()

/** 返回首页 */
function goHome(): void {
  router.push('/')
}
</script>

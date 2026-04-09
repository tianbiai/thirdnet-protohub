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

<script setup>
import { useRouter } from 'vue-router'
import { HomeFilled } from '@element-plus/icons-vue'

defineProps({
  title: {
    type: String,
    required: true
  },
  subtitle: {
    type: String,
    default: ''
  }
})

const router = useRouter()
function goHome() {
  router.push('/')
}
</script>

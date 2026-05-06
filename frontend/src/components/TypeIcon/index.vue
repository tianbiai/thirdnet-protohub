<template>
  <span
    class="type-icon-wrapper"
    :style="{ borderColor: getTypeTag(type).color }"
  >
    <el-icon
      :size="size"
      :style="{ color: getTypeTag(type).color }"
    >
      <component :is="typeIconMap[type] || typeIconMap.web" />
    </el-icon>
  </span>
</template>

<script setup lang="ts">
import { Monitor, Cellphone, Link, Notebook } from '@element-plus/icons-vue'
import { getTypeTag } from '@/utils/menu'
import type { Component } from 'vue'

/** 类型图标组件属性 */
defineProps<{
  /** 菜单项类型 */
  type?: string
  /** 图标大小 */
  size?: number
}>()

/** 类型到图标组件的映射表 */
const typeIconMap: Record<string, Component> = { web: Monitor, miniprogram: Cellphone, link: Link, changelog: Notebook }
</script>

<style lang="scss" scoped>
.type-icon-wrapper {
  width: 26px;
  height: 26px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-sm);
  border: 1.5px solid;
  flex-shrink: 0;
  transition: transform var(--transition-spring), box-shadow var(--transition-fast);
  background: transparent;

  &:hover {
    transform: scale(1.1);
  }

  :deep(.el-icon) {
    color: inherit;
  }

  :deep(svg) {
    fill: currentColor !important;
    color: inherit;
  }

  :deep(path) {
    fill: currentColor !important;
  }
}
</style>

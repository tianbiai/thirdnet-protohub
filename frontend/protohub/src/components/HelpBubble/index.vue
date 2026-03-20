<template>
  <div class="help-bubble">
    <el-popover
      :placement="placement"
      :width="width"
      trigger="click"
      popper-class="help-bubble-popper"
    >
      <template #reference>
        <div class="bubble-trigger" :class="{ 'is-active': showTooltip }">
          <el-icon :size="size"><QuestionFilled /></el-icon>
          <span class="trigger-ring"></span>
        </div>
      </template>

      <div class="bubble-content">
        <div v-if="title" class="bubble-header">
          <div class="header-icon">
            <el-icon :size="16"><QuestionFilled /></el-icon>
          </div>
          <h4 class="bubble-title">{{ title }}</h4>
        </div>

        <div class="bubble-body">
          <slot>
            <div
              v-for="(item, index) in items"
              :key="index"
              class="help-item"
              :style="{ animationDelay: `${index * 50}ms` }"
            >
              <div class="item-dot"></div>
              <div class="item-content">
                <strong v-if="item.label" class="item-label">{{ item.label }}</strong>
                <span class="item-text">{{ item.content }}</span>
              </div>
            </div>
          </slot>
        </div>
      </div>
    </el-popover>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  title: {
    type: String,
    default: ''
  },
  items: {
    type: Array,
    default: () => []
  },
  placement: {
    type: String,
    default: 'bottom'
  },
  width: {
    type: [String, Number],
    default: 340
  },
  size: {
    type: Number,
    default: 18
  }
})

const showTooltip = ref(false)
</script>

<style lang="scss" scoped>
.help-bubble {
  display: inline-flex;
}

.bubble-trigger {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: var(--radius-full);
  cursor: pointer;
  color: var(--text-tertiary);
  background: var(--bg-secondary);
  border: 1px solid transparent;
  transition: var(--transition-spring);

  &:hover {
    color: var(--primary-color);
    background: var(--primary-light);
    border-color: var(--primary-200);
    transform: scale(1.05);

    .trigger-ring {
      opacity: 1;
      transform: scale(1.3);
    }
  }

  &:active {
    transform: scale(0.95);
  }

  &.is-active {
    color: var(--primary-color);
    background: var(--primary-light);
  }

  .trigger-ring {
    position: absolute;
    inset: -4px;
    border: 2px solid var(--primary-200);
    border-radius: var(--radius-full);
    opacity: 0;
    transform: scale(1);
    transition: var(--transition-fast);
    pointer-events: none;
  }
}

.bubble-content {
  .bubble-header {
    display: flex;
    align-items: center;
    gap: 10px;
    margin-bottom: 16px;
    padding-bottom: 12px;
    border-bottom: 1px solid var(--border-lighter);

    .header-icon {
      width: 28px;
      height: 28px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--primary-light);
      border-radius: var(--radius-md);
      color: var(--primary-color);
    }

    .bubble-title {
      font-size: var(--font-size-md);
      font-weight: var(--font-weight-semibold);
      color: var(--text-primary);
      letter-spacing: -0.01em;
    }
  }

  .bubble-body {
    font-size: var(--font-size-sm);
    line-height: 1.7;
    color: var(--text-secondary);
  }
}

.help-item {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  padding: 8px 0;
  animation: fadeInLeft var(--duration-normal) var(--ease-out) both;

  &:not(:last-child) {
    border-bottom: 1px dashed var(--border-lighter);
  }

  .item-dot {
    flex-shrink: 0;
    width: 6px;
    height: 6px;
    margin-top: 8px;
    background: var(--primary-color);
    border-radius: var(--radius-full);
  }

  .item-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .item-label {
    font-weight: var(--font-weight-medium);
    color: var(--text-primary);
  }

  .item-text {
    color: var(--text-secondary);
  }
}

@keyframes fadeInLeft {
  from {
    opacity: 0;
    transform: translateX(-8px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}
</style>

<style lang="scss">
// 全局样式 - Popper
.help-bubble-popper {
  background: var(--bg-primary) !important;
  border: 1px solid var(--border-light) !important;
  border-radius: var(--radius-lg) !important;
  box-shadow: var(--shadow-lg) !important;
  padding: 16px 20px !important;

  .el-popper__arrow::before {
    background: var(--bg-primary) !important;
    border-color: var(--border-light) !important;
  }
}
</style>

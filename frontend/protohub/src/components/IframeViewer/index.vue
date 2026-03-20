<template>
  <div class="iframe-viewer" :class="{ 'mobile-mode': isMobile }">
    <!-- 移动端模式：带手机外壳 -->
    <template v-if="isMobile">
      <div class="phone-frame">
        <div class="phone-notch"></div>
        <iframe
          ref="iframeRef"
          :src="src"
          class="mobile-iframe"
          :style="{ width: `${viewport.width}px`, height: `${viewport.height + 100 - 88}px` }"
          frameborder="0"
          allowfullscreen
          @load="onLoad"
          @error="onError"
        />
        <div class="phone-home-bar"></div>
      </div>
    </template>

    <!-- Web 模式 -->
    <template v-else>
      <iframe
        ref="iframeRef"
        :src="src"
        class="web-iframe"
        frameborder="0"
        allowfullscreen
        @load="onLoad"
        @error="onError"
      />
    </template>

    <!-- 加载遮罩 -->
    <transition name="fade">
      <div v-if="loading" class="loading-overlay">
        <el-icon class="is-loading" :size="32"><Loading /></el-icon>
        <span>加载中...</span>
      </div>
    </transition>

    <!-- 错误遮罩 -->
    <transition name="fade">
      <div v-if="error" class="error-overlay">
        <el-icon :size="48" color="var(--el-color-danger)"><WarningFilled /></el-icon>
        <h3>加载失败</h3>
        <p>{{ error }}</p>
        <el-button type="primary" @click="reload">重新加载</el-button>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
  src: {
    type: String,
    required: true
  },
  type: {
    type: String,
    default: 'web'
  },
  viewport: {
    type: Object,
    default: () => ({ width: 375, height: 812 })
  }
})

const emit = defineEmits(['load', 'error'])

const iframeRef = ref(null)
const loading = ref(true)
const error = ref(null)

const isMobile = computed(() => props.type === 'mobile' || props.type === 'miniprogram')

function onLoad() {
  loading.value = false
  error.value = null
  emit('load')
}

function onError() {
  loading.value = false
  error.value = '无法加载页面'
  emit('error', error.value)
}

function reload() {
  if (iframeRef.value) {
    loading.value = true
    error.value = null
    iframeRef.value.src = props.src
  }
}

watch(() => props.src, () => {
  loading.value = true
  error.value = null
})
</script>

<style lang="scss" scoped>
.iframe-viewer {
  position: relative;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--el-bg-color-page);
  overflow: auto;

  &.mobile-mode {
    background: var(--el-bg-color-page);
    padding: 20px;
  }
}

.web-iframe {
  width: 100%;
  height: 100%;
  border: none;
}

// 手机外壳
.phone-frame {
  position: relative;
  background: #1C1C1E;
  border-radius: 44px;
  padding: 12px;
  box-shadow:
    0 0 0 3px #3A3A3C,
    0 0 0 6px #2C2C2E,
    0 25px 50px rgba(0, 0, 0, 0.5);
  flex-shrink: 0;
}

.mobile-iframe {
  border: none;
  border-radius: 32px;
  background: white;
  display: block;
}

.phone-notch {
  position: absolute;
  top: 12px;
  left: 50%;
  transform: translateX(-50%);
  width: 120px;
  height: 28px;
  background: #1C1C1E;
  border-radius: 0 0 18px 18px;
  z-index: 1;
}

.phone-home-bar {
  position: absolute;
  bottom: 8px;
  left: 50%;
  transform: translateX(-50%);
  width: 134px;
  height: 5px;
  background: #3A3A3C;
  border-radius: 3px;
}

.loading-overlay,
.error-overlay {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  background: rgba(255, 255, 255, 0.95);
  z-index: 10;

  h3 {
    font-size: 18px;
    font-weight: 500;
    color: var(--el-text-color-primary);
  }

  p {
    font-size: 14px;
    color: var(--el-text-color-secondary);
  }
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>

<template>
  <div class="iframe-viewer" :class="{ 'mobile-mode': isMobile }">
    <!-- 移动端模式：带手机外壳 -->
    <template v-if="isMobile">
      <div class="phone-frame">
        <div class="phone-notch"></div>
        <iframe
          ref="iframeRef"
          :src="cacheBustedSrc"
          class="mobile-iframe"
          :style="{ width: `${resolvedViewport.width}px`, height: `${resolvedViewport.height + 100 - 88}px` }"
          frameborder="0"
          sandbox="allow-scripts allow-same-origin allow-popups allow-forms"
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
        :src="cacheBustedSrc"
        class="web-iframe"
        frameborder="0"
        sandbox="allow-scripts allow-same-origin allow-popups allow-forms"
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

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { cacheBustUrl } from '@/utils/menu'

/** 视口配置 */
interface Viewport {
  /** 宽度 */
  width: number
  /** 高度 */
  height: number
}

/** iframe 查看器组件属性 */
const props = withDefaults(defineProps<{
  /** iframe 页面地址 */
  src: string
  /** 内容类型（web / mobile / miniprogram） */
  type?: string
  /** 移动端视口配置 */
  viewport?: Viewport
}>(), {
  type: 'web',
  viewport: () => ({ width: 380, height: 812 }),
})

const emit = defineEmits<{
  /** iframe 加载完成 */
  (e: 'load'): void
  /** iframe 加载失败 */
  (e: 'error', msg: string): void
}>()

/** iframe DOM 引用 */
const iframeRef = ref<HTMLIFrameElement | null>(null)
/** 是否正在加载 */
const loading = ref(true)
/** 错误信息 */
const error = ref<string | null>(null)
/** 缓存破坏键 */
const cacheKey = ref(Date.now())

/** 解析后的视口配置（带默认值兜底） */
const resolvedViewport = computed(() => props.viewport ?? { width: 380, height: 812 })

/** 是否为移动端模式 */
const isMobile = computed(() => props.type === 'mobile' || props.type === 'miniprogram')

/** 给 URL 追加时间戳参数，避免浏览器缓存 */
const cacheBustedSrc = computed(() => cacheBustUrl(props.src, cacheKey.value))

/** iframe 加载完成回调 */
function onLoad(): void {
  loading.value = false
  error.value = null
  emit('load')
}

/** iframe 加载失败回调 */
function onError(): void {
  loading.value = false
  error.value = '无法加载页面'
  emit('error', error.value!)
}

/** 重新加载 iframe */
function reload(): void {
  if (iframeRef.value) {
    loading.value = true
    error.value = null
    // 更新时间戳强制重新加载
    cacheKey.value = Date.now()
  }
}

defineExpose({ reload })

// 监听 src 变化，重置加载状态
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
  background: var(--bg-primary);
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
  background: var(--bg-primary);
  opacity: 0.95;
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

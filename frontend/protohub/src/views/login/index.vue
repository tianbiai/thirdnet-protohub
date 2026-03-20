<template>
  <div class="login-page" ref="pageRef">
    <!-- 粒子画布 -->
    <canvas ref="particleCanvas" class="particle-canvas"></canvas>

    <!-- 背景装饰层 -->
    <div class="background-art">
      <!-- 渐变网格 -->
      <div class="gradient-mesh"></div>

      <!-- 光晕效果 -->
      <div class="aurora-container">
        <div class="aurora aurora-1"></div>
        <div class="aurora aurora-2"></div>
        <div class="aurora aurora-3"></div>
      </div>

      <!-- 网格覆盖 -->
      <div class="grid-overlay"></div>
    </div>

    <!-- 波浪动画 -->
    <div class="waves-container">
      <svg class="waves" xmlns="http://www.w3.org/2000/svg" viewBox="0 24 150 28" preserveAspectRatio="none">
        <defs>
          <linearGradient id="wave-gradient-1" x1="0%" y1="0%" x2="100%" y2="0%">
            <stop offset="0%" style="stop-color: var(--primary-color);stop-opacity:0.25" />
            <stop offset="50%" style="stop-color: var(--info-color);stop-opacity:0.15" />
            <stop offset="100%" style="stop-color: var(--primary-color);stop-opacity:0.25" />
          </linearGradient>
          <linearGradient id="wave-gradient-2" x1="0%" y1="0%" x2="100%" y2="0%">
            <stop offset="0%" style="stop-color: var(--info-color);stop-opacity:0.15" />
            <stop offset="50%" style="stop-color: var(--primary-color);stop-opacity:0.1" />
            <stop offset="100%" style="stop-color: var(--info-color);stop-opacity:0.15" />
          </linearGradient>
          <linearGradient id="wave-gradient-3" x1="0%" y1="0%" x2="100%" y2="0%">
            <stop offset="0%" style="stop-color: var(--primary-color);stop-opacity:0.08" />
            <stop offset="50%" style="stop-color: var(--primary-color);stop-opacity:0.04" />
            <stop offset="100%" style="stop-color: var(--primary-color);stop-opacity:0.08" />
          </linearGradient>
        </defs>
        <path class="wave wave-3" fill="url(#wave-gradient-3)" d="M-160 44c30 0 58-18 88-18s 58 18 88 18 58-18 88-18 58 18 88 18 v28h-352z"/>
        <path class="wave wave-2" fill="url(#wave-gradient-2)" d="M-160 44c30 0 58-18 88-18s 58 18 88 18 58-18 88-18 58 18 88 18 v28h-352z"/>
        <path class="wave wave-1" fill="url(#wave-gradient-1)" d="M-160 44c30 0 58-18 88-18s 58 18 88 18 58-18 88-18 58 18 88 18 v28h-352z"/>
      </svg>
    </div>

    <!-- 登录容器 -->
    <div class="login-container">
      <!-- 品牌区域 -->
      <div class="brand-section">
        <div class="brand-icon-wrapper">
          <div class="brand-icon">
            <img src="@/assets/logo.png" alt="ProtoHub Logo" class="logo-img" />
          </div>
          <div class="icon-glow"></div>
          <div class="icon-ring ring-1"></div>
          <div class="icon-ring ring-2"></div>
          <div class="icon-ring ring-3"></div>
        </div>
        <h1 class="brand-title">ProtoHub 原型视界</h1>
        <p class="brand-subtitle">原型与文档的聚合视界</p>

        <div class="feature-list">
          <div class="feature-item">
            <span class="feature-icon">🎯</span>
            <span class="feature-text">原型页面预览</span>
          </div>
          <div class="feature-item">
            <span class="feature-icon">📄</span>
            <span class="feature-text">文档聚合查看</span>
          </div>
        </div>
      </div>

      <!-- 登录卡片 -->
      <div class="login-card">
        <div class="card-header">
          <h2>欢迎回来</h2>
          <p>请登录您的账号继续</p>
        </div>

        <el-form
          ref="formRef"
          :model="loginForm"
          :rules="rules"
          class="login-form"
          @submit.prevent="handleLogin"
        >
          <el-form-item prop="username">
            <div class="input-group">
              <label class="input-label">用户名</label>
              <el-input
                v-model="loginForm.username"
                placeholder="请输入用户名"
                size="large"
                clearable
              >
                <template #prefix>
                  <el-icon><User /></el-icon>
                </template>
              </el-input>
            </div>
          </el-form-item>

          <el-form-item prop="password">
            <div class="input-group">
              <label class="input-label">密码</label>
              <el-input
                v-model="loginForm.password"
                type="password"
                placeholder="请输入密码"
                size="large"
                show-password
                @keyup.enter="handleLogin"
              >
                <template #prefix>
                  <el-icon><Lock /></el-icon>
                </template>
              </el-input>
            </div>
          </el-form-item>

          <el-form-item class="submit-item">
            <el-button
              type="primary"
              size="large"
              :loading="loading"
              class="login-btn"
              @click="handleLogin"
            >
              <span>{{ loading ? '登录中...' : '登 录' }}</span>
              <el-icon v-if="!loading" class="btn-arrow"><ArrowRight /></el-icon>
            </el-button>
          </el-form-item>
        </el-form>

      </div>
    </div>

    <!-- 底部版权 -->
    <div class="footer">
      <p>© 2024 ProtoHub · Powered by Vue 3</p>
    </div>

    <!-- 全屏加载动画 -->
    <transition name="loading-fade">
      <div v-if="showLoading" class="fullscreen-loading">
        <div class="loading-content">
          <div class="loading-logo">
            <img src="@/assets/logo.png" alt="ProtoHub Logo" class="loading-logo-img" />
            <div class="loading-glow"></div>
            <div class="loading-ring ring-1"></div>
            <div class="loading-ring ring-2"></div>
            <div class="loading-ring ring-3"></div>
          </div>
          <div class="loading-spinner">
            <div class="spinner-ring"></div>
            <div class="spinner-ring"></div>
            <div class="spinner-ring"></div>
          </div>
          <h2 class="loading-title">{{ loadingText }}</h2>
          <div class="loading-progress">
            <div class="progress-bar" :style="{ width: progressWidth }"></div>
          </div>
          <p class="loading-tip">{{ loadingTip }}</p>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { ElMessage } from 'element-plus'
import { User, Lock, ArrowRight } from '@element-plus/icons-vue'

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()

const formRef = ref(null)
const loading = ref(false)
const pageRef = ref(null)
const particleCanvas = ref(null)

// 全屏加载状态
const showLoading = ref(false)
const loadingText = ref('正在登录...')
const loadingTip = ref('正在验证您的身份')
const progressWidth = ref('0%')

// 加载提示文案列表
const loadingTips = [
  { text: '正在验证身份', tip: '检查账号信息...' },
  { text: '加载用户数据', tip: '获取权限配置...' },
  { text: '即将进入', tip: '准备就绪...' }
]

const loginForm = reactive({
  username: '',
  password: ''
})

const rules = {
  username: [
    { required: true, message: '请输入用户名', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' }
  ]
}

// 粒子系统
let animationId = null
let particles = []
let mouseX = 0
let mouseY = 0

class Particle {
  constructor(canvas) {
    this.canvas = canvas
    this.reset()
  }

  reset() {
    this.x = Math.random() * this.canvas.width
    this.y = Math.random() * this.canvas.height
    this.vx = (Math.random() - 0.5) * 0.5
    this.vy = (Math.random() - 0.5) * 0.5
    this.radius = Math.random() * 2 + 1
    this.opacity = Math.random() * 0.4 + 0.15
  }

  update() {
    // 鼠标交互
    const dx = mouseX - this.x
    const dy = mouseY - this.y
    const dist = Math.sqrt(dx * dx + dy * dy)

    if (dist < 150) {
      const force = (150 - dist) / 150
      this.vx += dx * force * 0.001
      this.vy += dy * force * 0.001
    }

    // 速度衰减
    this.vx *= 0.99
    this.vy *= 0.99

    this.x += this.vx
    this.y += this.vy

    // 边界检测
    if (this.x < 0 || this.x > this.canvas.width) this.vx *= -1
    if (this.y < 0 || this.y > this.canvas.height) this.vy *= -1

    // 保持在画布内
    this.x = Math.max(0, Math.min(this.canvas.width, this.x))
    this.y = Math.max(0, Math.min(this.canvas.height, this.y))
  }

  draw(ctx) {
    ctx.beginPath()
    ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2)
    // 使用 CSS 变量对应的颜色
    ctx.fillStyle = `rgba(59, 130, 246, ${this.opacity})`
    ctx.fill()
  }
}

function initParticles() {
  const canvas = particleCanvas.value
  if (!canvas) return

  const ctx = canvas.getContext('2d')

  // 设置画布尺寸
  const resizeCanvas = () => {
    canvas.width = window.innerWidth
    canvas.height = window.innerHeight
  }
  resizeCanvas()
  window.addEventListener('resize', resizeCanvas)

  // 创建粒子
  const particleCount = Math.min(60, Math.floor((canvas.width * canvas.height) / 20000))
  particles = []
  for (let i = 0; i < particleCount; i++) {
    particles.push(new Particle(canvas))
  }

  // 绘制连线
  function drawConnections() {
    for (let i = 0; i < particles.length; i++) {
      for (let j = i + 1; j < particles.length; j++) {
        const dx = particles[i].x - particles[j].x
        const dy = particles[i].y - particles[j].y
        const dist = Math.sqrt(dx * dx + dy * dy)

        if (dist < 120) {
          const opacity = (1 - dist / 120) * 0.12
          ctx.beginPath()
          ctx.moveTo(particles[i].x, particles[i].y)
          ctx.lineTo(particles[j].x, particles[j].y)
          ctx.strokeStyle = `rgba(59, 130, 246, ${opacity})`
          ctx.lineWidth = 0.5
          ctx.stroke()
        }
      }
    }
  }

  // 动画循环
  function animate() {
    ctx.clearRect(0, 0, canvas.width, canvas.height)

    // 更新和绘制粒子
    particles.forEach(particle => {
      particle.update()
      particle.draw(ctx)
    })

    // 绘制连线
    drawConnections()

    animationId = requestAnimationFrame(animate)
  }

  animate()

  // 鼠标移动事件
  const handleMouseMove = (e) => {
    mouseX = e.clientX
    mouseY = e.clientY
  }
  window.addEventListener('mousemove', handleMouseMove)

  // 返回清理函数
  return () => {
    window.removeEventListener('resize', resizeCanvas)
    window.removeEventListener('mousemove', handleMouseMove)
    if (animationId) {
      cancelAnimationFrame(animationId)
    }
  }
}

let cleanupParticles = null

onMounted(() => {
  cleanupParticles = initParticles()
})

onUnmounted(() => {
  if (cleanupParticles) {
    cleanupParticles()
  }
})

async function handleLogin() {
  try {
    await formRef.value.validate()
    loading.value = true

    await userStore.login(loginForm.username, loginForm.password)
    ElMessage.success('登录成功')

    // 显示全屏加载动画
    showLoading.value = true

    // 模拟加载进度（2-3秒）
    const totalDuration = 2500 // 2.5秒
    const stepDuration = totalDuration / loadingTips.length

    for (let i = 0; i < loadingTips.length; i++) {
      loadingText.value = loadingTips[i].text
      loadingTip.value = loadingTips[i].tip

      // 更新进度条
      const startProgress = (i / loadingTips.length) * 100
      const endProgress = ((i + 1) / loadingTips.length) * 100

      // 动画进度
      const startTime = Date.now()
      await new Promise(resolve => {
        const updateProgress = () => {
          const elapsed = Date.now() - startTime
          const progress = Math.min(elapsed / stepDuration, 1)
          progressWidth.value = `${startProgress + (endProgress - startProgress) * progress}%`

          if (progress < 1) {
            requestAnimationFrame(updateProgress)
          } else {
            resolve()
          }
        }
        requestAnimationFrame(updateProgress)
      })
    }

    // 确保进度条满
    progressWidth.value = '100%'

    // 短暂停留后跳转
    await new Promise(resolve => setTimeout(resolve, 300))

    const redirect = route.query.redirect || '/'
    router.push(redirect)
  } catch (error) {
    showLoading.value = false
    progressWidth.value = '0%'
    ElMessage.error(error.message || '登录失败')
  } finally {
    loading.value = false
  }
}
</script>

<style lang="scss" scoped>
.login-page {
  width: 100%;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
  background: var(--bg-secondary);
}

// 粒子画布
.particle-canvas {
  position: fixed;
  inset: 0;
  pointer-events: none;
  z-index: 1;
}

// 背景装饰
.background-art {
  position: fixed;
  inset: 0;
  pointer-events: none;
  overflow: hidden;
  z-index: 0;
}

// 渐变网格
.gradient-mesh {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse at 20% 20%, var(--primary-100) 0%, transparent 50%),
    radial-gradient(ellipse at 80% 80%, var(--info-light) 0%, transparent 50%),
    radial-gradient(ellipse at 50% 50%, var(--primary-50) 0%, transparent 70%);
  opacity: 0.6;
  animation: gradientShift 15s ease-in-out infinite;
}

@keyframes gradientShift {
  0%, 100% {
    transform: scale(1) rotate(0deg);
  }
  50% {
    transform: scale(1.1) rotate(3deg);
  }
}

// 极光效果
.aurora-container {
  position: absolute;
  inset: 0;
  filter: blur(60px);
}

.aurora {
  position: absolute;
  border-radius: 50%;
  animation: auroraFloat 20s ease-in-out infinite;

  &.aurora-1 {
    width: 500px;
    height: 500px;
    background: radial-gradient(circle, var(--primary-200) 0%, transparent 70%);
    top: -15%;
    left: -10%;
    animation-delay: 0s;
  }

  &.aurora-2 {
    width: 400px;
    height: 400px;
    background: radial-gradient(circle, var(--info-light) 0%, transparent 70%);
    top: 30%;
    right: -10%;
    animation-delay: -7s;
  }

  &.aurora-3 {
    width: 300px;
    height: 300px;
    background: radial-gradient(circle, var(--primary-100) 0%, transparent 70%);
    bottom: -5%;
    left: 30%;
    animation-delay: -14s;
  }
}

@keyframes auroraFloat {
  0%, 100% {
    transform: translate(0, 0) scale(1);
    opacity: 0.7;
  }
  25% {
    transform: translate(40px, -20px) scale(1.1);
    opacity: 0.9;
  }
  50% {
    transform: translate(-20px, 40px) scale(0.95);
    opacity: 0.6;
  }
  75% {
    transform: translate(-40px, -15px) scale(1.05);
    opacity: 0.8;
  }
}

// 网格覆盖
.grid-overlay {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(0, 0, 0, 0.02) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0, 0, 0, 0.02) 1px, transparent 1px);
  background-size: 60px 60px;
}

// 波浪容器
.waves-container {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 180px;
  z-index: 1;
  pointer-events: none;
}

.waves {
  width: 200%;
  height: 100%;
  animation: waveMove 25s linear infinite;
}

@keyframes waveMove {
  0% {
    transform: translateX(0);
  }
  100% {
    transform: translateX(-50%);
  }
}

.wave {
  animation: waveWave 8s ease-in-out infinite;

  &.wave-1 {
    animation-delay: 0s;
  }

  &.wave-2 {
    animation-delay: -2s;
  }

  &.wave-3 {
    animation-delay: -4s;
  }
}

@keyframes waveWave {
  0%, 100% {
    d: path("M-160 44c30 0 58-18 88-18s 58 18 88 18 58-18 88-18 58 18 88 18 v28h-352z");
  }
  50% {
    d: path("M-160 34c30 0 58-8 88-8s 58 8 88 8 58-8 88-8 58 8 88 8 v38h-352z");
  }
}

// 深色主题
[data-theme="dark"] {
  .gradient-mesh {
    opacity: 0.3;
  }

  .aurora {
    opacity: 0.5;

    &.aurora-1 {
      background: radial-gradient(circle, var(--primary-700) 0%, transparent 70%);
    }

    &.aurora-2 {
      background: radial-gradient(circle, var(--primary-600) 0%, transparent 70%);
    }

    &.aurora-3 {
      background: radial-gradient(circle, var(--primary-800) 0%, transparent 70%);
    }
  }

  .grid-overlay {
    background-image:
      linear-gradient(rgba(255, 255, 255, 0.02) 1px, transparent 1px),
      linear-gradient(90deg, rgba(255, 255, 255, 0.02) 1px, transparent 1px);
  }
}

// 登录容器
.login-container {
  position: relative;
  z-index: 10;
  display: flex;
  align-items: stretch;
  gap: 48px;
  animation: fadeInUp 0.6s var(--ease-out);
}

// 品牌区域
.brand-section {
  display: flex;
  flex-direction: column;
  justify-content: center;
  width: 320px;
  flex-shrink: 0;
}

// Logo包装器
.brand-icon-wrapper {
  position: relative;
  width: 64px;
  height: 64px;
  margin-bottom: 48px;
}

.brand-icon {
  position: absolute;
  top: 0;
  left: 0;
  width: 64px;
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--primary-light), transparent);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);
  animation: iconFloat 3s ease-in-out infinite;
  z-index: 2;

  .logo-img {
    width: 48px;
    height: 48px;
    object-fit: contain;
  }
}

// Logo光晕效果
.icon-glow {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 80px;
  height: 80px;
  background: var(--primary-glow);
  border-radius: 50%;
  filter: blur(20px);
  opacity: 0.4;
  animation: glow 2s ease-in-out infinite;
}

// Logo扩散环
.icon-ring {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  border: 1px solid var(--primary-color);
  border-radius: 50%;
  opacity: 0;

  &.ring-1 {
    width: 80px;
    height: 80px;
    animation: ringSpread 3s ease-out infinite;
  }

  &.ring-2 {
    width: 80px;
    height: 80px;
    animation: ringSpread 3s ease-out infinite 1s;
  }

  &.ring-3 {
    width: 80px;
    height: 80px;
    animation: ringSpread 3s ease-out infinite 2s;
  }
}

@keyframes glow {
  0%, 100% {
    opacity: 0.3;
    transform: translate(-50%, -50%) scale(1);
  }
  50% {
    opacity: 0.5;
    transform: translate(-50%, -50%) scale(1.1);
  }
}

@keyframes ringSpread {
  0% {
    width: 64px;
    height: 64px;
    opacity: 0.5;
  }
  100% {
    width: 140px;
    height: 140px;
    opacity: 0;
  }
}

@keyframes iconFloat {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-6px);
  }
}

.brand-title {
  font-size: var(--font-size-2xl);
  font-weight: var(--font-weight-bold);
  color: var(--text-primary);
  margin-bottom: 8px;
  letter-spacing: -0.02em;
  line-height: 1.3;
}

.brand-subtitle {
  font-size: var(--font-size-base);
  color: var(--text-secondary);
  margin-bottom: 32px;
}

.feature-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.feature-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  background: rgba(255, 255, 255, 0.6);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.5);
  border-radius: var(--radius-md);
  transition: var(--transition-fast);

  &:hover {
    background: rgba(255, 255, 255, 0.8);
    transform: translateX(4px);
  }

  .feature-icon {
    font-size: 18px;
  }

  .feature-text {
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: var(--text-primary);
  }
}

[data-theme="dark"] {
  .feature-item {
    background: rgba(28, 28, 30, 0.6);
    border-color: rgba(255, 255, 255, 0.1);

    &:hover {
      background: rgba(44, 44, 46, 0.8);
    }
  }
}

// 登录卡片
.login-card {
  width: 380px;
  flex-shrink: 0;
  background: var(--glass-bg);
  backdrop-filter: blur(var(--glass-blur));
  border: 1px solid var(--glass-border);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-xl);
  padding: 40px;
}

.card-header {
  text-align: center;
  margin-bottom: 32px;

  h2 {
    font-size: var(--font-size-xl);
    font-weight: var(--font-weight-semibold);
    color: var(--text-primary);
    margin-bottom: 8px;
  }

  p {
    font-size: var(--font-size-sm);
    color: var(--text-secondary);
  }
}

// 表单样式
.login-form {
  .input-group {
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 12px;
  }

  .input-label {
    flex-shrink: 0;
    width: 56px;
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: var(--text-secondary);
  }

  // 确保 el-input 宽度一致
  .el-input {
    flex: 1;
    min-width: 0;
  }

  :deep(.el-input__wrapper) {
    background: var(--bg-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    box-shadow: none;
    transition: var(--transition-fast);

    &:hover {
      border-color: var(--primary-color);
    }

    &.is-focus {
      border-color: var(--primary-color);
      box-shadow: 0 0 0 3px var(--primary-glow);
    }
  }

  :deep(.el-input__inner) {
    font-size: var(--font-size-base);
    color: var(--text-primary);

    &::placeholder {
      color: var(--text-tertiary);
    }
  }

  :deep(.el-input__prefix) {
    color: var(--text-tertiary);
  }

  .el-form-item {
    margin-bottom: 24px;
  }

  .submit-item {
    margin-bottom: 0;
    margin-top: 32px;
  }
}

// 登录按钮
.login-btn {
  width: 100%;
  height: 48px;
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-semibold);
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: var(--transition-spring);

  .btn-arrow {
    font-size: 16px;
    opacity: 0;
    transform: translateX(-8px);
    transition: var(--transition-fast);
  }

  &:hover {
    transform: translateY(-2px);
    box-shadow: var(--shadow-primary);

    .btn-arrow {
      opacity: 1;
      transform: translateX(0);
    }
  }

  &:active {
    transform: translateY(0);
  }
}

// 底部版权
.footer {
  position: absolute;
  bottom: 24px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 10;

  p {
    font-size: var(--font-size-xs);
    color: var(--text-tertiary);
    text-align: center;
  }
}

// 动画
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(24px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

// 全屏加载动画
.fullscreen-loading {
  position: fixed;
  inset: 0;
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-primary) 100%);
}

.loading-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.loading-logo {
  position: relative;
  width: 80px;
  height: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--primary-light), transparent);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-lg);
  margin-bottom: 32px;
  animation: logoPulse 2s ease-in-out infinite;

  .loading-logo-img {
    width: 60px;
    height: 60px;
    object-fit: contain;
    position: relative;
    z-index: 1;
  }

  .loading-glow {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: 100px;
    height: 100px;
    background: var(--primary-glow);
    border-radius: 50%;
    filter: blur(25px);
    opacity: 0.4;
    animation: loadingGlow 2s ease-in-out infinite;
  }

  .loading-ring {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    border: 1px solid var(--primary-color);
    border-radius: 50%;
    opacity: 0;

    &.ring-1 {
      width: 80px;
      height: 80px;
      animation: loadingRingSpread 3s ease-out infinite;
    }

    &.ring-2 {
      width: 80px;
      height: 80px;
      animation: loadingRingSpread 3s ease-out infinite 1s;
    }

    &.ring-3 {
      width: 80px;
      height: 80px;
      animation: loadingRingSpread 3s ease-out infinite 2s;
    }
  }
}

@keyframes loadingGlow {
  0%, 100% {
    opacity: 0.3;
    transform: translate(-50%, -50%) scale(1);
  }
  50% {
    opacity: 0.5;
    transform: translate(-50%, -50%) scale(1.15);
  }
}

@keyframes loadingRingSpread {
  0% {
    width: 80px;
    height: 80px;
    opacity: 0.5;
  }
  100% {
    width: 160px;
    height: 160px;
    opacity: 0;
  }
}

@keyframes logoPulse {
  0%, 100% {
    transform: scale(1);
    box-shadow: var(--shadow-lg);
  }
  50% {
    transform: scale(1.05);
    box-shadow: var(--shadow-xl), 0 0 30px var(--primary-glow);
  }
}

.loading-spinner {
  position: relative;
  width: 60px;
  height: 60px;
  margin-bottom: 24px;
}

.spinner-ring {
  position: absolute;
  inset: 0;
  border: 2px solid transparent;
  border-radius: 50%;

  &:nth-child(1) {
    border-top-color: var(--primary-color);
    animation: spin 1s linear infinite;
  }

  &:nth-child(2) {
    inset: 6px;
    border-right-color: var(--info-color);
    animation: spin 1.5s linear infinite reverse;
  }

  &:nth-child(3) {
    inset: 12px;
    border-bottom-color: var(--primary-color);
    animation: spin 2s linear infinite;
  }
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.loading-title {
  font-size: var(--font-size-lg);
  font-weight: var(--font-weight-semibold);
  color: var(--text-primary);
  margin-bottom: 16px;
  animation: textFade 0.5s ease-out;
}

@keyframes textFade {
  from {
    opacity: 0;
    transform: translateY(8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.loading-progress {
  width: 240px;
  height: 4px;
  background: var(--bg-tertiary);
  border-radius: var(--radius-full);
  overflow: hidden;
  margin-bottom: 16px;
}

.progress-bar {
  height: 100%;
  background: linear-gradient(90deg, var(--primary-color), var(--info-color));
  border-radius: var(--radius-full);
  transition: width 0.1s ease-out;
  position: relative;

  &::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(
      90deg,
      transparent,
      rgba(255, 255, 255, 0.4),
      transparent
    );
    animation: shimmer 1.5s infinite;
  }
}

@keyframes shimmer {
  from {
    transform: translateX(-100%);
  }
  to {
    transform: translateX(100%);
  }
}

.loading-tip {
  font-size: var(--font-size-sm);
  color: var(--text-tertiary);
  animation: textFade 0.5s ease-out;
}

// 加载动画过渡
.loading-fade-enter-active {
  animation: loadingFadeIn 0.3s ease-out;
}

.loading-fade-leave-active {
  animation: loadingFadeOut 0.3s ease-in;
}

@keyframes loadingFadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@keyframes loadingFadeOut {
  from {
    opacity: 1;
  }
  to {
    opacity: 0;
  }
}

// 深色主题加载动画
[data-theme="dark"] {
  .fullscreen-loading {
    background: linear-gradient(135deg, var(--bg-primary) 0%, var(--bg-secondary) 100%);
  }

  .loading-logo {
    background: var(--bg-secondary);
  }

  .loading-progress {
    background: var(--bg-tertiary);
  }
}

// 响应式
@media (max-width: 900px) {
  .login-container {
    flex-direction: column;
    align-items: center;
    gap: 32px;
    padding: 24px;
  }

  .brand-section {
    width: 100%;
    max-width: 380px;
    align-items: center;
    text-align: center;
  }

  .brand-icon {
    margin: 0 auto 20px;
  }

  .feature-list {
    flex-direction: row;
    flex-wrap: wrap;
    justify-content: center;
    gap: 8px;
  }

  .feature-item {
    padding: 8px 12px;

    &:hover {
      transform: translateY(-2px);
    }
  }
}

@media (max-width: 480px) {
  .login-card {
    width: 100%;
    max-width: 360px;
    padding: 32px 24px;
  }

  .feature-list {
    display: none;
  }

  .brand-section {
    width: 100%;
  }

  .waves-container {
    height: 100px;
  }
}
</style>

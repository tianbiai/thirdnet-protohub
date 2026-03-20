<template>
  <div class="home-page">
    <!-- 欢迎区域 -->
    <div class="welcome-section">
      <!-- 动态粒子背景 -->
      <div class="particles-bg">
        <div
          v-for="i in 30"
          :key="i"
          class="particle"
          :style="{
            '--delay': `${Math.random() * 5}s`,
            '--duration': `${5 + Math.random() * 5}s`,
            '--size': `${2 + Math.random() * 4}px`,
            '--x': `${Math.random() * 100}%`,
            '--y': `${Math.random() * 100}%`
          }"
        ></div>
      </div>

      <!-- 流动光线 -->
      <div class="flowing-lines">
        <div class="line line-1"></div>
        <div class="line line-2"></div>
        <div class="line line-3"></div>
      </div>

      <div class="welcome-content">
        <div class="welcome-icon-wrapper">
          <div class="welcome-icon">
            <img src="@/assets/logo.png" alt="ProtoHub Logo" class="logo-img" />
          </div>
          <div class="icon-glow"></div>
          <div class="icon-ring ring-1"></div>
          <div class="icon-ring ring-2"></div>
          <div class="icon-ring ring-3"></div>
        </div>

        <h1 class="welcome-title">
          <span class="title-line">欢迎使用</span>
          <span class="title-highlight">
            <span v-for="(char, index) in 'ProtoHub 原型视界'" :key="index" :style="{ '--delay': `${index * 0.05}s` }">{{ char }}</span>
          </span>
        </h1>

        <p class="welcome-text">
          你好，<span class="user-name">{{ userStore.nickname }}</span> 👋
        </p>

        <div v-if="userStore.isAdmin" class="admin-actions">
          <el-button type="primary" size="large" class="action-btn breathe-btn" @click="$router.push('/menu-editor')">
            <el-icon class="btn-icon"><Menu /></el-icon>
            <span class="btn-text">菜单管理</span>
            <el-icon class="btn-arrow"><ArrowRight /></el-icon>
            <div class="btn-shine"></div>
          </el-button>
          <el-button size="large" class="action-btn secondary breathe-btn" @click="$router.push('/settings')">
            <el-icon class="btn-icon"><Setting /></el-icon>
            <span class="btn-text">系统设置</span>
            <el-icon class="btn-arrow"><ArrowRight /></el-icon>
          </el-button>
        </div>
      </div>

      <!-- 装饰元素 -->
      <div class="decoration-shapes">
        <div class="shape shape-1"></div>
        <div class="shape shape-2"></div>
        <div class="shape shape-3"></div>
        <div class="shape shape-4"></div>
        <div class="shape shape-5"></div>
      </div>

      <!-- 底部波浪 -->
      <div class="wave-container">
        <svg class="wave" viewBox="0 0 1440 120" preserveAspectRatio="none">
          <path d="M0,60 C240,120 480,0 720,60 C960,120 1200,0 1440,60 L1440,120 L0,120 Z" fill="url(#waveGradient)"/>
          <defs>
            <linearGradient id="waveGradient" x1="0%" y1="0%" x2="100%" y2="0%">
              <stop offset="0%" style="stop-color: var(--primary-color); stop-opacity: 0.05" />
              <stop offset="50%" style="stop-color: var(--info-color); stop-opacity: 0.08" />
              <stop offset="100%" style="stop-color: var(--primary-color); stop-opacity: 0.05" />
            </linearGradient>
          </defs>
        </svg>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useUserStore } from '@/stores/user'

const userStore = useUserStore()
</script>

<style lang="scss" scoped>
.home-page {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}

// 欢迎区域
.welcome-section {
  position: relative;
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg-primary);
  border-radius: var(--radius-xl);
  overflow: hidden;
  min-height: 100%;
}

// 粒子背景
.particles-bg {
  position: absolute;
  inset: 0;
  pointer-events: none;
  overflow: hidden;
}

.particle {
  position: absolute;
  width: var(--size);
  height: var(--size);
  left: var(--x);
  top: var(--y);
  background: var(--primary-color);
  border-radius: 50%;
  opacity: 0;
  animation: particleFloat var(--duration) ease-in-out var(--delay) infinite;
}

@keyframes particleFloat {
  0%, 100% {
    opacity: 0;
    transform: translateY(0) scale(0.5);
  }
  25% {
    opacity: 0.6;
  }
  50% {
    opacity: 0.8;
    transform: translateY(-100px) scale(1);
  }
  75% {
    opacity: 0.4;
  }
}

// 流动光线
.flowing-lines {
  position: absolute;
  inset: 0;
  pointer-events: none;
  overflow: hidden;
}

.line {
  position: absolute;
  height: 2px;
  background: linear-gradient(90deg, transparent, var(--primary-color), transparent);
  opacity: 0.15;
  animation: lineFlow 8s linear infinite;

  &.line-1 {
    width: 200px;
    top: 20%;
    left: -200px;
    animation-delay: 0s;
  }

  &.line-2 {
    width: 150px;
    top: 50%;
    left: -150px;
    animation-delay: 2s;
  }

  &.line-3 {
    width: 180px;
    top: 75%;
    left: -180px;
    animation-delay: 4s;
  }
}

@keyframes lineFlow {
  from {
    transform: translateX(0);
  }
  to {
    transform: translateX(calc(100vw + 200px));
  }
}

.welcome-content {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  animation: contentFadeIn 0.8s ease-out;
}

@keyframes contentFadeIn {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.welcome-icon-wrapper {
  position: relative;
  margin-bottom: 28px;
}

.welcome-icon {
  position: relative;
  width: 96px;
  height: 96px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--primary-light), transparent);
  border-radius: var(--radius-2xl);
  box-shadow: var(--shadow-md);
  animation: iconFloat 2.5s ease-in-out infinite;
  transition: transform var(--transition-spring);

  &:hover {
    transform: scale(1.08);
  }

  .logo-img {
    width: 72px;
    height: 72px;
    object-fit: contain;
  }
}

.icon-glow {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 120px;
  height: 120px;
  background: var(--primary-glow);
  border-radius: 50%;
  filter: blur(30px);
  opacity: 0.5;
  animation: glow 2s ease-in-out infinite;
}

// 图标扩散环
.icon-ring {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  border: 1px solid var(--primary-color);
  border-radius: 50%;
  opacity: 0;

  &.ring-1 {
    width: 120px;
    height: 120px;
    animation: ringSpread 3s ease-out infinite;
  }

  &.ring-2 {
    width: 120px;
    height: 120px;
    animation: ringSpread 3s ease-out infinite 1s;
  }

  &.ring-3 {
    width: 120px;
    height: 120px;
    animation: ringSpread 3s ease-out infinite 2s;
  }
}

@keyframes ringSpread {
  0% {
    width: 96px;
    height: 96px;
    opacity: 0.6;
  }
  100% {
    width: 200px;
    height: 200px;
    opacity: 0;
  }
}

.welcome-title {
  display: flex;
  flex-direction: column;
  gap: 4px;
  margin-bottom: 12px;

  .title-line {
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-regular);
    color: var(--text-secondary);
    letter-spacing: 0.02em;
    animation: titleSlide 0.6s ease-out;
  }

  .title-highlight {
    font-size: var(--font-size-3xl);
    font-weight: var(--font-weight-bold);
    background: linear-gradient(135deg, var(--text-primary), var(--primary-color));
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    letter-spacing: -0.02em;

    span {
      display: inline-block;
      animation: charReveal 0.5s ease-out forwards;
      opacity: 0;
      transform: translateY(10px);
    }
  }
}

@keyframes titleSlide {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes charReveal {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.welcome-text {
  font-size: var(--font-size-lg);
  color: var(--text-secondary);
  margin-bottom: 32px;
  animation: textFade 0.8s ease-out 0.4s backwards;

  .user-name {
    font-weight: var(--font-weight-semibold);
    color: var(--primary-color);
  }
}

@keyframes textFade {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

// 操作按钮
.admin-actions {
  display: flex;
  justify-content: center;
  gap: 16px;
  animation: actionsSlide 0.6s ease-out 0.6s backwards;
}

@keyframes actionsSlide {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  border-radius: var(--radius-lg);
  font-weight: var(--font-weight-medium);
  transition: var(--transition-spring);
  position: relative;
  overflow: hidden;

  .btn-icon {
    font-size: 18px;
    transition: transform var(--transition-fast);
  }

  .btn-arrow {
    font-size: 14px;
    opacity: 0;
    transform: translateX(-4px);
    transition: var(--transition-fast);
  }

  &:hover {
    .btn-icon {
      transform: scale(1.1);
    }

    .btn-arrow {
      opacity: 1;
      transform: translateX(0);
    }
  }

  // 按钮呼吸效果
  &.breathe-btn {
    animation: btnBreathe 2.5s ease-in-out infinite;
  }

  // 按钮闪光效果
  .btn-shine {
    position: absolute;
    top: 0;
    left: -100%;
    width: 50%;
    height: 100%;
    background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
    animation: btnShine 3s ease-in-out infinite;
  }

  &.secondary {
    background: var(--bg-secondary);
    border-color: var(--border-color);
    color: var(--text-primary);

    &:hover {
      background: var(--bg-hover);
      border-color: var(--primary-color);
      color: var(--primary-color);
    }
  }
}

@keyframes btnBreathe {
  0%, 100% {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }
  50% {
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.15);
  }
}

@keyframes btnShine {
  0%, 100% {
    left: -100%;
  }
  50% {
    left: 150%;
  }
}

// 装饰形状
.decoration-shapes {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
  overflow: hidden;

  .shape {
    position: absolute;
    border-radius: 50%;
    opacity: 0.5;

    &.shape-1 {
      width: 200px;
      height: 200px;
      background: linear-gradient(135deg, var(--primary-100), transparent);
      top: -50px;
      right: 10%;
      animation: float 5s ease-in-out infinite, breathe 3s ease-in-out infinite;
    }

    &.shape-2 {
      width: 150px;
      height: 150px;
      background: linear-gradient(135deg, var(--info-light), transparent);
      bottom: 10%;
      left: 5%;
      animation: float 6s ease-in-out infinite reverse, breathe 3.5s ease-in-out infinite 0.5s;
    }

    &.shape-3 {
      width: 100px;
      height: 100px;
      background: linear-gradient(135deg, var(--primary-200), transparent);
      top: 40%;
      right: 5%;
      animation: float 7s ease-in-out infinite, breathe 4s ease-in-out infinite 1s;
    }

    &.shape-4 {
      width: 80px;
      height: 80px;
      background: linear-gradient(135deg, var(--success-light), transparent);
      top: 15%;
      left: 10%;
      animation: float 8s ease-in-out infinite, breathe 3s ease-in-out infinite 1.5s;
    }

    &.shape-5 {
      width: 60px;
      height: 60px;
      background: linear-gradient(135deg, var(--warning-light), transparent);
      bottom: 20%;
      right: 15%;
      animation: float 6s ease-in-out infinite reverse, breathe 3.5s ease-in-out infinite 2s;
    }
  }
}

// 底部波浪
.wave-container {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 120px;
  pointer-events: none;
  overflow: hidden;

  .wave {
    width: 200%;
    height: 100%;
    animation: waveMove 15s linear infinite;
  }
}

@keyframes waveMove {
  from {
    transform: translateX(0);
  }
  to {
    transform: translateX(-50%);
  }
}

[data-theme="dark"] {
  .decoration-shapes .shape {
    opacity: 0.2;
  }

  .particle {
    background: var(--primary-400);
  }

  .line {
    background: linear-gradient(90deg, transparent, var(--primary-400), transparent);
  }
}

// 动画
@keyframes iconFloat {
  0%, 100% {
    transform: translateY(0) scale(1);
  }
  50% {
    transform: translateY(-6px) scale(1.02);
  }
}

@keyframes glow {
  0%, 100% {
    opacity: 0.3;
    transform: translate(-50%, -50%) scale(1);
  }
  50% {
    opacity: 0.6;
    transform: translate(-50%, -50%) scale(1.15);
  }
}

@keyframes float {
  0%, 100% {
    transform: translate(0, 0);
  }
  33% {
    transform: translate(15px, -10px);
  }
  66% {
    transform: translate(-10px, 15px);
  }
}

@keyframes breathe {
  0%, 100% {
    opacity: 0.3;
    transform: scale(1);
  }
  50% {
    opacity: 0.6;
    transform: scale(1.1);
  }
}

// 响应式
@media (max-width: 768px) {
  .welcome-section {
    padding: 32px 20px;
    min-height: 280px;
  }

  .welcome-icon {
    width: 72px;
    height: 72px;
  }

  .welcome-title .title-highlight {
    font-size: var(--font-size-2xl);
  }

  .admin-actions {
    flex-direction: column;
    width: 100%;

    .action-btn {
      width: 100%;
      justify-content: center;
    }
  }

  .particles-bg {
    display: none;
  }
}
</style>

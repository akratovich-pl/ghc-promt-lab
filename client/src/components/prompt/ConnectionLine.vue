<template>
  <svg
    :width="width"
    :height="height"
    class="absolute pointer-events-none"
    :style="{ left: `${x}px`, top: `${y}px` }"
  >
    <defs>
      <linearGradient :id="`gradient-${id}`" x1="0%" y1="0%" x2="100%" y2="0%">
        <stop offset="0%" :style="{ stopColor: startColor, stopOpacity: 1 }" />
        <stop offset="100%" :style="{ stopColor: endColor, stopOpacity: 1 }" />
      </linearGradient>
      
      <!-- Animated gradient for flow effect -->
      <linearGradient :id="`flow-gradient-${id}`" x1="0%" y1="0%" x2="100%" y2="0%">
        <stop offset="0%" :style="{ stopColor: startColor, stopOpacity: 0 }">
          <animate
            v-if="animated"
            attributeName="offset"
            values="0;1"
            :dur="`${animationDuration}s`"
            repeatCount="indefinite"
          />
        </stop>
        <stop offset="50%" :style="{ stopColor: flowColor, stopOpacity: 1 }">
          <animate
            v-if="animated"
            attributeName="offset"
            values="0.5;1.5"
            :dur="`${animationDuration}s`"
            repeatCount="indefinite"
          />
        </stop>
        <stop offset="100%" :style="{ stopColor: endColor, stopOpacity: 0 }">
          <animate
            v-if="animated"
            attributeName="offset"
            values="1;2"
            :dur="`${animationDuration}s`"
            repeatCount="indefinite"
          />
        </stop>
      </linearGradient>
    </defs>

    <!-- Base line -->
    <path
      ref="pathRef"
      :d="pathData"
      :stroke="`url(#gradient-${id})`"
      :stroke-width="strokeWidth"
      fill="none"
      stroke-linecap="round"
      :opacity="baseOpacity"
    />

    <!-- Animated flow line -->
    <path
      v-if="animated"
      :d="pathData"
      :stroke="`url(#flow-gradient-${id})`"
      :stroke-width="strokeWidth + 2"
      fill="none"
      stroke-linecap="round"
    />

    <!-- Arrow head -->
    <polygon
      v-if="showArrow"
      :points="arrowPoints"
      :fill="endColor"
      :opacity="baseOpacity"
    />
  </svg>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { gsap } from 'gsap'

export interface ConnectionLineProps {
  x?: number
  y?: number
  width?: number
  height?: number
  startX?: number
  startY?: number
  endX?: number
  endY?: number
  animated?: boolean
  startColor?: string
  endColor?: string
  flowColor?: string
  strokeWidth?: number
  showArrow?: boolean
  curve?: boolean
  animationDuration?: number
  baseOpacity?: number
}

const props = withDefaults(defineProps<ConnectionLineProps>(), {
  x: 0,
  y: 0,
  width: 200,
  height: 100,
  startX: 0,
  startY: 50,
  endX: 200,
  endY: 50,
  animated: true,
  startColor: '#0ea5e9',
  endColor: '#06b6d4',
  flowColor: '#38bdf8',
  strokeWidth: 3,
  showArrow: true,
  curve: true,
  animationDuration: 2,
  baseOpacity: 0.6
})

const pathRef = ref<SVGPathElement>()
const id = ref(`line-${Math.random().toString(36).substr(2, 9)}`)

const pathData = computed(() => {
  const { startX, startY, endX, endY, curve } = props
  
  if (curve) {
    // Create a smooth bezier curve
    const controlX1 = startX + (endX - startX) / 3
    const controlY1 = startY
    const controlX2 = startX + (2 * (endX - startX)) / 3
    const controlY2 = endY
    
    return `M ${startX} ${startY} C ${controlX1} ${controlY1}, ${controlX2} ${controlY2}, ${endX} ${endY}`
  }
  
  return `M ${startX} ${startY} L ${endX} ${endY}`
})

const arrowPoints = computed(() => {
  const { endX, endY, strokeWidth } = props
  const arrowSize = strokeWidth * 2.5
  
  // Calculate arrow direction
  const dx = props.endX - props.startX
  const angle = Math.atan2(props.endY - props.startY, dx)
  
  const x1 = endX
  const y1 = endY
  const x2 = endX - arrowSize * Math.cos(angle - Math.PI / 6)
  const y2 = endY - arrowSize * Math.sin(angle - Math.PI / 6)
  const x3 = endX - arrowSize * Math.cos(angle + Math.PI / 6)
  const y3 = endY - arrowSize * Math.sin(angle + Math.PI / 6)
  
  return `${x1},${y1} ${x2},${y2} ${x3},${y3}`
})

watch(() => props.animated, (isAnimated) => {
  if (isAnimated && pathRef.value) {
    animatePulse()
  }
})

function animatePulse() {
  if (pathRef.value) {
    gsap.to(pathRef.value, {
      opacity: props.baseOpacity + 0.3,
      duration: props.animationDuration / 2,
      yoyo: true,
      repeat: -1,
      ease: 'sine.inOut'
    })
  }
}

onMounted(() => {
  if (props.animated && pathRef.value) {
    // Initial fade in
    gsap.from(pathRef.value, {
      opacity: 0,
      duration: 0.8,
      ease: 'power2.out',
      onComplete: () => {
        animatePulse()
      }
    })
  }
})
</script>

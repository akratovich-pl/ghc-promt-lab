import type { TooltipRegistry } from './types'
import { parameterTooltips } from './parameters'
import { modelTooltips } from './models'
import { conceptTooltips } from './concepts'

/**
 * Central registry of all tooltip content
 * Organized by category for easy access
 */
export const tooltips: TooltipRegistry = {
  parameters: parameterTooltips,
  models: modelTooltips,
  concepts: conceptTooltips
}

/**
 * Get tooltip content by category and key
 */
export function getTooltip(category: keyof TooltipRegistry, key: string) {
  return tooltips[category][key]
}

/**
 * Check if tooltip exists
 */
export function hasTooltip(category: keyof TooltipRegistry, key: string): boolean {
  return !!tooltips[category]?.[key]
}

// Export types for use in components
export type { TooltipContent, TooltipCategory, TooltipRegistry } from './types'

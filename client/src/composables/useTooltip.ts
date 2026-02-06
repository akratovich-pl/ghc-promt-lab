import { getTooltip, hasTooltip } from '@/config/tooltips'
import type { TooltipContent, TooltipRegistry } from '@/config/tooltips'

/**
 * Composable for accessing tooltip content
 * Provides type-safe access to tooltip information
 */
export function useTooltip() {
  /**
   * Get tooltip content for a specific item
   * @param category - The tooltip category (parameters, models, concepts)
   * @param key - The specific tooltip key
   * @returns TooltipContent or undefined if not found
   */
  const getTooltipContent = (
    category: keyof TooltipRegistry,
    key: string
  ): TooltipContent | undefined => {
    return getTooltip(category, key)
  }

  /**
   * Check if tooltip exists for a given item
   * @param category - The tooltip category
   * @param key - The specific tooltip key
   * @returns boolean indicating if tooltip exists
   */
  const tooltipExists = (category: keyof TooltipRegistry, key: string): boolean => {
    return hasTooltip(category, key)
  }

  /**
   * Get tooltip text (title + description) for display
   * @param category - The tooltip category
   * @param key - The specific tooltip key
   * @returns Formatted tooltip text or empty string
   */
  const getTooltipText = (category: keyof TooltipRegistry, key: string): string => {
    const tooltip = getTooltip(category, key)
    if (!tooltip) return ''

    let text = `<strong>${tooltip.title}</strong><br/>${tooltip.description}`

    if (tooltip.example) {
      text += `<br/><br/><em>Example:</em> ${tooltip.example}`
    }

    if (tooltip.bestPractice) {
      text += `<br/><br/><em>Best Practice:</em> ${tooltip.bestPractice}`
    }

    return text
  }

  return {
    getTooltipContent,
    tooltipExists,
    getTooltipText
  }
}

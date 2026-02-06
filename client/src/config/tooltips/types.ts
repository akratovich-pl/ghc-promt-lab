/**
 * Tooltip content structure for educational help system
 */
export interface TooltipContent {
  /** Display title for the tooltip */
  title: string

  /** Main description text (beginner-friendly) */
  description: string

  /** Optional usage example */
  example?: string

  /** Optional link to official documentation */
  learnMoreUrl?: string

  /** Optional best practice recommendation */
  bestPractice?: string
}

/**
 * Category of related tooltips
 */
export interface TooltipCategory {
  [key: string]: TooltipContent
}

/**
 * All tooltip content organized by category
 */
export interface TooltipRegistry {
  parameters: TooltipCategory
  models: TooltipCategory
  concepts: TooltipCategory
}

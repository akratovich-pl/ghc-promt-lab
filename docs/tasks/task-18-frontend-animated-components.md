# Task 18: Animated Block Components and Visual Interface

**Status**: ✅ Done  
**Priority**: High  
**Estimated Effort**: 4-5 hours  
**GitHub Issue**: [#23](https://github.com/akratovich-pl/ghc-promt-lab/issues/23)

## Description
Build the core animated block components (Input, Processor, Output) with visual connections, implement animations using GSAP, and create the main visual layout with Tailwind CSS.

## Requirements

### 1. Create Components
Create components in `components/prompt/`:
- `InputBlock.vue` - Prompt input with character/token counter
- `ProcessorBlock.vue` - LLM name, token calculation, processing state
- `OutputBlock.vue` - Response display with metrics panel
- `ConnectionLine.vue` - Animated SVG lines connecting blocks
- `MetricsDisplay.vue` - Real-time metrics (tokens, time, cost)

### 2. Implement GSAP Animations
- Pulsing/flowing effect on connection lines
- Block entrance animations
- Processing state transitions

### 3. Style with Tailwind CSS
- Responsive block layout
- Loading states and transitions
- Consistent design system

## Acceptance Criteria
- [x] All block components render correctly in PromptLabView
- [x] Connection lines animate between blocks
- [x] Input block accepts text and shows character count
- [x] Processor block displays LLM name and processing state
- [x] Output block displays response and metrics
- [x] Animations are smooth (60fps)
- [x] Layout is responsive (desktop and tablet)
- [x] Components use TypeScript with proper prop types

## Technical Details
- GSAP for animations
- Tailwind CSS for styling
- TypeScript for type safety
- Responsive design principles

## Dependencies
- Depends on: Task 17 (Page Structure and Stores)

## Related Tasks
- Enables: Task 19 (Backend Integration)
- Enhanced by: Task 20 (Markdown Rendering)
- Enhanced by: Task 21 (Interactive Tooltips)

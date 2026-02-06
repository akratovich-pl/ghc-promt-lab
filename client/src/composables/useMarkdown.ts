import { marked } from 'marked'
// @ts-ignore - marked-highlight doesn't have types
import { markedHighlight } from 'marked-highlight'
import hljs from 'highlight.js'
import DOMPurify from 'dompurify'

/**
 * Configure marked with syntax highlighting and custom options
 */
function configureMarked() {
  marked.use(
    markedHighlight({
      langPrefix: 'hljs language-',
      highlight(code: string, lang: string) {
        if (lang && hljs.getLanguage(lang)) {
          try {
            return hljs.highlight(code, { language: lang }).value
          } catch (err) {
            console.error('Highlight.js error:', err)
          }
        }
        // Auto-detect language if not specified
        try {
          return hljs.highlightAuto(code).value
        } catch (err) {
          console.error('Highlight.js auto-detect error:', err)
          return code
        }
      }
    })
  )
  
  marked.setOptions({
    breaks: true, // Convert line breaks to <br>
    gfm: true, // GitHub Flavored Markdown
  })
}

// Initialize marked configuration
configureMarked()

/**
 * Renders markdown text to sanitized HTML
 * @param markdownText - The markdown text to render
 * @returns Sanitized HTML string
 */
export function renderMarkdown(markdownText: string): string {
  if (!markdownText) {
    return ''
  }

  try {
    // Parse markdown to HTML
    const rawHtml = marked.parse(markdownText) as string

    // Sanitize HTML to prevent XSS attacks
    const sanitizedHtml = DOMPurify.sanitize(rawHtml, {
      ALLOWED_TAGS: [
        'p', 'br', 'strong', 'em', 'u', 's', 'code', 'pre',
        'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
        'ul', 'ol', 'li',
        'blockquote',
        'a', 'img',
        'table', 'thead', 'tbody', 'tr', 'th', 'td',
        'hr',
        'div', 'span'
      ],
      ALLOWED_ATTR: [
        'href', 'target', 'rel', 'class',
        'src', 'alt', 'title',
        'align', 'colspan', 'rowspan'
      ],
      ALLOW_DATA_ATTR: false,
      // Ensure external links are safe
      ALLOWED_URI_REGEXP: /^(?:(?:(?:f|ht)tps?|mailto|tel|callto|sms|cid|xmpp):|[^a-z]|[a-z+.\-]+(?:[^a-z+.\-:]|$))/i,
    })

    // Add security attributes to links
    return sanitizedHtml.replace(
      /<a /g,
      '<a target="_blank" rel="noopener noreferrer" '
    )
  } catch (error) {
    console.error('Markdown rendering error:', error)
    return markdownText // Fallback to plain text
  }
}

/**
 * Composable for using markdown rendering in Vue components
 */
export function useMarkdown() {
  return {
    renderMarkdown
  }
}

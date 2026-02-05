import { AiProvider } from '@/types/api'

/**
 * Maps provider name (string) to AiProvider enum value
 * @param providerName - Provider name like "Google", "Groq"
 * @returns AiProvider enum value
 */
export function mapProviderNameToEnum(providerName: string): AiProvider {
  const normalized = providerName.toLowerCase()
  switch (normalized) {
    case 'google':
      return AiProvider.Google
    case 'groq':
      return AiProvider.Groq
    default:
      throw new Error(`Unknown provider: ${providerName}`)
  }
}

/**
 * Maps AiProvider enum value to provider name (string)
 * @param provider - AiProvider enum value
 * @returns Provider name string
 */
export function mapProviderEnumToName(provider: AiProvider): string {
  switch (provider) {
    case AiProvider.Google:
      return 'Google'
    case AiProvider.Groq:
      return 'Groq'
    default:
      return 'Unknown'
  }
}

<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 via-white to-gray-50">
    <!-- Navigation Header -->
    <AppHeader 
      tagline="Learn more about our AI learning platform" 
      :use-animation="true"
    >
      <template #navigation>
        <!-- Empty navigation for About page -->
      </template>
    </AppHeader>

    <!-- Main Content -->
    <div class="max-w-5xl mx-auto px-4 py-12 sm:px-6 lg:px-8">
      <!-- Hero Section -->
      <section class="text-center mb-16">
        <div class="mb-6 flex justify-center">
          <img 
            src="/logo.png" 
            alt="PromptLab Logo" 
            class="rounded-lg shadow-lg"
            style="width: 50%; height: auto;"
          />
        </div>
        <h2 class="text-5xl font-bold text-gray-900 mb-4">
          About PromptLab
        </h2>
        <p class="text-xl text-gray-600 max-w-3xl mx-auto leading-relaxed">
          An interactive learning platform designed to help beginners understand how prompts 
          and Large Language Models (LLMs) work, and improve the quality of daily AI conversations.
        </p>
      </section>

      <!-- Mission Section -->
      <section class="mb-16 bg-white rounded-xl shadow-md p-8 border border-gray-200">
        <h3 class="text-3xl font-bold text-gray-900 mb-4 flex items-center gap-3">
          <span>🎯</span>
          Our Mission
        </h3>
        <p class="text-lg text-gray-700 leading-relaxed mb-4">
          PromptLab empowers users to master the art of AI interaction through hands-on experimentation 
          and real-time feedback. Whether you're new to AI or looking to refine your prompting skills, 
          our platform provides the tools and insights you need to communicate effectively with AI models.
        </p>
        <p class="text-lg text-gray-700 leading-relaxed">
          We believe that understanding how LLMs work and learning to craft effective prompts are 
          essential skills in the AI era. PromptLab makes this learning process intuitive, 
          engaging, and immediately applicable.
        </p>
      </section>

      <!-- Current Features -->
      <section class="mb-16">
        <h3 class="text-3xl font-bold text-gray-900 mb-6 flex items-center gap-3">
          <span>✅</span>
          Current Features
        </h3>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Feature Card 1 -->
          <div class="bg-white rounded-lg shadow-md p-6 border border-gray-200 hover:shadow-lg transition-shadow">
            <div class="text-4xl mb-3">🔄</div>
            <h4 class="text-xl font-semibold text-gray-900 mb-2">
              Multi-Provider Support
            </h4>
            <p class="text-gray-600">
              Connect to multiple AI providers (Google Gemini, OpenAI, Anthropic, Groq) 
              and switch between different models seamlessly.
            </p>
          </div>

          <!-- Feature Card 2 -->
          <div class="bg-white rounded-lg shadow-md p-6 border border-gray-200 hover:shadow-lg transition-shadow">
            <div class="text-4xl mb-3">🎛️</div>
            <h4 class="text-xl font-semibold text-gray-900 mb-2">
              Parameter Control
            </h4>
            <p class="text-gray-600">
              Experiment with key parameters like temperature, top-p, and max tokens 
              to understand how they affect AI responses.
            </p>
          </div>

          <!-- Feature Card 3 -->
          <div class="bg-white rounded-lg shadow-md p-6 border border-gray-200 hover:shadow-lg transition-shadow">
            <div class="text-4xl mb-3">💬</div>
            <h4 class="text-xl font-semibold text-gray-900 mb-2">
              Interactive Playground
            </h4>
            <p class="text-gray-600">
              Test your prompts in real-time and see immediate responses from your selected AI model.
            </p>
          </div>

          <!-- Feature Card 4 -->
          <div class="bg-white rounded-lg shadow-md p-6 border border-gray-200 hover:shadow-lg transition-shadow">
            <div class="text-4xl mb-3">🔍</div>
            <h4 class="text-xl font-semibold text-gray-900 mb-2">
              Response Analysis
            </h4>
            <p class="text-gray-600">
              View detailed metrics about each response including processing time and performance indicators.
            </p>
          </div>

          <!-- Feature Card 5 -->
          <div class="bg-white rounded-lg shadow-md p-6 border border-gray-200 hover:shadow-lg transition-shadow">
            <div class="text-4xl mb-3">🎨</div>
            <h4 class="text-xl font-semibold text-gray-900 mb-2">
              Clean, Modern UI
            </h4>
            <p class="text-gray-600">
              Intuitive interface with light theme design optimized for readability and ease of use.
            </p>
          </div>

          <!-- Feature Card 6 -->
          <div class="bg-white rounded-lg shadow-md p-6 border border-gray-200 hover:shadow-lg transition-shadow">
            <div class="text-4xl mb-3">⚡</div>
            <h4 class="text-xl font-semibold text-gray-900 mb-2">
              Real-time Execution
            </h4>
            <p class="text-gray-600">
              Fast prompt execution with instant feedback and error handling for a smooth learning experience.
            </p>
          </div>
        </div>
      </section>

      <!-- Upcoming Features -->
      <section class="mb-16">
        <h3 class="text-3xl font-bold text-gray-900 mb-6 flex items-center gap-3">
          <span>🚀</span>
          Upcoming Features
        </h3>
        <div class="space-y-4">
          <!-- Upcoming Feature 1 -->
          <div class="bg-gradient-to-r from-blue-50 to-blue-100 rounded-lg shadow-md p-6 border-2 border-blue-300">
            <div class="flex items-start gap-4">
              <div class="text-4xl">📚</div>
              <div>
                <h4 class="text-xl font-semibold text-gray-900 mb-2">
                  Prompt Templates Library
                  <span class="ml-2 px-3 py-1 bg-blue-200 text-blue-800 text-sm rounded-full font-medium">
                    Coming in v1.1.0
                  </span>
                </h4>
                <p class="text-gray-700 mb-2">
                  Pre-built prompt templates for common tasks like summarization, code review, 
                  creative writing, and translation. Save, tag, and share your own templates.
                </p>
                <p class="text-sm text-gray-600 italic">
                  Perfect for beginners to learn effective prompting patterns
                </p>
              </div>
            </div>
          </div>

          <!-- Upcoming Feature 2 -->
          <div class="bg-gradient-to-r from-green-50 to-green-100 rounded-lg shadow-md p-6 border-2 border-green-300">
            <div class="flex items-start gap-4">
              <div class="text-4xl">🔢</div>
              <div>
                <h4 class="text-xl font-semibold text-gray-900 mb-2">
                  Token Counter & Cost Estimator
                  <span class="ml-2 px-3 py-1 bg-green-200 text-green-800 text-sm rounded-full font-medium">
                    Coming in v1.1.0
                  </span>
                </h4>
                <p class="text-gray-700 mb-2">
                  Real-time token counting as you type, with cost estimation based on model pricing. 
                  Track your usage and optimize prompts for efficiency.
                </p>
                <p class="text-sm text-gray-600 italic">
                  Learn to balance cost and quality in your AI interactions
                </p>
              </div>
            </div>
          </div>

          <!-- Upcoming Feature 3 -->
          <div class="bg-gradient-to-r from-purple-50 to-purple-100 rounded-lg shadow-md p-6 border-2 border-purple-300">
            <div class="flex items-start gap-4">
              <div class="text-4xl">⚖️</div>
              <div>
                <h4 class="text-xl font-semibold text-gray-900 mb-2">
                  Response Comparison View
                  <span class="ml-2 px-3 py-1 bg-purple-200 text-purple-800 text-sm rounded-full font-medium">
                    Coming in v1.2.0
                  </span>
                </h4>
                <p class="text-gray-700 mb-2">
                  Send the same prompt to multiple models side-by-side and compare outputs, 
                  latency, and token usage.
                </p>
                <p class="text-sm text-gray-600 italic">
                  Discover which models work best for different tasks
                </p>
              </div>
            </div>
          </div>

          <!-- Upcoming Feature 4 -->
          <div class="bg-gradient-to-r from-orange-50 to-orange-100 rounded-lg shadow-md p-6 border-2 border-orange-300">
            <div class="flex items-start gap-4">
              <div class="text-4xl">💡</div>
              <div>
                <h4 class="text-xl font-semibold text-gray-900 mb-2">
                  Interactive Tooltips & Help System
                  <span class="ml-2 px-3 py-1 bg-orange-200 text-orange-800 text-sm rounded-full font-medium">
                    Coming in v1.1.0
                  </span>
                </h4>
                <p class="text-gray-700 mb-2">
                  Hover over any UI element (Temperature, Top P, Tokens, etc.) to see clear explanations, 
                  examples, and links to documentation.
                </p>
                <p class="text-sm text-gray-600 italic">
                  Learn terminology and concepts without leaving the app
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- View Full Roadmap -->
        <div class="mt-8 text-center">
          <a 
            href="https://github.com/akratovich-pl/ghc-promt-lab/blob/main/docs/UPCOMING_FEATURES.md"
            target="_blank"
            class="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg shadow-md transition-colors"
          >
            <span>📋</span>
            View Full Feature Roadmap
          </a>
        </div>
      </section>

      <!-- Technology Stack -->
      <section class="mb-16 bg-white rounded-xl shadow-md p-8 border border-gray-200">
        <h3 class="text-3xl font-bold text-gray-900 mb-6 flex items-center gap-3">
          <span>🛠️</span>
          Technology Stack
        </h3>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
          <div>
            <h4 class="text-xl font-semibold text-gray-800 mb-3">Backend</h4>
            <ul class="space-y-2 text-gray-700">
              <li class="flex items-center gap-2">
                <span class="text-blue-600">▸</span>
                <strong>.NET 10</strong> Web API
              </li>
              <li class="flex items-center gap-2">
                <span class="text-blue-600">▸</span>
                <strong>Entity Framework Core</strong> with SQLite
              </li>
              <li class="flex items-center gap-2">
                <span class="text-blue-600">▸</span>
                <strong>Clean Architecture</strong> pattern
              </li>
            </ul>
          </div>
          <div>
            <h4 class="text-xl font-semibold text-gray-800 mb-3">Frontend</h4>
            <ul class="space-y-2 text-gray-700">
              <li class="flex items-center gap-2">
                <span class="text-blue-600">▸</span>
                <strong>Vue 3</strong> with TypeScript
              </li>
              <li class="flex items-center gap-2">
                <span class="text-blue-600">▸</span>
                <strong>Vite</strong> build tool
              </li>
              <li class="flex items-center gap-2">
                <span class="text-blue-600">▸</span>
                <strong>Pinia</strong> state management
              </li>
              <li class="flex items-center gap-2">
                <span class="text-blue-600">▸</span>
                <strong>Tailwind CSS</strong> styling
              </li>
            </ul>
          </div>
        </div>
      </section>

      <!-- Footer -->
      <footer class="text-center text-gray-600 py-8 border-t border-gray-200">
        <p class="text-lg mb-2">
          Built with ❤️ for the AI learning community
        </p>
        <p class="text-sm">
          © 2026 PromptLab • Godel Copilot training course project
        </p>
      </footer>
    </div>
  </div>
</template>

<script setup lang="ts">
import AppHeader from '@/components/common/AppHeader.vue'
// No additional logic needed for this static page
</script>

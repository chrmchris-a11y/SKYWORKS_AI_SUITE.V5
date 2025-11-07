import { defineConfig } from 'vitest/config'

export default defineConfig({
  test: {
    // Exclude legacy test files that don't have proper test runner setup
    exclude: [
      '**/node_modules/**',
      '**/dist/**',
      '**/Pages/test-mission-planner.spec.js', // Legacy file without describe/it imports
    ],
  },
})

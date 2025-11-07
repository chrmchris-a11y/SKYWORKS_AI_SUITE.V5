import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './ui',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5210',
    trace: 'on-first-retry',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  webServer: {
    command: 'cd ../Backend && dotnet run --project src/Skyworks.Api/Skyworks.Api.csproj --urls http://localhost:5210',
    url: 'http://localhost:5210/api/v1/health',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});

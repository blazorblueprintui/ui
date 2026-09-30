import { defineConfig } from "@playwright/test";

// Accessibility audit: axe over every demo page plus a keyboard pass over the
// most-used interactive components, against the Server demo in Chromium.
// BB_A11Y_MODE=report (default) records findings without failing; enforce fails on them.
export default defineConfig({
    testDir: ".",
    testMatch: "a11y-*.checks.mjs",
    fullyParallel: true,
    workers: 4,
    retries: 0,
    timeout: 90_000,
    expect: { timeout: 8_000 },
    reporter: [["list"], ["./a11y-reporter.mjs", { outputDir: "test-results/a11y" }]],
    use: {
        baseURL: process.env.BB_SERVER_URL || "http://localhost:7172",
        browserName: "chromium",
        viewport: { width: 1440, height: 1000 },
        locale: "en-GB",
        reducedMotion: "reduce",
        trace: "retain-on-failure",
        ...(process.env.BB_CHROMIUM_CHANNEL ? { channel: process.env.BB_CHROMIUM_CHANNEL } : {}),
    },
    projects: [{ name: "server-chromium" }],
});

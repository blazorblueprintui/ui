import { expect } from "@playwright/test";

export const enforce = process.env.BB_A11Y_MODE === "enforce";

// Opens a demo page and returns its heading, which names the component in the report.
export async function openDemo(page, route) {
    await page.goto(route, { waitUntil: "networkidle" });
    const heading = page.locator("#main-content h1").first();
    await expect(heading).toBeVisible();
    return (await heading.innerText()).trim();
}

// Attaches one page's results for the reporter. Only enforce mode turns findings into failures.
export async function record(testInfo, result) {
    await testInfo.attach("a11y", {
        body: JSON.stringify(result),
        contentType: "application/json",
    });
    if (enforce) {
        expect(
            result.findings.map((f) => `${f.rule ?? f.check}: ${f.actual}`),
            `${result.findings.length} accessibility finding(s) on ${result.route}`,
        ).toEqual([]);
    }
}

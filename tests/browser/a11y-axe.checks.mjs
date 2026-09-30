import { readdirSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { fileURLToPath } from "node:url";
import { test } from "@playwright/test";
import AxeBuilder from "@axe-core/playwright";
import { openDemo, record } from "./a11y-support.mjs";

// axe has no wcag22a tag: WCAG 2.2 added no Level A rules that axe can test.
const tags = ["wcag2a", "wcag2aa", "wcag21a", "wcag21aa", "wcag22aa"];

const pages = fileURLToPath(new URL("../../demos/BlazorBlueprint.Demo.Shared/Pages", import.meta.url));

// Every component, primitive and chart demo, read from the @page directives so new demos are picked up.
function demoRoutes() {
    const routes = new Set();
    for (const file of readdirSync(pages, { recursive: true })) {
        if (!file.endsWith(".razor")) {
            continue;
        }
        const source = readFileSync(join(pages, file), "utf8");
        for (const [, route] of source.matchAll(/@page\s+"(\/(?:components|primitives|charts)\/[^"]+)"/g)) {
            routes.add(route);
        }
    }
    return [...routes].sort();
}

function findings(component, route, violations) {
    return violations.map((v) => ({
        kind: "axe",
        component,
        route,
        rule: v.id,
        severity: v.impact,
        wcag: v.tags.filter((t) => /^wcag\d{3,}$/.test(t)),
        help: v.help,
        helpUrl: v.helpUrl,
        actual: v.nodes[0]?.failureSummary?.replace(/\s+/g, " ").trim() ?? v.help,
        nodes: v.nodes.map((n) => ({
            target: n.target.join(" "),
            html: n.html.length > 200 ? `${n.html.slice(0, 200)}…` : n.html,
        })),
    }));
}

async function scan(page, browser, testInfo, route, component, build) {
    const results = await build(new AxeBuilder({ page }).withTags(tags)).analyze();
    await record(testInfo, {
        kind: "axe",
        route,
        component,
        browser: `${testInfo.project.use.channel ?? "chromium"} ${browser.version()}`,
        engine: `axe-core ${results.testEngine.version}`,
        tags,
        findings: findings(component, route, results.violations),
        incomplete: results.incomplete.map((r) => ({ rule: r.id, nodes: r.nodes.length })),
    });
}

// The header and sidebar are on every page, so they are checked once here and left out below.
test("axe demo layout (header and sidebar)", async ({ page, browser }, testInfo) => {
    const route = "/components/button";
    await openDemo(page, route);
    await scan(page, browser, testInfo, route, "Demo layout", (axe) => axe.exclude("#main-content"));
});

for (const route of demoRoutes()) {
    test(`axe ${route}`, async ({ page, browser }, testInfo) => {
        const component = await openDemo(page, route);
        await scan(page, browser, testInfo, route, component, (axe) => axe.include("#main-content"));
    });
}

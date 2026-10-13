import { mkdirSync, writeFileSync } from "node:fs";
import { join } from "node:path";

const order = ["critical", "serious", "moderate", "minor"];
const rank = (severity) => (order.includes(severity) ? order.indexOf(severity) : order.length);
const cell = (value) => String(value ?? "").replace(/\s+/g, " ").replace(/\|/g, "\\|").trim();

// Keyboard checks run with and without reduced motion; a failure in both is one finding. A failure
// is marked as specific to one setting only when the same check passed under the other.
function merged(results) {
    const passed = new Set(results.flatMap((r) => (r.passed ?? []).map((check) => `${r.route}\u0000${check}\u0000${r.motion}`)));
    const findings = results.flatMap((r) => r.findings);
    const keyboard = new Map();
    const rest = [];
    for (const f of findings) {
        if (f.kind !== "keyboard") {
            rest.push(f);
            continue;
        }
        const key = `${f.route}\u0000${f.check}`;
        const seen = keyboard.get(key);
        if (seen) {
            seen.motion.push(f.motion);
        } else {
            keyboard.set(key, { ...f, motion: [f.motion] });
        }
    }
    const checks = [...keyboard.values()].map((f) => {
        const other = f.motion[0] === "reduce" ? "no-preference" : "reduce";
        return f.motion.length === 1 && passed.has(`${f.route}\u0000${f.check}\u0000${other}`)
            ? { ...f, check: `${f.check} (only with prefers-reduced-motion: ${f.motion[0]})` }
            : f;
    });
    return [...rest, ...checks];
}

// Collects the "a11y" attachments from the audit checks and writes report.json and report.md.
export default class A11yReporter {
    constructor(options = {}) {
        this.outputDir = options.outputDir ?? "test-results/a11y";
        this.results = [];
    }

    onBegin(config) {
        this.baseURL = config.projects[0]?.use?.baseURL;
    }

    onTestEnd(test, result) {
        for (const attachment of result.attachments) {
            if (attachment.name === "a11y" && attachment.body) {
                this.results.push(JSON.parse(attachment.body.toString()));
            }
        }
    }

    onEnd() {
        if (this.results.length === 0) {
            return;
        }
        this.results.sort((a, b) => a.kind.localeCompare(b.kind) || a.route.localeCompare(b.route));
        const findings = merged(this.results).sort(
            (a, b) => a.component.localeCompare(b.component) || rank(a.severity) - rank(b.severity),
        );
        mkdirSync(this.outputDir, { recursive: true });
        writeFileSync(join(this.outputDir, "report.json"), JSON.stringify(this.results, null, 2));
        writeFileSync(join(this.outputDir, "report.md"), this.markdown(findings));
        const axe = findings.filter((f) => f.kind === "axe").length;
        console.log(
            `\nAccessibility: ${axe} axe and ${findings.length - axe} keyboard finding(s). Report: ${join(this.outputDir, "report.md")}`,
        );
    }

    markdown(findings) {
        const axePages = this.results.filter((r) => r.kind === "axe");
        const keyboard = this.results.filter((r) => r.kind === "keyboard");
        const sample = axePages[0] ?? keyboard[0];
        const lines = [
            "# Accessibility audit",
            "",
            `- Host: ${this.baseURL ?? "unknown"} (${sample?.browser ?? "chromium"})`,
            `- Engine: ${axePages[0]?.engine ?? "n/a"}, tags ${axePages[0]?.tags?.join(", ") ?? "n/a"}`,
            `- Pages scanned with axe: ${axePages.length}, with violations: ${axePages.filter((r) => r.findings.length).length}`,
            `- Components with a keyboard pass (with and without reduced motion): ${new Set(keyboard.map((r) => r.route)).size}, with findings: ${new Set(keyboard.filter((r) => r.findings.length).map((r) => r.route)).size}`,
            "",
            "## By component",
            "",
            "| Component | Demo | axe rules | axe nodes | Keyboard findings | Worst severity |",
            "| --- | --- | --- | --- | --- | --- |",
        ];
        const byComponent = new Map();
        for (const f of findings) {
            const key = `${f.component}\u0000${f.route}`;
            const entry = byComponent.get(key) ?? { component: f.component, route: f.route, rules: 0, nodes: 0, keyboard: 0, worst: "minor" };
            if (f.kind === "axe") {
                entry.rules++;
                entry.nodes += f.nodes.length;
            } else {
                entry.keyboard++;
            }
            if (rank(f.severity) < rank(entry.worst)) {
                entry.worst = f.severity;
            }
            byComponent.set(key, entry);
        }
        for (const e of [...byComponent.values()].sort((a, b) => rank(a.worst) - rank(b.worst) || a.component.localeCompare(b.component))) {
            lines.push(`| ${cell(e.component)} | ${e.route} | ${e.rules} | ${e.nodes} | ${e.keyboard} | ${e.worst} |`);
        }

        lines.push("", "## By rule", "", "| Rule or check | Kind | Severity | WCAG | Pages | Nodes |", "| --- | --- | --- | --- | --- | --- |");
        const byRule = new Map();
        for (const f of findings) {
            const key = f.rule ?? f.check;
            const entry = byRule.get(key) ?? { key, kind: f.kind, severity: f.severity, wcag: f.wcag ?? [], pages: new Set(), nodes: 0 };
            entry.pages.add(f.route);
            entry.nodes += f.nodes?.length ?? 1;
            byRule.set(key, entry);
        }
        for (const e of [...byRule.values()].sort((a, b) => rank(a.severity) - rank(b.severity) || b.pages.size - a.pages.size)) {
            lines.push(`| ${cell(e.key)} | ${e.kind} | ${e.severity} | ${e.wcag.join(", ")} | ${e.pages.size} | ${e.nodes} |`);
        }

        lines.push(
            "",
            "## Findings",
            "",
            "| # | Component | Demo URL | Rule or check | What happens | Steps to reproduce | Severity |",
            "| --- | --- | --- | --- | --- | --- | --- |",
        );
        findings.forEach((f, i) => {
            const steps =
                f.kind === "axe"
                    ? `Open ${f.route}, run axe on #main-content. ${f.nodes.length} element(s): ${f.nodes
                          .slice(0, 3)
                          .map((n) => `\`${n.target}\``)
                          .join(", ")}${f.nodes.length > 3 ? ", …" : ""}`
                    : f.steps;
            const rule = f.kind === "axe" ? `axe \`${f.rule}\`` : `keyboard: ${f.check}`;
            lines.push(`| ${i + 1} | ${cell(f.component)} | ${f.route} | ${cell(rule)} | ${cell(f.actual)} | ${cell(steps)} | ${f.severity} |`);
        });
        return `${lines.join("\n")}\n`;
    }

    printsToStdio() {
        return false;
    }
}

import { test } from "@playwright/test";
import { openDemo, record } from "./a11y-support.mjs";

// Keyboard pass over the most-used interactive components. Each check records a finding instead of
// failing, so one broken behaviour does not hide the rest; a missing demo element still fails the test.

const settle = 2_000;

function describeFocus(page) {
    return page.evaluate(() => {
        const el = document.activeElement;
        if (!el || el === document.body) {
            return "<body> (focus lost)";
        }
        const role = el.getAttribute("role");
        const name = (el.getAttribute("aria-label") || el.innerText || el.value || "").trim().replace(/\s+/g, " ").slice(0, 40);
        return `<${el.tagName.toLowerCase()}${role ? ` role="${role}"` : ""}>${name ? ` "${name}"` : ""}`;
    });
}

async function until(page, condition, failure) {
    const deadline = Date.now() + settle;
    for (;;) {
        if (await condition()) {
            return;
        }
        if (Date.now() > deadline) {
            throw new Error(`${failure}; focus is on ${await describeFocus(page)}`);
        }
        await page.waitForTimeout(50);
    }
}

const inPage = (locator, fn, arg) => locator.evaluate(fn, arg, { timeout: 1_000 }).catch(() => false);
const isFocused = (locator) => inPage(locator, (el) => el === document.activeElement);
const holdsFocus = (locator) => inPage(locator, (el) => el.contains(document.activeElement));
const isShown = (locator) => locator.isVisible().catch(() => false);
const isGone = async (locator) => !(await isShown(locator));

// The first element after a section heading that also matches `target`, so examples are picked by heading.
const example = (page, heading, target) =>
    page.locator(`xpath=//*[@id="main-content"]//h2[normalize-space()="${heading}"]/following::*`).and(target).first();

// Reaches `locator` with the keyboard so it gets :focus-visible; falls back to focus() if the round trip misses.
async function tabTo(page, locator) {
    await locator.focus();
    await page.keyboard.press("Shift+Tab");
    await page.keyboard.press("Tab");
    if (!(await isFocused(locator))) {
        await locator.focus();
    }
}

// Outline, shadow (Tailwind rings), border, background and colour, for the element and its parent.
function focusStyle(locator) {
    return locator.evaluate((el) =>
        [el, el.parentElement].map((node) => {
            const s = getComputedStyle(node);
            const outline = s.outlineStyle === "none" || parseFloat(s.outlineWidth) === 0 ? "none" : `${s.outlineStyle} ${s.outlineWidth} ${s.outlineColor}`;
            return [outline, s.boxShadow, s.borderColor, s.backgroundColor, s.color, s.textDecorationLine].join(" / ");
        }).join(" | "),
    );
}

async function tabStops(container) {
    return container.evaluate(
        (el) =>
            [...el.querySelectorAll('a[href], button, input, select, textarea, [tabindex]')].filter(
                (node) => !node.disabled && node.tabIndex >= 0 && node.type !== "hidden" && node.getClientRects().length > 0,
            ).length,
    );
}

function audit(component, route, motion) {
    const findings = [];
    const passed = [];
    return {
        findings,
        passed,
        async check(check, severity, steps, run) {
            try {
                await run();
                passed.push(check);
                return true;
            } catch (error) {
                findings.push({ kind: "keyboard", component, route, motion, check, severity, steps: `Open ${route}. ${steps}`, actual: error.message.split("\n")[0] });
                return false;
            }
        },
    };
}

// Tabs to `target` and compares its focused style with its resting style.
async function focusIndicator(page, { check }, target, label, steps) {
    const resting = await focusStyle(target);
    await tabTo(page, target);
    return check(`${label} shows a visible focus indicator`, "serious", steps, async () => {
        await until(page, () => isFocused(target), `${label} is not focused`);
        await page.waitForTimeout(250);
        if ((await focusStyle(target)) === resting) {
            throw new Error("Outline, ring, border, background and text colour are identical focused and unfocused");
        }
    });
}

// Checks that focus stays inside a modal container for more presses than it has Tab stops.
async function trapped(page, container, key, name) {
    const stops = await tabStops(container);
    for (let i = 1; i <= stops + 1; i++) {
        await page.keyboard.press(key);
        await until(page, () => holdsFocus(container), `After ${i}× ${key} (of ${stops} stops) focus left ${name}`);
    }
}

async function closesWith(page, { check }, { dialog, trigger, name, how, steps }) {
    const closed = await check(`${how} closes the ${name}`, "serious", steps, () => until(page, () => isGone(dialog), `The ${name} is still open`));
    if (closed) {
        await check(`Focus returns to the trigger after ${how}`, "serious", steps, () =>
            until(page, () => isFocused(trigger), "Focus did not return to the trigger"),
        );
    }
}

// Dialog, AlertDialog and Sheet. `escape: false` is for AlertDialog, which documents that Escape does not close it.
async function modal(page, audit, { trigger, name, at, close, escape = true }) {
    const { check } = audit;
    const dialog = page.getByRole(name === "alert dialog" ? "alertdialog" : "dialog");
    const button = dialog.getByRole("button", { name: close, exact: true });
    await focusIndicator(page, audit, trigger, `The ${name} trigger`, at);
    await page.keyboard.press("Enter");
    const opened = await check(`Enter on the trigger opens the ${name}`, "critical", `${at}, press Enter.`, () =>
        until(page, () => isShown(dialog), `No ${name} opened`),
    );
    if (!opened) {
        return;
    }
    await check(`Focus moves into the ${name} when it opens`, "serious", `${at}, press Enter.`, () =>
        until(page, () => holdsFocus(dialog), `Focus stayed outside the ${name}`),
    );
    await check(`Tab keeps focus inside the ${name}`, "serious", `${at}, press Enter, then Tab repeatedly.`, () =>
        trapped(page, dialog, "Tab", `the ${name}`),
    );
    await check(`Shift+Tab keeps focus inside the ${name}`, "serious", `${at}, press Enter, then Shift+Tab repeatedly.`, () =>
        trapped(page, dialog, "Shift+Tab", `the ${name}`),
    );
    if (escape) {
        await page.keyboard.press("Escape");
        await closesWith(page, audit, { dialog, trigger, name, how: "Escape", steps: `${at}, press Enter, then Escape.` });
    } else {
        await button.focus();
        await page.keyboard.press("Enter");
        await closesWith(page, audit, { dialog, trigger, name, how: `“${close}”`, steps: `${at}, press Enter, Tab to “${close}”, press Enter.` });
    }
    if (await isShown(dialog)) {
        return;
    }
    await tabTo(page, trigger);
    await page.keyboard.press("Space");
    const reopened = await check(`Space on the trigger opens the ${name}`, "serious", `${at}, press Space.`, () =>
        until(page, () => isShown(dialog), `No ${name} opened`),
    );
    if (reopened && escape) {
        await button.focus();
        await page.keyboard.press("Enter");
        await closesWith(page, audit, { dialog, trigger, name, how: `“${close}”`, steps: `${at}, press Space, Tab to “${close}”, press Enter.` });
    }
}

// Menu keyboard model shared by DropdownMenu, ContextMenu and Menubar menus. Returns false when focus
// never reaches the menu, since the item checks would then only repeat that finding.
async function menuKeys(page, { check }, menu, at) {
    const items = menu.locator(':scope [role^="menuitem"]:not([aria-disabled="true"]):not([data-disabled])');
    const count = await items.count();
    const index = () => items.evaluateAll((els) => els.indexOf(document.activeElement));
    const inside = await check("Focus moves into the menu when it opens", "serious", at, () =>
        until(page, () => holdsFocus(menu), "Focus stayed outside the menu"),
    );
    if (!inside) {
        return false;
    }
    await check("Focus lands on the first menu item", "moderate", at, () =>
        until(page, async () => (await index()) === 0, "Focus is on the menu container, so the first item needs an extra ArrowDown"),
    );
    const start = await index();
    const resting = await focusStyle(items.nth(start + 1));
    await page.keyboard.press("ArrowDown");
    await check("ArrowDown moves focus to the next menu item", "serious", `${at}, press ArrowDown.`, () =>
        until(page, async () => (await index()) === start + 1, "Focus did not move to the next item"),
    );
    await check("The focused menu item is visibly highlighted", "serious", `${at}, press ArrowDown.`, async () => {
        await page.waitForTimeout(250);
        if ((await focusStyle(items.nth(start + 1))) === resting) {
            throw new Error("The focused item looks the same as when it is not focused");
        }
    });
    await page.keyboard.press("ArrowDown");
    await page.keyboard.press("ArrowUp");
    await check("ArrowUp moves focus to the previous menu item", "moderate", `${at}, press ArrowDown twice, then ArrowUp.`, () =>
        until(page, async () => (await index()) === start + 1, "Focus did not move back one item"),
    );
    await page.keyboard.press("End");
    await check("End moves focus to the last menu item", "moderate", `${at}, press End.`, () =>
        until(page, async () => (await index()) === count - 1, "The last item is not focused"),
    );
    await page.keyboard.press("Home");
    await check("Home moves focus to the first menu item", "moderate", `${at}, press End, Home.`, () =>
        until(page, async () => (await index()) === 0, "The first item is not focused"),
    );
    return true;
}

const scenarios = {
    async dialog(page, audit) {
        const trigger = example(page, "Dialog with Footer", page.getByRole("button", { name: "Edit Profile", exact: true }));
        await modal(page, audit, { trigger, name: "dialog", at: "Tab to “Edit Profile” (Dialog with Footer)", close: "Close" });
    },

    async "alert-dialog"(page, audit) {
        const trigger = example(page, "Basic Example", page.getByRole("button", { name: "Show Dialog", exact: true }));
        await modal(page, audit, { trigger, name: "alert dialog", at: "Tab to “Show Dialog” (Basic Example)", close: "Cancel", escape: false });
    },

    async sheet(page, audit) {
        const trigger = example(page, "Side: Right (Default)", page.getByRole("button", { name: "Open Sheet", exact: true }));
        await modal(page, audit, { trigger, name: "sheet", at: "Tab to “Open Sheet” (Side: Right)", close: "Close" });
    },

    async popover(page, audit) {
        const { check } = audit;
        const trigger = example(page, "Basic Popover", page.getByRole("button", { name: "Open Popover", exact: true }));
        const at = "Tab to “Open Popover” (Basic Popover)";
        const content = page.locator(`[id="${await trigger.getAttribute("aria-controls")}"]`);
        await focusIndicator(page, audit, trigger, "The popover trigger", at);
        await page.keyboard.press("Enter");
        if (!(await check("Enter on the trigger opens the popover", "critical", `${at}, press Enter.`, () => until(page, () => isShown(content), "No popover opened")))) {
            return;
        }
        await check("Focus moves into the popover on open or on the next Tab", "serious", `${at}, press Enter, then Tab.`, async () => {
            if (!(await holdsFocus(content))) {
                await page.keyboard.press("Tab");
                await until(page, () => holdsFocus(content), "Focus did not enter the popover");
            }
        });
        await page.keyboard.press("Escape");
        if (await check("Escape closes the popover", "serious", `${at}, press Enter, then Escape.`, () => until(page, () => isGone(content), "The popover is still open"))) {
            await check("Focus returns to the trigger when the popover closes", "serious", `${at}, press Enter, then Escape.`, () =>
                until(page, () => isFocused(trigger), "Focus did not return to the trigger"),
            );
        }
    },

    async "dropdown-menu"(page, audit) {
        const { check } = audit;
        const trigger = example(page, "Basic Dropdown Menu", page.getByRole("button", { name: "Open Menu", exact: true }));
        const menu = page.getByRole("menu");
        const at = "Tab to “Open Menu” (Basic Dropdown Menu)";
        await focusIndicator(page, audit, trigger, "The menu trigger", at);
        await page.keyboard.press("Enter");
        if (!(await check("Enter on the trigger opens the menu", "critical", `${at}, press Enter.`, () => until(page, () => isShown(menu), "No menu opened")))) {
            return;
        }
        await menuKeys(page, audit, menu, `${at}, press Enter`);
        await page.keyboard.press("Escape");
        if (await check("Escape closes the menu", "serious", `${at}, press Enter, then Escape.`, () => until(page, () => isGone(menu), "The menu is still open"))) {
            await check("Focus returns to the trigger when the menu closes", "serious", `${at}, press Enter, then Escape.`, () =>
                until(page, () => isFocused(trigger), "Focus did not return to the trigger"),
            );
        }
        await tabTo(page, trigger);
        await page.keyboard.press("Space");
        if (await check("Space on the trigger opens the menu", "serious", `${at}, press Space.`, () => until(page, () => isShown(menu), "No menu opened"))) {
            await page.keyboard.press("Escape");
            await until(page, () => isGone(menu), "The menu did not close").catch(() => {});
        }
        await tabTo(page, trigger);
        await page.keyboard.press("ArrowDown");
        if (await check("ArrowDown on the trigger opens the menu", "moderate", `${at}, press ArrowDown.`, () => until(page, () => isShown(menu), "No menu opened"))) {
            await page.keyboard.press("Tab");
            await check("Tab closes the open menu", "moderate", `${at}, press ArrowDown, then Tab.`, () => until(page, () => isGone(menu), "The menu is still open"));
        }
    },

    async "context-menu"(page, audit) {
        const { check } = audit;
        const area = example(page, "Basic Example", page.locator("[data-state]").filter({ hasText: "Right click here" }));
        const menu = page.getByRole("menu");
        const reachable = await check("The trigger area can be reached with Tab", "critical", "Tab through the “Basic Example” section.", async () => {
            const focusable = await area.evaluate((el) => [el, ...el.querySelectorAll("*")].some((node) => node.tabIndex >= 0));
            if (!focusable) {
                throw new Error("Neither the trigger area nor anything inside it is focusable, so keyboard users cannot open the menu");
            }
        });
        if (reachable) {
            await tabTo(page, area);
            await page.keyboard.press("Shift+F10");
            await check("Shift+F10 on the trigger area opens the menu", "critical", "Tab to the “Right click here” area, press Shift+F10.", () =>
                until(page, () => isShown(menu), "No menu opened"),
            );
            await page.keyboard.press("Escape");
        }
        await area.click({ button: "right" });
        const at = "Right-click “Right click here” (Basic Example)";
        if (!(await check("Right-click opens the menu", "critical", at, () => until(page, () => isShown(menu), "No menu opened")))) {
            return;
        }
        await menuKeys(page, audit, menu, at);
        await page.keyboard.press("Escape");
        await check("Escape closes the menu", "serious", `${at}, then press Escape.`, () => until(page, () => isGone(menu), "The menu is still open"));
    },

    async menubar(page, audit) {
        const { check } = audit;
        const bar = example(page, "Example", page.getByRole("menubar"));
        const file = bar.locator('button[role="menuitem"]').nth(0);
        const edit = bar.locator('button[role="menuitem"]').nth(1);
        const menu = page.getByRole("menu");
        const at = "Tab to “File” (Example)";
        await focusIndicator(page, audit, file, "The menubar item", at);
        await page.keyboard.press("Tab");
        await check("The menubar is a single Tab stop", "moderate", `${at}, press Tab.`, () =>
            until(page, async () => !(await holdsFocus(bar)), "Tab moved to the next menubar item instead of leaving the menubar"),
        );
        await file.focus();
        await page.keyboard.press("ArrowRight");
        await check("ArrowRight moves to the next menubar item", "serious", `${at}, press ArrowRight.`, () =>
            until(page, () => isFocused(edit), "“Edit” is not focused"),
        );
        await edit.focus();
        await page.keyboard.press("ArrowDown");
        if (!(await check("ArrowDown on a menubar item opens its menu", "critical", "Focus “Edit”, press ArrowDown.", () => until(page, () => isShown(menu), "No menu opened")))) {
            return;
        }
        await menuKeys(page, audit, menu, "Focus “Edit”, press ArrowDown");
        await page.keyboard.press("ArrowRight");
        await check("ArrowRight in an open menu moves to the next menubar menu", "moderate", "Focus “Edit”, press ArrowDown, then ArrowRight.", () =>
            until(
                page,
                async () => (await bar.locator('button[role="menuitem"]').nth(2).getAttribute("aria-expanded")) === "true" && (await holdsFocus(menu)),
                "“View” did not open with focus in its menu",
            ),
        );
        await page.keyboard.press("Escape");
        if (await check("Escape closes the menu", "serious", "Open a menubar menu, press Escape.", () => until(page, () => isGone(menu), "The menu is still open"))) {
            await check("Focus returns to the menubar item when the menu closes", "serious", "Open a menubar menu, press Escape.", () =>
                until(page, () => holdsFocus(bar), "Focus is outside the menubar"),
            );
        }
    },

    async "navigation-menu"(page, audit) {
        const { check } = audit;
        const nav = example(page, "Simple Links", page.getByRole("navigation"));
        await check("Top-level links can be reached with Tab", "critical", "Tab through the “Simple Links” section.", async () => {
            const stops = await nav.locator("a[href]").evaluateAll((links) => links.filter((a) => a.tabIndex >= 0).length);
            if (stops === 0) {
                throw new Error("Every top-level link has tabindex=\"-1\", so Tab skips the whole menu");
            }
        });
    },

    async select(page, audit) {
        const { check } = audit;
        const trigger = example(page, "Data-Binding Mode", page.getByRole("combobox"));
        const list = page.getByRole("listbox");
        const at = "Tab to “Choose an option” (Data-Binding Mode)";
        const active = () =>
            page.evaluate(() => {
                const el = document.activeElement;
                const ref = el?.getAttribute("aria-activedescendant");
                const option = ref ? document.getElementById(ref) : el?.closest('[role="option"]');
                return option?.textContent.trim() ?? null;
            });
        await focusIndicator(page, audit, trigger, "The select trigger", at);
        const before = (await trigger.innerText()).trim();
        await page.keyboard.press("Enter");
        if (!(await check("Enter on the trigger opens the list", "critical", `${at}, press Enter.`, () => until(page, () => isShown(list), "No listbox opened")))) {
            return;
        }
        let first = null;
        await check("An option is focused or active when the list opens", "serious", `${at}, press Enter.`, () =>
            until(page, async () => (first = await active()) !== null, "No option is focused or referenced by aria-activedescendant"),
        );
        await page.keyboard.press("ArrowDown");
        let next = null;
        await check("ArrowDown moves to the next option", "serious", `${at}, press Enter, then ArrowDown.`, () =>
            until(page, async () => (next = await active()) !== null && next !== first, "The active option did not change"),
        );
        await page.keyboard.press("Enter");
        if (await check("Enter selects the option and closes the list", "serious", `${at}, press Enter, ArrowDown, Enter.`, () => until(page, () => isGone(list), "The list is still open"))) {
            await check("The trigger shows the chosen option", "serious", `${at}, press Enter, ArrowDown, Enter.`, () =>
                until(page, async () => (await trigger.innerText()).trim() !== before, "The trigger text did not change"),
            );
            const returned = await check("Focus returns to the trigger after choosing", "serious", `${at}, press Enter, ArrowDown, Enter.`, () =>
                until(page, () => isFocused(trigger), "Focus did not return to the trigger"),
            );
            if (returned) {
                await page.keyboard.press("Shift+Tab");
                await check("Shift+Tab moves off the trigger after choosing", "minor", `${at}, press Enter, ArrowDown, Enter, then Shift+Tab.`, () =>
                    until(page, async () => !(await isFocused(trigger)), "The first Shift+Tab leaves focus on the trigger; a second one is needed"),
                );
            }
        }
        await tabTo(page, trigger);
        await page.keyboard.press("Space");
        if (await check("Space on the trigger opens the list", "serious", `${at}, press Space.`, () => until(page, () => isShown(list), "No listbox opened"))) {
            await page.keyboard.press("Escape");
            if (await check("Escape closes the list", "serious", `${at}, press Space, then Escape.`, () => until(page, () => isGone(list), "The list is still open"))) {
                await check("Focus returns to the trigger after Escape", "serious", `${at}, press Space, then Escape.`, () =>
                    until(page, () => isFocused(trigger), "Focus did not return to the trigger"),
                );
            }
        }
    },

    async combobox(page, audit) {
        const { check } = audit;
        const trigger = example(page, "Basic Combobox", page.getByRole("combobox"));
        const content = page.locator(`[id="${await trigger.getAttribute("aria-controls")}"]`);
        const options = content.getByRole("option");
        const at = "Tab to “Choose an option” (Basic Combobox)";
        const active = () =>
            page.evaluate(() => {
                const el = document.activeElement;
                const ref = el?.getAttribute("aria-activedescendant");
                const option = ref ? document.getElementById(ref) : el?.closest('[role="option"]');
                return option?.textContent.trim() ?? null;
            });
        await focusIndicator(page, audit, trigger, "The combobox trigger", at);
        await page.keyboard.press("Enter");
        if (!(await check("Enter on the trigger opens the list", "critical", `${at}, press Enter.`, () => until(page, () => isShown(content), "No list opened")))) {
            return;
        }
        const searching = await check("Focus moves to the search box when the list opens", "serious", `${at}, press Enter.`, () =>
            until(page, () => inPage(content, (el) => el.contains(document.activeElement) && document.activeElement.tagName === "INPUT"), "The search box is not focused, so typing and arrow keys do nothing"),
        );
        if (searching) {
            const all = await options.count();
            await page.keyboard.type("s");
            await check("Typing filters the options", "serious", `${at}, press Enter, type “s”.`, () =>
                until(page, async () => (await options.count()) < all, `Still ${all} options after typing`),
            );
            const before = (await trigger.innerText()).trim();
            await page.keyboard.press("ArrowDown");
            const highlighted = await check("ArrowDown highlights an option", "serious", `${at}, press Enter, type “s”, press ArrowDown.`, () =>
                until(page, async () => (await content.locator('[role="option"][aria-selected="true"]').count()) === 1, "No option has aria-selected=true"),
            );
            if (highlighted) {
                await check("The highlighted option is announced (aria-activedescendant)", "serious", `${at}, press Enter, type “s”, press ArrowDown.`, () =>
                    until(page, async () => (await active()) !== null, "The search box has no aria-activedescendant, so screen readers do not announce the highlighted option"),
                );
            }
            await page.keyboard.press("Enter");
            if (await check("Enter chooses the option and closes the list", "serious", `${at}, press Enter, type “s”, ArrowDown, Enter.`, () => until(page, () => isGone(content), "The list is still open"))) {
                await check("The trigger shows the chosen option", "serious", `${at}, press Enter, type “s”, ArrowDown, Enter.`, () =>
                    until(page, async () => (await trigger.innerText()).trim() !== before, "The trigger text did not change"),
                );
                await check("Focus returns to the trigger after choosing", "serious", `${at}, press Enter, type “s”, ArrowDown, Enter.`, () =>
                    until(page, () => isFocused(trigger), "Focus did not return to the trigger"),
                );
            }
        }
        if (await isShown(content)) {
            await page.keyboard.press("Escape");
            await until(page, () => isGone(content), "The list did not close").catch(() => {});
        }
        await tabTo(page, trigger);
        await page.keyboard.press("Enter");
        await until(page, () => isShown(content), "No list opened").catch(() => {});
        await page.keyboard.press("Escape");
        if (await check("Escape closes the list", "serious", `${at}, press Enter, then Escape.`, () => until(page, () => isGone(content), "The list is still open"))) {
            await check("Focus returns to the trigger after Escape", "serious", `${at}, press Enter, then Escape.`, () =>
                until(page, () => isFocused(trigger), "Focus did not return to the trigger"),
            );
        }
    },

    async tabs(page, audit) {
        const { check } = audit;
        const list = example(page, "Default", page.getByRole("tablist"));
        const tabs = list.getByRole("tab");
        const at = "Tab to “Account” (Default)";
        await focusIndicator(page, audit, tabs.first(), "The active tab", at);
        await page.keyboard.press("ArrowRight");
        if (await check("ArrowRight moves focus to the next tab", "serious", `${at}, press ArrowRight.`, () => until(page, () => isFocused(tabs.nth(1)), "“Password” is not focused"))) {
            await check("The focused tab is selected (automatic activation)", "moderate", `${at}, press ArrowRight.`, () =>
                until(page, async () => (await tabs.nth(1).getAttribute("aria-selected")) === "true", "“Password” is focused but not selected"),
            );
        }
        await page.keyboard.press("Home");
        await check("Home moves focus to the first tab", "moderate", `${at}, press ArrowRight, Home.`, () => until(page, () => isFocused(tabs.first()), "“Account” is not focused"));
        await page.keyboard.press("End");
        await check("End moves focus to the last tab", "moderate", `${at}, press End.`, () => until(page, () => isFocused(tabs.last()), "The last tab is not focused"));
        await page.keyboard.press("Home");
        await until(page, async () => (await tabs.first().getAttribute("aria-selected")) === "true", "“Account” is not selected").catch(() => {});
        const panel = page.locator(`[id="${await tabs.first().getAttribute("aria-controls")}"]`);
        await page.keyboard.press("Tab");
        await check("Tab moves from the tab list into the active panel", "serious", `${at}, press Tab.`, () =>
            until(page, () => holdsFocus(panel), "Focus did not move into the tab panel"),
        );
        await panel.focus();
        await page.keyboard.press("Shift+Tab");
        await check("Shift+Tab from the panel returns to the active tab", "serious", `${at}, press Tab, then Shift+Tab.`, () =>
            until(page, () => isFocused(tabs.first()), "Focus did not return to “Account”"),
        );
    },

    async accordion(page, audit) {
        const { check } = audit;
        const first = example(page, "Default", page.getByRole("button", { name: "Is it customizable?", exact: true }));
        const second = example(page, "Default", page.getByRole("button", { name: "Is it styled?", exact: true }));
        const third = example(page, "Default", page.getByRole("button", { name: "Is it animated?", exact: true }));
        const at = "Tab to “Is it customizable?” (Default)";
        await focusIndicator(page, audit, first, "The accordion trigger", at);
        await page.keyboard.press("Enter");
        await check("Enter expands the focused item", "critical", `${at}, press Enter.`, () =>
            until(page, async () => (await first.getAttribute("aria-expanded")) === "true", "aria-expanded is not true"),
        );
        await page.keyboard.press("Space");
        await check("Space collapses the focused item", "serious", `${at}, press Enter, then Space.`, () =>
            until(page, async () => (await first.getAttribute("aria-expanded")) === "false", "aria-expanded is not false"),
        );
        await page.keyboard.press("ArrowDown");
        await check("ArrowDown moves focus to the next trigger", "moderate", `${at}, press ArrowDown.`, () => until(page, () => isFocused(second), "“Is it styled?” is not focused"));
        await page.keyboard.press("ArrowUp");
        await check("ArrowUp moves focus to the previous trigger", "moderate", `${at}, press ArrowDown, ArrowUp.`, () => until(page, () => isFocused(first), "“Is it customizable?” is not focused"));
        await page.keyboard.press("End");
        await check("End moves focus to the last trigger", "moderate", `${at}, press End.`, () => until(page, () => isFocused(third), "“Is it animated?” is not focused"));
        await page.keyboard.press("Home");
        await check("Home moves focus to the first trigger", "moderate", `${at}, press End, Home.`, () => until(page, () => isFocused(first), "“Is it customizable?” is not focused"));
    },

    async "date-picker"(page, audit) {
        const { check } = audit;
        const trigger = example(page, "Preselected Date", page.locator('button[aria-haspopup]'));
        const content = page.locator(`[id="${await trigger.getAttribute("aria-controls")}"]`);
        const at = "Tab to the date button (Preselected Date)";
        const day = () => page.evaluate(() => document.activeElement?.getAttribute("aria-label") || document.activeElement?.textContent.trim() || null);
        await focusIndicator(page, audit, trigger, "The date picker trigger", at);
        const before = (await trigger.innerText()).trim();
        await page.keyboard.press("Enter");
        if (!(await check("Enter on the trigger opens the calendar", "critical", `${at}, press Enter.`, () => until(page, () => isShown(content), "No calendar opened")))) {
            return;
        }
        let start = null;
        await check("Focus moves to a day in the calendar when it opens", "serious", `${at}, press Enter.`, () =>
            until(page, async () => (await holdsFocus(content)) && /\d/.test((start = await day()) ?? ""), "No day in the calendar is focused"),
        );
        await page.keyboard.press("ArrowRight");
        let next = null;
        await check("ArrowRight moves focus to the next day", "serious", `${at}, press Enter, then ArrowRight.`, () =>
            until(page, async () => (await holdsFocus(content)) && (next = await day()) !== start, "The focused day did not change"),
        );
        await page.keyboard.press("ArrowDown");
        await check("ArrowDown moves focus to the same day next week", "moderate", `${at}, press Enter, ArrowRight, ArrowDown.`, () =>
            until(page, async () => (await holdsFocus(content)) && (await day()) !== next, "The focused day did not change"),
        );
        await page.keyboard.press("Enter");
        if (await check("Enter selects the focused day and closes the calendar", "serious", `${at}, press Enter, ArrowRight, ArrowDown, Enter.`, () => until(page, () => isGone(content), "The calendar is still open"))) {
            await check("The trigger shows the chosen date", "serious", `${at}, press Enter, ArrowRight, ArrowDown, Enter.`, () =>
                until(page, async () => (await trigger.innerText()).trim() !== before, "The trigger text did not change"),
            );
            await check("Focus returns to the trigger after choosing a date", "serious", `${at}, press Enter, ArrowRight, ArrowDown, Enter.`, () =>
                until(page, () => isFocused(trigger), "Focus did not return to the trigger"),
            );
        }
        await tabTo(page, trigger);
        await page.keyboard.press("Enter");
        await until(page, () => isShown(content), "No calendar opened").catch(() => {});
        await page.keyboard.press("Escape");
        if (await check("Escape closes the calendar", "serious", `${at}, press Enter, then Escape.`, () => until(page, () => isGone(content), "The calendar is still open"))) {
            await check("Focus returns to the trigger after Escape", "serious", `${at}, press Enter, then Escape.`, () =>
                until(page, () => isFocused(trigger), "Focus did not return to the trigger"),
            );
        }
    },

    async datagrid(page, audit) {
        const { check } = audit;
        const grid = example(page, "Basic DataGrid", page.getByRole("grid"));
        const header = grid.getByRole("columnheader").first();
        const at = "Tab to the “Name” column header (Basic DataGrid)";
        await focusIndicator(page, audit, header, "The sortable column header", at);
        await page.keyboard.press("Enter");
        await check("Enter on a sortable header sorts the column", "serious", `${at}, press Enter.`, () =>
            until(page, async () => (await header.getAttribute("aria-sort")) === "ascending", "aria-sort did not become ascending"),
        );
        await page.keyboard.press("Space");
        await check("Space on a sortable header sorts the column", "serious", `${at}, press Enter, then Space.`, () =>
            until(page, async () => (await header.getAttribute("aria-sort")) === "descending", "aria-sort did not become descending"),
        );
        await page.keyboard.press("ArrowRight");
        await check("ArrowRight moves to the next cell (grid pattern)", "moderate", `${at}, press ArrowRight.`, () =>
            until(page, () => isFocused(grid.getByRole("columnheader").nth(1)), "Focus did not move to the next column header"),
        );

        const selection = example(page, "Row Selection", page.getByRole("grid"));
        const rows = selection.locator("tbody tr");
        const where = "Tab into the rows of the “Row Selection” grid";
        await selection.getByRole("columnheader").last().focus();
        let reached = false;
        for (let i = 0; i < 4 && !reached; i++) {
            await page.keyboard.press("Tab");
            reached = await holdsFocus(rows.first());
        }
        if (!(await check("Tab reaches the first row", "serious", `${where}.`, () => until(page, () => holdsFocus(rows.first()), "Focus did not reach the first row")))) {
            return;
        }
        await focusIndicator(page, audit, rows.first(), "The focused row", `${where}.`);
        await page.keyboard.press("ArrowDown");
        await check("ArrowDown moves to the next row", "serious", `${where}, press ArrowDown.`, () => until(page, () => holdsFocus(rows.nth(1)), "The second row is not focused"));
        await rows.first().focus();
        await page.keyboard.press("Tab");
        await page.keyboard.press("Tab");
        await check("Rows are not each a separate Tab stop (grid pattern)", "moderate", `${where}, press Tab twice.`, () =>
            until(page, async () => !(await holdsFocus(rows.nth(1))), "Tab moved from row to row, via each row's checkbox"),
        );
        await rows.nth(1).focus();
        await page.keyboard.press("Space");
        await check("Space toggles the focused row's selection", "serious", `${where}, press ArrowDown, then Space.`, () =>
            until(page, async () => (await rows.nth(1).getAttribute("aria-selected")) === "true", "aria-selected is not true"),
        );
    },

    async tooltip(page, audit) {
        const { check } = audit;
        const trigger = example(page, "Basic Usage", page.locator('[id$="-trigger"]'));
        const tip = page.getByRole("tooltip");
        const at = "Tab to “Hover me” (Basic Usage)";
        await focusIndicator(page, audit, trigger, "The tooltip trigger", at);
        if (!(await check("Keyboard focus shows the tooltip", "serious", at, () => until(page, () => isShown(tip), "No tooltip appeared")))) {
            return;
        }
        await check("The trigger is described by the tooltip", "moderate", at, async () => {
            const id = await tip.getAttribute("id");
            const describedBy = (await trigger.getAttribute("aria-describedby")) ?? "";
            if (!id || !describedBy.split(/\s+/).includes(id)) {
                throw new Error(`aria-describedby is "${describedBy}", tooltip id is "${id}"`);
            }
        });
        await page.keyboard.press("Escape");
        await check("Escape hides the tooltip", "serious", `${at}, press Escape.`, () => until(page, () => isGone(tip), "The tooltip is still shown"));
        await check("Focus stays on the trigger after Escape", "moderate", `${at}, press Escape.`, () => until(page, () => isFocused(trigger), "Focus moved off the trigger"));
        await tabTo(page, trigger);
        await until(page, () => isShown(tip), "No tooltip appeared").catch(() => {});
        await page.keyboard.press("Tab");
        await check("Moving focus away hides the tooltip", "serious", `${at}, press Tab.`, () => until(page, () => isGone(tip), "The tooltip is still shown"));
    },
};

// Reduced motion changes how some overlays open, so every scenario runs with and without it.
for (const motion of ["no-preference", "reduce"]) {
    test.describe(`prefers-reduced-motion: ${motion}`, () => {
        test.use({ reducedMotion: motion });
        for (const [route, run] of Object.entries(scenarios)) {
            test(`keyboard /components/${route}`, async ({ page }, testInfo) => {
                const path = `/components/${route}`;
                const component = await openDemo(page, path);
                const result = audit(component, path, motion);
                await run(page, result);
                await record(testInfo, { kind: "keyboard", route: path, component, motion, passed: result.passed, findings: result.findings });
            });
        }
    });
}

import { test, expect } from '@playwright/test';

test.beforeEach(async ({ page }) => {
    await page.goto('/');
    // Charts initialize only after the interactive circuit is ready.
    await expect(page.locator('#chart-callback svg')).toBeVisible();
});

test('mobile date range presets follow bound ranges, custom lists and external resets', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    const trigger = page.getByRole('button', { name: 'Report dates', exact: true });
    const select = page.locator('[data-drp-presets-select] select:visible');
    const selected = () => select.locator('option:checked');
    const load = async name => {
        await page.keyboard.press('Escape');
        await expect(page.locator('[data-drp]')).toHaveCount(0);
        await page.getByRole('button', { name, exact: true }).click();
        await trigger.click();
        await expect(select).toBeVisible();
    };
    await trigger.click();
    await expect(selected()).toHaveText('Custom');
    await load('Load today');
    await expect(selected()).toHaveText('Today');
    await load('Load yesterday');
    await expect(selected()).toHaveText('Yesterday');
    await load('Load last week');
    await expect(selected()).toHaveText('Last 7 days');
    await load('Load stored range');
    await expect(selected()).toHaveText('Custom');
    await load('Use custom presets');
    await expect(selected()).toHaveText('Stored report');
    await load('Load today');
    await expect(selected()).toHaveText('Today');
    await expect(select).toHaveValue('2');
    await load('Use default presets');
    await expect(selected()).toHaveText('Today');
    await expect(select).toHaveValue('0');
    await load('Reset report');
    await expect(page.locator('#report-range')).toHaveText('empty');
    await expect(selected()).toHaveText('Select date range');
    await select.selectOption({ label: 'Today' });
    await expect(selected()).toHaveText('Today');
    await page.locator('[data-drp]').getByRole('button', { name: 'Apply', exact: true }).click();
    await expect(page.locator('#report-range')).not.toHaveText('empty');
});

test('restoring strokes awaits the drawn pad when starting or returning from typed mode', async ({ page }) => {
    const signature = page.locator('#signature');
    await expect(signature.locator('canvas')).toHaveCount(0);
    for (let restored = 1; restored <= 2; restored++) {
        await signature.getByRole('button', { name: 'Restore strokes', exact: true }).click();
        await expect(page.locator('#restore-count')).toHaveText(String(restored));
        await expect(page.locator('#signature-kind')).toHaveText('Drawn');
        await signature.getByRole('button', { name: 'Read strokes', exact: true }).click();
        await expect(page.locator('#stroke-count')).toHaveText('1');
        await expect(signature.locator('canvas')).toBeVisible();
        if (restored === 1) {
            await signature.getByRole('radio', { name: 'Type', exact: true }).click();
            await expect(signature.locator('canvas')).toHaveCount(0);
        }
    }
});

test('pivot parent labels redraw even when leaf labels and aggregate values stay the same', async ({ page }) => {
    const pivot = page.locator('#pivot');
    await expect(pivot).toContainText('North');
    await pivot.getByRole('button').click();
    await expect(pivot).toContainText('South');
    await expect(pivot).not.toContainText('North');
    await expect(pivot).toContainText('City');
    await expect(pivot).toContainText('10');
});

test('gantt redraws nonworking bands without changing its date range', async ({ page }) => {
    const bands = () => page.locator('#working-gantt div').evaluateAll(nodes =>
        nodes.filter(node => node.classList.contains('bb:bg-muted/50')).map(node => node.getAttribute('style')));
    await expect.poll(bands).not.toEqual([]);
    const before = await bands();
    await page.getByRole('button', { name: 'Change nonworking days' }).click();
    await expect.poll(bands).not.toEqual(before);
    expect((await bands()).every(style => style.includes('width:34px'))).toBe(true);
});

test('gantt displays an initial build error and recovers when data is corrected', async ({ page }) => {
    const gantt = page.locator('#invalid-gantt');
    await expect(gantt).toContainText("Two tasks share the identifier 'Task'");
    await gantt.getByRole('button', { name: 'Toggle invalid data' }).click();
    await expect(gantt).not.toContainText('Two tasks share');
    await expect(gantt.locator('tbody tr')).toHaveCount(1);
    await gantt.getByRole('button', { name: 'Toggle invalid data' }).click();
    await expect(gantt).toContainText("Two tasks share the identifier 'Task'");
});

test('ListBox has its visible accessible label and navigation never consumes the next Tab', async ({ page }) => {
    const list = page.getByRole('listbox', { name: 'Probe list', exact: true });
    await list.focus();
    await list.press('End');
    await expect(list).toHaveAttribute('aria-activedescendant', /-option-2$/);
    await list.press('Tab');
    // WebKit's native focus policy may skip ordinary buttons; Tab must still leave the list.
    await expect(list).not.toBeFocused();
    await list.focus();
    await list.press('Home');
    await expect(list).toHaveAttribute('aria-activedescendant', /-option-0$/);
    await list.press('Shift+Tab');
    await expect(page.getByRole('textbox', { name: 'Before list' })).toBeFocused();
});

test('checkbox activates once per key and slider navigation leaves Tab available', async ({ page }) => {
    const checkbox = page.getByRole('checkbox', { name: 'Probe checkbox' });
    await checkbox.focus();
    await checkbox.press(' ');
    await expect(checkbox).toBeChecked();
    await checkbox.press('Enter');
    await expect(checkbox).not.toBeChecked();
    await checkbox.press(' ');
    await expect(checkbox).toBeChecked();
    await checkbox.press('Tab');
    const slider = page.getByRole('slider');
    await expect(slider).toBeFocused();
    const before = Number(await slider.getAttribute('aria-valuenow'));
    await slider.press('ArrowRight');
    await expect.poll(async () => Number(await slider.getAttribute('aria-valuenow'))).toBeGreaterThan(before);
    await slider.press('Tab');
    await expect(slider).not.toBeFocused();
});

test('chart callbacks can be added, replaced and removed without remounting or changing data', async ({ page }) => {
    const fire = () => page.evaluate(async () => {
        const echarts = await import('/_content/BlazorBlueprint.Components/lib/echarts/echarts.min.js');
        const chart = echarts.getInstanceByDom(document.querySelector('#chart-callback [data-slot=map-chart]'));
        chart.trigger('click', { seriesType: 'map', seriesName: 'Visitors', seriesIndex: 0,
            dataIndex: 0, data: { bbSourceIndex: 0 }, name: 'United States of America', value: 5 });
    });
    await page.getByRole('button', { name: 'Enable chart handler' }).click();
    await expect(page.locator('#chart-listening')).toHaveText('True');
    // The callback is installed asynchronously after the parent's render.
    // Wait for the actual subscription before emitting one event, avoiding duplicate test clicks.
    const subscribed = () => page.evaluate(async () => {
        const echarts = await import('/_content/BlazorBlueprint.Components/lib/echarts/echarts.min.js');
        return !echarts.getInstanceByDom(document.querySelector('#chart-callback [data-slot=map-chart]')).isSilent('click');
    });
    await expect.poll(subscribed).toBe(true);
    await fire();
    await expect(page.locator('#chart-clicks')).toHaveText('1');
    await page.getByRole('button', { name: 'Replace chart handler' }).click();
    await expect(page.locator('#chart-alternate')).toHaveText('True');
    await fire();
    await expect(page.locator('#chart-clicks')).toHaveText('11');
    await page.getByRole('button', { name: 'Disable chart handler' }).click();
    await expect.poll(subscribed).toBe(false);
    await fire();
    await expect(page.locator('#chart-clicks')).toHaveText('11');
    await page.getByRole('button', { name: 'Enable chart handler' }).click();
    await expect.poll(subscribed).toBe(true);
    await fire();
    await expect(page.locator('#chart-clicks')).toHaveText('21');
});

test('explicit null clears bound files while an omitted Files parameter retains selections', async ({ page }) => {
    const file = { name: 'state-change.txt', mimeType: 'text/plain', buffer: Buffer.from('regression') };
    const bound = page.locator('#upload');
    await bound.locator('input[type=file]').setInputFiles(file);
    await expect(page.locator('#file-count')).toHaveText('1');
    await expect(bound).toContainText(file.name);
    await bound.getByRole('button', { name: 'Reset files to null' }).click();
    await expect(page.locator('#file-count')).toHaveText('null');
    await expect(bound).not.toContainText(file.name);
    await bound.locator('input[type=file]').setInputFiles(file);
    await expect(page.locator('#file-count')).toHaveText('1');
    await bound.getByRole('button', { name: 'Reset files to empty' }).click();
    await expect(bound).not.toContainText(file.name);
    const uncontrolled = page.locator('#uncontrolled-upload');
    await uncontrolled.locator('input[type=file]').setInputFiles(file);
    await expect(uncontrolled).toContainText(file.name);
    await uncontrolled.getByRole('button', { name: 'Unrelated render' }).click();
    await expect(page.locator('#unrelated-renders')).toHaveText('1');
    await expect(uncontrolled).toContainText(file.name);
});

test('radar tooltips display untrusted labels as text', async ({ page }) => {
    const payload = '<img src="/missing-tooltip-image" onerror="window.tooltipExecuted=true">';
    const formatter = await page.evaluate(async payload => {
        const renderer = await import('/_content/BlazorBlueprint.Components/js/echarts-renderer.js');
        const element = document.createElement('div');
        element.id = 'tooltip-fixture';
        element.style = 'width:500px;height:350px';
        document.body.append(element);
        await renderer.initialize(element.id, {
            radar: { indicator: [{ name: '{a|1}{b|' + payload + '}', max: 100 }, { name: 'B', max: 100 }, { name: 'C', max: 100 }] },
            tooltip: { show: true },
            series: [{ type: 'radar', name: payload, data: [{ value: [50, 60, 70] }] }],
        });
        const echarts = await import('/_content/BlazorBlueprint.Components/lib/echarts/echarts.min.js');
        const chart = echarts.getInstanceByDom(element);
        chart.dispatchAction({ type: 'showTip', seriesIndex: 0, dataIndex: 0 });
        return chart.getOption().tooltip[0].formatter({ marker: '<span>marker</span>', seriesName: payload, value: [payload, 60, 70] });
    }, payload);
    expect(formatter).not.toContain('<img');
    expect(formatter).toContain('&lt;img');
    expect(formatter).toContain('<span>marker</span>');
    const tooltip = page.locator('#tooltip-fixture div').filter({ hasText: payload }).last();
    await expect(tooltip).toBeVisible();
    await expect(page.locator('#tooltip-fixture img')).toHaveCount(0);
    expect(await page.evaluate(() => window.tooltipExecuted)).toBeUndefined();
});

test('Escape in a MultiSelect inside a dialog closes only the list, after typing or after clicking an option', async ({ page }) => {
    const section = page.locator('#multiselect-dialog');
    const dialogOpen = page.locator('#picker-dialog-open');
    const list = page.locator('[role=listbox][aria-multiselectable=true]');
    const trigger = page.getByRole('combobox', { name: 'Picker', exact: true });
    await section.getByRole('button', { name: 'Open picker dialog' }).click();
    await expect(dialogOpen).toHaveText('True');

    // Typed in the search box: the input's own handler takes Escape.
    await trigger.click();
    await expect(list).toBeVisible();
    const search = page.locator('input[id$="-search"]');
    await expect(search).toBeFocused();
    await search.pressSequentially('Be');
    await page.keyboard.press('Escape');
    await expect(list).toHaveCount(0);
    await expect(dialogOpen).toHaveText('True');

    // Clicked an option: focus has left the search box, so the open list must take Escape itself.
    await trigger.click();
    await expect(list).toBeVisible();
    await list.getByRole('option', { name: 'Alpha' }).click();
    await expect(page.locator('#picker-values')).toHaveText('a');
    await page.keyboard.press('Escape');
    await expect(list).toHaveCount(0);
    await expect(dialogOpen).toHaveText('True');
    await expect(page.locator('#picker-values')).toHaveText('a');
    await expect(trigger).toBeVisible();

    // With the list closed, Escape goes back to closing the dialog.
    await page.keyboard.press('Escape');
    await expect(dialogOpen).toHaveText('False');
});

test('a label names the Combobox and MultiSelect triggers, opens them, and focus returns to their own id', async ({ page }) => {
    await expect(page.getByTestId('fruit')).toHaveAttribute('id', 'fruit-trigger');
    await expect(page.getByTestId('fruit')).toHaveAttribute('aria-invalid', 'true');
    for (const [name, id] of [['Fruit', 'fruit-trigger'], ['Fruits', 'fruits-trigger']]) {
        const trigger = page.getByRole('combobox', { name, exact: true });
        await expect(trigger).toHaveAttribute('id', id);
        await page.locator(`label[for="${id}"]`).click();
        await expect(trigger).toHaveAttribute('aria-expanded', 'true');
        await page.keyboard.press('Escape');
        await expect(trigger).toHaveAttribute('aria-expanded', 'false');
        await expect(trigger).toBeFocused();
    }
});

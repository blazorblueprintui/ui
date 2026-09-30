// The column resize behaviour of table-columns.js, the module BbDataGrid and BbGantt share.
// The drag reports every managed column's width, not only the one dragged, so what these tests
// mostly pin down is what the report does with a column that has nothing to report.

import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const repo = new URL('../../', import.meta.url);
const urlFor = source => `data:text/javascript;base64,${Buffer.from(source).toString('base64')}`;
const moduleUrl = urlFor(await readFile(
  new URL('src/BlazorBlueprint.Components/wwwroot/js/table-columns.js', repo), 'utf8'));
const { initColumnResize, setupResizeHandles, dispose } = await import(moduleUrl);

const GRID_ID = 'grid-1';
const POINTER_ID = 1;

function fakeElement(attributes = {}) {
  const attrs = new Map(Object.entries(attributes));
  const listeners = new Map();
  return {
    attrs,
    style: {},
    getAttribute: key => attrs.get(key) ?? null,
    addEventListener(type, fn) {
      listeners.set(type, [...(listeners.get(type) ?? []), fn]);
    },
    removeEventListener(type, fn) {
      listeners.set(type, (listeners.get(type) ?? []).filter(handler => handler !== fn));
    },
    setPointerCapture() {},
    releasePointerCapture() {},
    fire(type, options = {}) {
      for (const handler of [...(listeners.get(type) ?? [])]) {
        handler({
          type,
          target: this,
          pointerId: POINTER_ID,
          clientX: 0,
          stopPropagation() {},
          preventDefault() {},
          ...options
        });
      }
    }
  };
}

/**
 * A grid shaped the way the module expects one: a container holding a table, a header cell and a
 * <col> per managed column, one resize handle per draggable column, and an interop stub that
 * records the calls. `columns` gives each column's header measurement, so a column measured at
 * zero is the control column the module has to leave alone. `declared` gives the inline width a
 * <col> already carries, which is what the grid renders for the width that column declares.
 */
function buildGrid(columns, { tableWidth = 600, declared = {} } = {}) {
  const documentListeners = new Map();
  const calls = [];
  const cols = new Map();
  const headers = new Map();
  const handles = new Map();

  const document = {
    body: { style: {} },
    addEventListener(type, fn) {
      documentListeners.set(type, [...(documentListeners.get(type) ?? []), fn]);
    },
    removeEventListener(type, fn) {
      documentListeners.set(type, (documentListeners.get(type) ?? []).filter(handler => handler !== fn));
    }
  };

  const table = fakeElement();
  table.getBoundingClientRect = () => ({ left: 0, top: 0, width: tableWidth, height: 40 });
  table.querySelectorAll = selector => {
    if (selector === 'thead th[data-column-id]') return [...headers.values()];
    if (selector === '[data-resize-handle]') return [...handles.values()];
    return [];
  };
  table.querySelector = selector => {
    const match = /^colgroup col\[data-column-id="(.+)"\]$/.exec(selector);
    return match ? cols.get(match[1]) ?? null : null;
  };

  const container = fakeElement();
  container.querySelector = selector => (selector === 'table' ? table : null);

  for (const column of columns) {
    const col = fakeElement();
    col.style.width = declared[column.id];
    cols.set(column.id, col);

    const header = fakeElement({ 'data-column-id': column.id });
    header.getBoundingClientRect = () => ({ left: 0, top: 0, width: column.width, height: 32 });
    headers.set(column.id, header);

    handles.set(column.id, fakeElement({ 'data-resize-handle': column.id }));
  }

  return {
    container,
    table,
    cols,
    handles,
    calls,
    document,
    dotNetRef: {
      invokeMethodAsync(method, ...args) {
        calls.push([method, ...args]);
        return Promise.resolve();
      }
    },
    fireDocument(type, options = {}) {
      for (const handler of [...(documentListeners.get(type) ?? [])]) {
        handler({ type, pointerId: POINTER_ID, clientX: 0, preventDefault() {}, ...options });
      }
    },
    /** A whole gesture: press the handle, move the pointer, release it. */
    drag(columnId, { from = 200, to = 200 } = {}) {
      handles.get(columnId).fire('pointerdown', { clientX: from });
      this.fireDocument('pointermove', { clientX: to });
      this.fireDocument('pointerup', { clientX: to });
    },
    /** The args of the OnResizeCompleted call the gesture made. */
    payload() {
      const call = calls.find(([method]) => method === 'OnResizeCompleted');
      assert.ok(call, 'the drag should have reported its widths to Blazor');
      return { dragged: call[1], widths: call[2] };
    }
  };
}

async function withGrid(columns, body, options) {
  const savedDocument = Object.getOwnPropertyDescriptor(globalThis, 'document');
  const savedCss = Object.getOwnPropertyDescriptor(globalThis, 'CSS');
  const grid = buildGrid(columns, options);
  Object.defineProperty(globalThis, 'document', { configurable: true, value: grid.document });
  Object.defineProperty(globalThis, 'CSS', { configurable: true, value: { escape: value => value } });

  try {
    initColumnResize(grid.container, grid.dotNetRef, GRID_ID, options?.minWidth ?? 50);
    setupResizeHandles(GRID_ID);
    await body(grid);
  } finally {
    dispose(GRID_ID);
    for (const [key, descriptor] of [['document', savedDocument], ['CSS', savedCss]]) {
      if (descriptor) Object.defineProperty(globalThis, key, descriptor);
      else delete globalThis[key];
    }
  }
}

const NAME = { id: 'name', width: 200 };
// The control column: its header is screen-reader text only, so there is nothing to measure.
const ACTIONS = { id: 'actions', width: 0 };

test('the columns a drag manages are frozen at the width they measure', async () => {
  await withGrid([NAME, ACTIONS], grid => {
    grid.handles.get('name').fire('pointerdown', { clientX: 200 });

    assert.equal(grid.cols.get('name').style.width, '200px');
    // The table is locked too, or the other columns scale to make room for the dragged one.
    assert.equal(grid.table.style.width, '600px');
  });
});

test('a column that measures nothing is not frozen at zero', async () => {
  await withGrid([NAME, ACTIONS], grid => {
    grid.handles.get('name').fire('pointerdown', { clientX: 200 });

    assert.equal(grid.cols.get('actions').style.width, undefined,
      'a zero measurement is an absence of a width, not a width of zero');
  });
});

test('the drag reports the width it settled on', async () => {
  await withGrid([NAME, ACTIONS], grid => {
    grid.drag('name', { from: 200, to: 260 });

    const { dragged, widths } = grid.payload();
    assert.equal(dragged, 'name');
    assert.equal(widths.name, 260);
    assert.equal(grid.cols.get('name').style.width, '260px');
  });
});

test('a column that measures nothing is left out of the report rather than reported as zero', async () => {
  await withGrid([NAME, ACTIONS], grid => {
    grid.drag('name', { from: 200, to: 260 });

    const { widths } = grid.payload();
    assert.ok(!('actions' in widths),
      'reporting the zero is what put "0px" into the column state');
    assert.deepEqual(Object.values(widths).filter(width => !(width > 0)), []);
  });
});

test('a column already carrying a zero inline width is still left out of the report', async () => {
  // What an older build left behind: a column frozen at zero, which this drag's snapshot skips
  // because there is nothing to freeze.
  await withGrid([NAME, ACTIONS], grid => {
    grid.drag('name', { from: 200, to: 260 });

    assert.ok(!('actions' in grid.payload().widths));
  }, { declared: { actions: '0px' } });
});

test('a column that declares a width is not reported as one the drag resized', async () => {
  // A column this drag left unfrozen still carries the width it declares on its <col>, which is
  // not a width the drag measured and must not be committed as one.
  await withGrid([NAME, ACTIONS], grid => {
    grid.drag('name', { from: 200, to: 260 });

    assert.ok(!('actions' in grid.payload().widths));
    assert.equal(grid.cols.get('actions').style.width, '80px', 'the declared width is left alone');
  }, { declared: { actions: '80px' } });
});

test('a declared percentage is not reported as pixels', async () => {
  // parseFloat('20%') is 20, which would be committed as 20px.
  await withGrid([NAME, ACTIONS], grid => {
    grid.drag('name', { from: 200, to: 260 });

    assert.ok(!('actions' in grid.payload().widths));
  }, { declared: { actions: '20%' } });
});

test('a drag past the minimum width reports the minimum, not a collapsed width', async () => {
  await withGrid([NAME, ACTIONS], grid => {
    grid.drag('name', { from: 200, to: 0 });

    assert.equal(grid.payload().widths.name, 50);
  }, { minWidth: 50 });
});

test('the width a drag settles on is what the table is left drawn at', async () => {
  await withGrid([NAME, ACTIONS], grid => {
    grid.drag('name', { from: 200, to: 260 });

    assert.equal(grid.table.style.width, '660px');
  });
});

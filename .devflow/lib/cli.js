#!/usr/bin/env node

/**
 * DevFlow CLI Helper
 *
 * Centralized command-line interface for DevFlow state queries and operations.
 * Reduces permission prompts by standardizing all queries into a single command pattern.
 *
 * Usage:
 *   node .devflow/lib/cli.js query <query-type> [args...]
 *
 * Examples:
 *   node .devflow/lib/cli.js query active_feature
 *   node .devflow/lib/cli.js query has_spec feature-name
 *   node .devflow/lib/cli.js query feature_count
 */

const fs = require('fs');
const path = require('path');

// ============================================================================
// State Reading
// ============================================================================

function readState() {
    const statePath = path.resolve(process.cwd(), '.devflow/state.json');

    if (!fs.existsSync(statePath)) {
        return null;
    }

    try {
        const content = fs.readFileSync(statePath, 'utf8');
        return JSON.parse(content);
    } catch (error) {
        return null;
    }
}

function resolveFeatureKey(featureName) {
    const state = readState();
    if (!state) return null;

    // If no feature name provided, use active feature
    if (!featureName || featureName === '') {
        return state.active_feature;
    }

    // Find feature by partial match
    const keys = Object.keys(state.features);
    return keys.find(k => k.includes(featureName)) || null;
}

// ============================================================================
// Query Handlers
// ============================================================================

const queries = {
    // ========== Active Feature Queries ==========

    active_feature: () => {
        const state = readState();
        return state?.active_feature || 'none';
    },

    active_feature_name: () => {
        const state = readState();
        if (!state || !state.active_feature) return 'N/A';
        return state.features[state.active_feature]?.display_name || 'Unknown';
    },

    active_phase: () => {
        const state = readState();
        if (!state || !state.active_feature) return 'N/A';
        return state.features[state.active_feature]?.phase || 'Unknown';
    },

    active_progress: () => {
        const state = readState();
        if (!state || !state.active_feature) return 'N/A';
        const feature = state.features[state.active_feature];
        return `${feature?.current_task}/tasks`;
    },

    // ========== Feature Count Queries ==========

    feature_count: () => {
        const state = readState();
        return state ? Object.keys(state.features).length.toString() : '0';
    },

    pending_count: () => {
        const state = readState();
        if (!state) return '0';
        return Object.values(state.features).filter(f => f.status === 'pending').length.toString();
    },

    active_count: () => {
        const state = readState();
        if (!state) return '0';
        return Object.values(state.features).filter(f => f.status === 'active').length.toString();
    },

    paused_count: () => {
        const state = readState();
        if (!state) return '0';
        return Object.values(state.features).filter(f => f.status === 'paused').length.toString();
    },

    completed_count: () => {
        const state = readState();
        if (!state) return '0';
        return Object.values(state.features).filter(f => f.status === 'completed').length.toString();
    },

    // ========== Feature-Specific Queries ==========

    feature_exists: (featureName) => {
        const key = resolveFeatureKey(featureName);
        return key ? 'yes' : 'no';
    },

    has_spec: (featureName) => {
        const key = resolveFeatureKey(featureName);
        if (!key) return 'no';

        const specPath = path.resolve(process.cwd(), `.devflow/features/${key}/spec.md`);
        return fs.existsSync(specPath) ? 'yes' : 'no';
    },

    has_plan: (featureName) => {
        const key = resolveFeatureKey(featureName);
        if (!key) return 'no';

        const planPath = path.resolve(process.cwd(), `.devflow/features/${key}/plan.md`);
        return fs.existsSync(planPath) ? 'yes' : 'no';
    },

    has_tasks: (featureName) => {
        const key = resolveFeatureKey(featureName);
        if (!key) return 'no';

        const tasksPath = path.resolve(process.cwd(), `.devflow/features/${key}/tasks.md`);
        return fs.existsSync(tasksPath) ? 'yes' : 'no';
    },

    current_phase: (featureName) => {
        const key = resolveFeatureKey(featureName);
        if (!key) return 'unknown';

        const state = readState();
        return state?.features[key]?.phase || 'unknown';
    },

    // ========== Metadata Queries ==========

    latest_feature: () => {
        const state = readState();
        if (!state) return 'None';

        const keys = Object.keys(state.features).sort().reverse();
        return keys[0] || 'None';
    },

    last_initialized: () => {
        const state = readState();
        if (!state || !state.initialized_at) return 'Never';

        try {
            const date = new Date(state.initialized_at);
            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric'
            });
        } catch {
            return 'Unknown';
        }
    },

    // ========== File System Queries ==========

    code_detected: () => {
        try {
            const glob = require('glob');
            const files = glob.sync('**/*.{js,ts,cs,py}', {
                ignore: ['**/node_modules/**', '**/.git/**'],
                cwd: process.cwd(),
                nodir: true
            });
            return files.length > 0 ? 'yes' : 'no';
        } catch {
            return 'no';
        }
    },

    package_files: () => {
        try {
            const glob = require('glob');
            const commonFiles = ['package.json', 'requirements.txt', 'pom.xml'];
            const found = commonFiles.filter(f => fs.existsSync(path.resolve(process.cwd(), f)));

            // Also check for .csproj files
            const csproj = glob.sync('*.csproj', { cwd: process.cwd() });

            const allFiles = found.concat(csproj);
            return allFiles.length > 0 ? allFiles.join(', ') : 'none';
        } catch {
            return 'none';
        }
    },

    markdown_count: (excludePattern) => {
        try {
            const glob = require('glob');
            const ignore = [
                '**/node_modules/**',
                '**/.git/**',
                '**/dist/**',
                '**/build/**',
                '**/.devflow/**',
                'CLAUDE.md'
            ];

            // Add additional excludes if provided
            if (excludePattern && excludePattern !== '') {
                const additionalExcludes = excludePattern.split(',').map(p => p.trim());
                ignore.push(...additionalExcludes);
            }

            const files = glob.sync('**/*.md', {
                ignore,
                cwd: process.cwd()
            });

            return files.length.toString();
        } catch {
            return '0';
        }
    },

    doc_count: () => {
        try {
            const glob = require('glob');
            const files = glob.sync('**/*.md', {
                ignore: [
                    '**/node_modules/**',
                    '**/.git/**',
                    '**/dist/**',
                    '**/build/**',
                    '**/.devflow/**',
                    'CLAUDE.md'
                ],
                cwd: process.cwd()
            });
            return files.length.toString();
        } catch {
            return '0';
        }
    }
};

// ============================================================================
// Update Operations (State Mutations)
// ============================================================================

const { readState: readStateUtil, writeState: writeStateUtil, validateSchema } = require('./state-io.js');

const updateOperations = {
    'init-or-migrate-state': () => {
        const { initializeOrMigrateState } = require('./state-io.js');

        try {
            const state = initializeOrMigrateState();
            const featureCount = Object.keys(state.features).length;
            const wasPreserved = featureCount > 0;

            return {
                success: true,
                preserved: wasPreserved,
                features_count: featureCount,
                active_feature: state.active_feature,
                message: wasPreserved
                    ? `Preserved existing state.json (${featureCount} feature${featureCount !== 1 ? 's' : ''})`
                    : 'Created new state.json'
            };
        } catch (error) {
            return {
                success: false,
                error: error.message
            };
        }
    },

    'transition-phase': (featureKey, newPhase, currentTask = null) => {
        const state = readStateUtil();

        if (!state.features[featureKey]) {
            return {
                success: false,
                error: `Feature ${featureKey} not found in state`
            };
        }

        const feature = state.features[featureKey];
        const previousPhase = feature.phase;

        // Validate phase transition
        const validPhases = ['SPEC', 'PLAN', 'TASKS', 'EXECUTE', 'VALIDATE', 'DONE'];
        if (!validPhases.includes(newPhase)) {
            return {
                success: false,
                error: `Invalid phase: ${newPhase}. Valid phases: ${validPhases.join(', ')}`
            };
        }

        // Update phase
        state.features[featureKey].phase = newPhase;

        // Set current_task if provided
        if (currentTask !== null) {
            state.features[featureKey].current_task = currentTask;
        }

        // Validate schema
        const validation = validateSchema(state);
        if (!validation.valid) {
            return {
                success: false,
                errors: validation.errors,
                message: 'State validation failed'
            };
        }

        // Write atomically
        try {
            writeStateUtil(state);
            return {
                success: true,
                feature: featureKey,
                display_name: feature.display_name,
                previous_phase: previousPhase,
                new_phase: newPhase,
                current_task: currentTask || feature.current_task,
                message: `Transitioned from ${previousPhase} to ${newPhase}`
            };
        } catch (error) {
            return {
                success: false,
                error: error.message
            };
        }
    },

    'set-current-task': (featureKey, taskId) => {
        const state = readStateUtil();

        if (!state.features[featureKey]) {
            return {
                success: false,
                error: `Feature ${featureKey} not found`
            };
        }

        state.features[featureKey].current_task = taskId;

        const validation = validateSchema(state);
        if (!validation.valid) {
            return {
                success: false,
                errors: validation.errors
            };
        }

        try {
            writeStateUtil(state);
            return {
                success: true,
                feature: featureKey,
                current_task: taskId,
                message: `Updated current_task to ${taskId}`
            };
        } catch (error) {
            return {
                success: false,
                error: error.message
            };
        }
    },

    'set-active': (featureKey) => {
        const state = readStateUtil();

        if (!state.features[featureKey]) {
            return {
                success: false,
                error: `Feature ${featureKey} not found`
            };
        }

        // Deactivate other features
        Object.keys(state.features).forEach(key => {
            if (key !== featureKey && state.features[key].status === 'active') {
                state.features[key].status = 'pending';
            }
        });

        // Activate this feature
        state.features[featureKey].status = 'active';
        state.active_feature = featureKey;

        const validation = validateSchema(state);
        if (!validation.valid) {
            return {
                success: false,
                errors: validation.errors
            };
        }

        try {
            writeStateUtil(state);
            return {
                success: true,
                feature: featureKey,
                display_name: state.features[featureKey].display_name,
                message: `Set ${featureKey} as active feature`
            };
        } catch (error) {
            return {
                success: false,
                error: error.message
            };
        }
    },

    'create-feature': (featureKey, displayName, workflowType = 'full', concernsJson = '[]') => {
        const state = readStateUtil();

        if (state.features[featureKey]) {
            return {
                success: false,
                error: `Feature ${featureKey} already exists`
            };
        }

        const timestamp = new Date().toISOString();
        const concerns = JSON.parse(concernsJson);

        state.features[featureKey] = {
            display_name: displayName,
            status: state.active_feature ? 'pending' : 'active',
            phase: 'SPEC',
            workflow_type: workflowType,
            current_task: 0,
            concerns: concerns,
            created_at: timestamp,
            completed_at: null,
            snapshot: null
        };

        // Set as active if no other active feature
        if (!state.active_feature) {
            state.active_feature = featureKey;
        }

        const validation = validateSchema(state);
        if (!validation.valid) {
            return {
                success: false,
                errors: validation.errors
            };
        }

        try {
            writeStateUtil(state);
            return {
                success: true,
                feature: featureKey,
                display_name: displayName,
                workflow_type: workflowType,
                status: state.features[featureKey].status,
                message: `Created feature ${featureKey}`
            };
        } catch (error) {
            return {
                success: false,
                error: error.message
            };
        }
    },

    'mark-complete': (featureKey) => {
        const state = readStateUtil();

        if (!state.features[featureKey]) {
            return {
                success: false,
                error: `Feature ${featureKey} not found`
            };
        }

        const feature = state.features[featureKey];
        const completedAt = new Date().toISOString();

        state.features[featureKey].phase = 'DONE';
        state.features[featureKey].status = 'completed';
        state.features[featureKey].completed_at = completedAt;
        state.features[featureKey].snapshot = null;

        // Clear active_feature if this was active
        if (state.active_feature === featureKey) {
            state.active_feature = null;
        }

        const validation = validateSchema(state);
        if (!validation.valid) {
            return {
                success: false,
                errors: validation.errors
            };
        }

        try {
            writeStateUtil(state);
            return {
                success: true,
                feature: featureKey,
                display_name: feature.display_name,
                completed_at: completedAt,
                message: `Marked feature ${featureKey} as complete`
            };
        } catch (error) {
            return {
                success: false,
                error: error.message
            };
        }
    },

    'set-status': (featureKey, status) => {
        const state = readStateUtil();

        if (!state.features[featureKey]) {
            return {
                success: false,
                error: `Feature ${featureKey} not found`
            };
        }

        const validStatuses = ['pending', 'active', 'paused', 'completed'];
        if (!validStatuses.includes(status)) {
            return {
                success: false,
                error: `Invalid status: ${status}. Valid: ${validStatuses.join(', ')}`
            };
        }

        state.features[featureKey].status = status;

        const validation = validateSchema(state);
        if (!validation.valid) {
            return {
                success: false,
                errors: validation.errors
            };
        }

        try {
            writeStateUtil(state);
            return {
                success: true,
                feature: featureKey,
                status: status,
                message: `Updated status to ${status}`
            };
        } catch (error) {
            return {
                success: false,
                error: error.message
            };
        }
    },

    'set-snapshot': (featureKey, snapshotPath) => {
        const state = readStateUtil();

        if (!state.features[featureKey]) {
            return {
                success: false,
                error: `Feature ${featureKey} not found`
            };
        }

        // Convert 'null' string to actual null
        const snapshot = snapshotPath === 'null' ? null : snapshotPath;
        state.features[featureKey].snapshot = snapshot;

        const validation = validateSchema(state);
        if (!validation.valid) {
            return {
                success: false,
                errors: validation.errors
            };
        }

        try {
            writeStateUtil(state);
            return {
                success: true,
                feature: featureKey,
                snapshot: snapshot,
                message: snapshot ? `Set snapshot to ${snapshot}` : 'Cleared snapshot'
            };
        } catch (error) {
            return {
                success: false,
                error: error.message
            };
        }
    },

    'init-validation': (featureKey, criteriaTotal) => {
        const state = readStateUtil();

        if (!state.features[featureKey]) {
            return {
                success: false,
                error: `Feature ${featureKey} not found`
            };
        }

        const total = parseInt(criteriaTotal, 10);
        const timestamp = new Date().toISOString();

        state.features[featureKey].validation = {
            started_at: timestamp,
            criteria_total: total,
            criteria_passed: 0,
            criteria_failed: 0,
            criteria_pending: total,
            issues: []
        };

        const validation = validateSchema(state);
        if (!validation.valid) {
            return {
                success: false,
                errors: validation.errors
            };
        }

        try {
            writeStateUtil(state);
            return {
                success: true,
                feature: featureKey,
                criteria_total: total,
                message: `Initialized validation with ${total} criteria`
            };
        } catch (error) {
            return {
                success: false,
                error: error.message
            };
        }
    },

    'update-validation-metrics': (featureKey, passed, failed, pending) => {
        const state = readStateUtil();

        if (!state.features[featureKey]) {
            return {
                success: false,
                error: `Feature ${featureKey} not found`
            };
        }

        if (!state.features[featureKey].validation) {
            return {
                success: false,
                error: `Feature ${featureKey} has no validation object`
            };
        }

        state.features[featureKey].validation.criteria_passed = parseInt(passed, 10);
        state.features[featureKey].validation.criteria_failed = parseInt(failed, 10);
        state.features[featureKey].validation.criteria_pending = parseInt(pending, 10);

        const validation = validateSchema(state);
        if (!validation.valid) {
            return {
                success: false,
                errors: validation.errors
            };
        }

        try {
            writeStateUtil(state);
            return {
                success: true,
                feature: featureKey,
                passed: parseInt(passed, 10),
                failed: parseInt(failed, 10),
                pending: parseInt(pending, 10),
                message: `Updated validation metrics`
            };
        } catch (error) {
            return {
                success: false,
                error: error.message
            };
        }
    },

    'add-issue': (featureKey, issueJson) => {
        const state = readStateUtil();

        if (!state.features[featureKey]) {
            return {
                success: false,
                error: `Feature ${featureKey} not found`
            };
        }

        if (!state.features[featureKey].validation) {
            return {
                success: false,
                error: `Feature ${featureKey} has no validation object. Run init-validation first.`
            };
        }

        const issue = JSON.parse(issueJson);
        state.features[featureKey].validation.issues.push(issue);

        const validation = validateSchema(state);
        if (!validation.valid) {
            return {
                success: false,
                errors: validation.errors
            };
        }

        try {
            writeStateUtil(state);
            return {
                success: true,
                feature: featureKey,
                issue_id: issue.id,
                message: `Added issue #${issue.id}`
            };
        } catch (error) {
            return {
                success: false,
                error: error.message
            };
        }
    },

    'update-issue-status': (featureKey, issueId, status, fixedAt = null) => {
        const state = readStateUtil();

        if (!state.features[featureKey]) {
            return {
                success: false,
                error: `Feature ${featureKey} not found`
            };
        }

        if (!state.features[featureKey].validation) {
            return {
                success: false,
                error: `Feature ${featureKey} has no validation object`
            };
        }

        const issues = state.features[featureKey].validation.issues;
        const issue = issues.find(i => i.id === parseInt(issueId, 10));

        if (!issue) {
            return {
                success: false,
                error: `Issue #${issueId} not found`
            };
        }

        issue.status = status;
        if (fixedAt && fixedAt !== 'null') {
            issue.fixed_at = fixedAt;
        }

        const validation = validateSchema(state);
        if (!validation.valid) {
            return {
                success: false,
                errors: validation.errors
            };
        }

        try {
            writeStateUtil(state);
            return {
                success: true,
                feature: featureKey,
                issue_id: parseInt(issueId, 10),
                status: status,
                message: `Updated issue #${issueId} status to ${status}`
            };
        } catch (error) {
            return {
                success: false,
                error: error.message
            };
        }
    }
};

// ============================================================================
// Command Handlers
// ============================================================================

function handleQuery(args) {
    const [queryType, ...queryArgs] = args;

    if (!queries[queryType]) {
        process.stderr.write(`Unknown query type: ${queryType}\n`);
        process.stderr.write(`Available queries: ${Object.keys(queries).join(', ')}\n`);
        process.exit(1);
    }

    try {
        const result = queries[queryType](...queryArgs);
        process.stdout.write(result);
    } catch (error) {
        process.stderr.write(`Query error: ${error.message}\n`);
        process.exit(1);
    }
}

function handleUpdate(args) {
    const [operation, ...operationArgs] = args;

    if (!updateOperations[operation]) {
        process.stderr.write(`Unknown update operation: ${operation}\n`);
        process.stderr.write(`Available operations: ${Object.keys(updateOperations).join(', ')}\n`);
        process.exit(1);
    }

    try {
        const result = updateOperations[operation](...operationArgs);
        // Always output as JSON for easy parsing
        process.stdout.write(JSON.stringify(result, null, 2));

        // Exit with error code if operation failed
        if (!result.success) {
            process.exit(1);
        }
    } catch (error) {
        const errorResult = {
            success: false,
            error: error.message,
            stack: error.stack
        };
        process.stdout.write(JSON.stringify(errorResult, null, 2));
        process.exit(1);
    }
}

// ============================================================================
// Main Entry Point
// ============================================================================

const commands = {
    query: handleQuery,
    update: handleUpdate
};

function main() {
    const [,, command, ...args] = process.argv;

    if (!command || !commands[command]) {
        process.stderr.write('DevFlow CLI Helper\n\n');
        process.stderr.write('Usage: node .devflow/lib/cli.js <command> [args...]\n\n');
        process.stderr.write('Commands:\n');
        process.stderr.write('  query <query-type> [args...]   - Query DevFlow state (read-only)\n');
        process.stderr.write('  update <operation> [args...]   - Update DevFlow state (writes)\n\n');
        process.stderr.write('Query Examples:\n');
        process.stderr.write('  node .devflow/lib/cli.js query active_feature\n');
        process.stderr.write('  node .devflow/lib/cli.js query has_spec feature-name\n\n');
        process.stderr.write('Update Examples:\n');
        process.stderr.write('  node .devflow/lib/cli.js update transition-phase feature-key EXECUTE 1.1\n');
        process.stderr.write('  node .devflow/lib/cli.js update set-current-task feature-key 1.2\n');
        process.stderr.write('  node .devflow/lib/cli.js update mark-complete feature-key\n');
        process.exit(1);
    }

    commands[command](args);
}

// Run main if called directly
if (require.main === module) {
    main();
}

module.exports = { queries, handleQuery };

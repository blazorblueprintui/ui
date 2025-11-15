/**
 * DevFlow State I/O Utilities
 *
 * Simple, deterministic file operations for state.json management.
 * Used by State Manager Agent for intelligent state transitions.
 */

const fs = require('fs');
const path = require('path');

const STATE_FILE = '.devflow/state.json';
const STATE_BACKUP = '.devflow/state.json.bak';
const STATE_SCHEMA_PATH = '.devflow/state.json.schema';

/**
 * Read state.json from the project root
 * @returns {Object} Parsed state object
 * @throws {Error} If file doesn't exist or JSON is invalid
 */
function readState() {
    try {
        const statePath = path.resolve(process.cwd(), STATE_FILE);

        if (!fs.existsSync(statePath)) {
            throw new Error('State file not found. Has DevFlow been initialized? Run /init first.');
        }

        const content = fs.readFileSync(statePath, 'utf8');
        const state = JSON.parse(content);

        return state;
    } catch (error) {
        if (error instanceof SyntaxError) {
            throw new Error(`State file is corrupted (invalid JSON): ${error.message}`);
        }
        throw error;
    }
}

/**
 * Write state.json to the project root with automatic backup
 * @param {Object} data - State object to write
 * @throws {Error} If write operation fails
 */
function writeState(data) {
    try {
        const statePath = path.resolve(process.cwd(), STATE_FILE);
        const backupPath = path.resolve(process.cwd(), STATE_BACKUP);

        // Create backup of existing state before overwriting
        if (fs.existsSync(statePath)) {
            fs.copyFileSync(statePath, backupPath);
        }

        // Write new state atomically (write to temp, then rename)
        const tempPath = statePath + '.tmp';
        const content = JSON.stringify(data, null, 2);

        fs.writeFileSync(tempPath, content, 'utf8');
        fs.renameSync(tempPath, statePath);

    } catch (error) {
        throw new Error(`Failed to write state file: ${error.message}`);
    }
}

/**
 * Validate state object against schema
 * @param {Object} data - State object to validate
 * @returns {Object} Validation result { valid: boolean, errors: string[] }
 */
function validateSchema(data) {
    const errors = [];

    // Required top-level fields
    if (typeof data.initialized !== 'boolean') {
        errors.push('Missing or invalid field: initialized (must be boolean)');
    }

    if (data.initialized && !data.initialized_at) {
        errors.push('Missing field: initialized_at (required when initialized=true)');
    }

    if (data.active_feature !== null && typeof data.active_feature !== 'string') {
        errors.push('Invalid field: active_feature (must be string or null)');
    }

    if (typeof data.features !== 'object' || Array.isArray(data.features)) {
        errors.push('Missing or invalid field: features (must be object)');
    } else {
        // Validate each feature
        for (const [featureName, feature] of Object.entries(data.features)) {
            const validStatuses = ['pending', 'active', 'paused', 'completed'];
            const validPhases = ['SPEC', 'PLAN', 'TASKS', 'EXECUTE', 'DONE'];

            if (!feature.display_name || typeof feature.display_name !== 'string') {
                errors.push(`Feature ${featureName}: missing or invalid display_name`);
            }

            if (!validStatuses.includes(feature.status)) {
                errors.push(`Feature ${featureName}: invalid status (must be one of: ${validStatuses.join(', ')})`);
            }

            if (!validPhases.includes(feature.phase)) {
                errors.push(`Feature ${featureName}: invalid phase (must be one of: ${validPhases.join(', ')})`);
            }

            // Validate current_task: allow integer 0 or string in "X.Y" format
            const isValidInteger = feature.current_task === 0;
            const isValidString = typeof feature.current_task === 'string' && /^\d+\.\d+$/.test(feature.current_task);

            if (!isValidInteger && !isValidString) {
                errors.push(`Feature ${featureName}: invalid current_task (must be 0 or subtask format "X.Y" like "1.2")`);
            }

            if (!Array.isArray(feature.concerns)) {
                errors.push(`Feature ${featureName}: invalid concerns (must be array)`);
            }

            if (!feature.created_at) {
                errors.push(`Feature ${featureName}: missing created_at`);
            }

            if (feature.snapshot !== null && typeof feature.snapshot !== 'string') {
                errors.push(`Feature ${featureName}: invalid snapshot (must be string or null)`);
            }
        }
    }

    return {
        valid: errors.length === 0,
        errors
    };
}

/**
 * Ensure state.json exists, create default if missing
 * @returns {boolean} True if state was created, false if already existed
 */
function ensureStateExists() {
    const statePath = path.resolve(process.cwd(), STATE_FILE);

    if (fs.existsSync(statePath)) {
        return false;
    }

    // Create default state
    const defaultState = {
        initialized: false,
        initialized_at: null,
        active_feature: null,
        features: {}
    };

    writeState(defaultState);
    return true;
}

/**
 * Initialize state.json with timestamp
 * @returns {Object} Initialized state
 */
function initializeState() {
    const state = {
        initialized: true,
        initialized_at: new Date().toISOString(),
        active_feature: null,
        features: {}
    };

    writeState(state);
    return state;
}

/**
 * Initialize or migrate state.json - preserves existing state during reinitialization
 * @returns {Object} State object (existing if valid, or newly created)
 */
function initializeOrMigrateState() {
    const statePath = path.resolve(process.cwd(), STATE_FILE);

    // If state exists, try to preserve it
    if (fs.existsSync(statePath)) {
        try {
            const state = readState();
            const validation = validateSchema(state);

            if (validation.valid) {
                // State exists and is valid - preserve it
                // Ensure initialized flag and timestamp are set
                if (!state.initialized) {
                    state.initialized = true;
                }
                if (!state.initialized_at) {
                    state.initialized_at = new Date().toISOString();
                    writeState(state);
                }
                return state;
            } else {
                // State exists but invalid - warn and create fresh
                console.error('Warning: Existing state.json failed validation:');
                validation.errors.forEach(err => console.error(`  - ${err}`));
                console.error('Creating fresh state.json...');
            }
        } catch (error) {
            // State exists but corrupted - warn and create fresh
            console.error(`Warning: Existing state.json is corrupted: ${error.message}`);
            console.error('Creating fresh state.json...');
        }
    }

    // No existing state, or it's invalid/corrupted - create fresh
    const newState = {
        initialized: true,
        initialized_at: new Date().toISOString(),
        active_feature: null,
        features: {}
    };

    writeState(newState);
    return newState;
}

/**
 * Restore state from backup
 * @throws {Error} If backup doesn't exist
 */
function restoreFromBackup() {
    const statePath = path.resolve(process.cwd(), STATE_FILE);
    const backupPath = path.resolve(process.cwd(), STATE_BACKUP);

    if (!fs.existsSync(backupPath)) {
        throw new Error('No backup file found');
    }

    fs.copyFileSync(backupPath, statePath);
}

module.exports = {
    readState,
    writeState,
    validateSchema,
    ensureStateExists,
    initializeState,
    initializeOrMigrateState,
    restoreFromBackup
};

// Dark mode toggle functionality

/**
 * Check if dark mode is currently enabled
 * @returns {boolean} True if dark mode is enabled
 */
export function isDarkMode() {
    return document.documentElement.classList.contains('dark');
}

/**
 * Toggle dark mode on/off
 */
export function toggleDarkMode() {
    const html = document.documentElement;
    const isDark = html.classList.contains('dark');

    if (isDark) {
        html.classList.remove('dark');
        localStorage.setItem('theme', 'light');
    } else {
        html.classList.add('dark');
        localStorage.setItem('theme', 'dark');
    }
}

/**
 * Initialize dark mode based on user preference
 * This should be called on page load
 */
export function initializeDarkMode() {
    const theme = localStorage.getItem('theme');
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;

    if (theme === 'dark' || (!theme && prefersDark)) {
        document.documentElement.classList.add('dark');
    }
}

// Initialize dark mode on module load
initializeDarkMode();

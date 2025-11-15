// Theme switcher for dynamically changing theme CSS files

/**
 * Switches the active theme by updating the theme stylesheet link
 * @param {string} themeName - Name of the theme ('default', 'blue', 'orange', 'purple')
 */
export function switchTheme(themeName) {
    const themeLink = document.getElementById('theme-stylesheet');

    if (!themeLink) {
        console.error('Theme stylesheet link not found. Make sure there is a <link> tag with id="theme-stylesheet" in the document.');
        return;
    }

    // Map theme names to their CSS file paths
    const themeFiles = {
        'default': 'styles/theme.css',
        'blue': 'styles/theme-blue.css',
        'orange': 'styles/theme-orange.css',
        'purple': 'styles/theme-purple.css',
        'rose': 'styles/theme-rose.css',
        'yellow': 'styles/theme-yellow.css',
        'green': 'styles/theme-green.css'
    };

    const themePath = themeFiles[themeName];

    if (!themePath) {
        console.error(`Unknown theme: ${themeName}. Available themes: ${Object.keys(themeFiles).join(', ')}`);
        return;
    }

    // Update the href to load the new theme
    themeLink.href = themePath;
}

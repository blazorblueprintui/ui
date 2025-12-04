/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: ["class"],
  content: [
    './Components/**/*.{razor,html,cs}',
  ],
  theme: {
    extend: {
      colors: {
        border: "var(--border)",
        input: "var(--input)",
        ring: "var(--ring)",
        background: "var(--background)",
        foreground: "var(--foreground)",
        primary: {
          DEFAULT: "var(--primary)",
          foreground: "var(--primary-foreground)",
        },
        secondary: {
          DEFAULT: "var(--secondary)",
          foreground: "var(--secondary-foreground)",
        },
        destructive: {
          DEFAULT: "var(--destructive)",
          foreground: "var(--destructive-foreground)",
        },
        muted: {
          DEFAULT: "var(--muted)",
          foreground: "var(--muted-foreground)",
        },
        accent: {
          DEFAULT: "var(--accent)",
          foreground: "var(--accent-foreground)",
        },
        popover: {
          DEFAULT: "var(--popover)",
          foreground: "var(--popover-foreground)",
        },
        card: {
          DEFAULT: "var(--card)",
          foreground: "var(--card-foreground)",
        },
        sidebar: {
          DEFAULT: "var(--sidebar)",
          foreground: "var(--sidebar-foreground)",
          primary: "var(--sidebar-primary)",
          "primary-foreground": "var(--sidebar-primary-foreground)",
          accent: "var(--sidebar-accent)",
          "accent-foreground": "var(--sidebar-accent-foreground)",
          border: "var(--sidebar-border)",
          ring: "var(--sidebar-ring)",
        },
      },
      borderRadius: {
        lg: "var(--radius)",
        md: "calc(var(--radius) - 2px)",
        sm: "calc(var(--radius) - 4px)",
      },
      width: {
        "sidebar": "var(--sidebar-width)",
        "sidebar-mobile": "var(--sidebar-width-mobile)",
        "sidebar-icon": "var(--sidebar-width-icon)",
      },
      keyframes: {
        "accordion-down": {
          from: { height: "0" },
          to: { height: "var(--radix-accordion-content-height)" },
        },
        "accordion-up": {
          from: { height: "var(--radix-accordion-content-height)" },
          to: { height: "0" },
        },
        "enter": {
          from: { opacity: "var(--tw-enter-opacity, 1)", transform: "translate3d(var(--tw-enter-translate-x, 0), var(--tw-enter-translate-y, 0), 0) scale3d(var(--tw-enter-scale, 1), var(--tw-enter-scale, 1), var(--tw-enter-scale, 1)) rotate(var(--tw-enter-rotate, 0))" },
          to: { opacity: "1", transform: "translate3d(0, 0, 0) scale3d(1, 1, 1) rotate(0)" },
        },
        "exit": {
          from: { opacity: "1", transform: "translate3d(0, 0, 0) scale3d(1, 1, 1) rotate(0)" },
          to: { opacity: "var(--tw-exit-opacity, 1)", transform: "translate3d(var(--tw-exit-translate-x, 0), var(--tw-exit-translate-y, 0), 0) scale3d(var(--tw-exit-scale, 1), var(--tw-exit-scale, 1), var(--tw-exit-scale, 1)) rotate(var(--tw-exit-rotate, 0))" },
        },
      },
      animation: {
        "accordion-down": "accordion-down 0.2s ease-out",
        "accordion-up": "accordion-up 0.2s ease-out",
        "in": "enter 0.15s ease-out",
        "out": "exit 0.15s ease-in",
      },
    },
  },
  plugins: [
    // Custom plugin for animate-in/animate-out utilities (similar to tailwindcss-animate)
    function({ addUtilities, matchUtilities, theme }) {
      addUtilities({
        ".animate-in": {
          animationName: "enter",
          animationDuration: theme("animationDuration.DEFAULT", "150ms"),
          "--tw-enter-opacity": "initial",
          "--tw-enter-scale": "initial",
          "--tw-enter-rotate": "initial",
          "--tw-enter-translate-x": "initial",
          "--tw-enter-translate-y": "initial",
        },
        ".animate-out": {
          animationName: "exit",
          animationDuration: theme("animationDuration.DEFAULT", "150ms"),
          "--tw-exit-opacity": "initial",
          "--tw-exit-scale": "initial",
          "--tw-exit-rotate": "initial",
          "--tw-exit-translate-x": "initial",
          "--tw-exit-translate-y": "initial",
        },
      });
      matchUtilities(
        {
          "fade-in": (value) => ({ "--tw-enter-opacity": value }),
          "fade-out": (value) => ({ "--tw-exit-opacity": value }),
        },
        { values: { ...theme("opacity"), DEFAULT: "0" } }
      );
      matchUtilities(
        {
          "zoom-in": (value) => ({ "--tw-enter-scale": value }),
          "zoom-out": (value) => ({ "--tw-exit-scale": value }),
        },
        { values: { ...theme("scale"), DEFAULT: "0" } }
      );
      matchUtilities(
        {
          "slide-in-from-top": (value) => ({ "--tw-enter-translate-y": `-${value}` }),
          "slide-in-from-bottom": (value) => ({ "--tw-enter-translate-y": value }),
          "slide-in-from-left": (value) => ({ "--tw-enter-translate-x": `-${value}` }),
          "slide-in-from-right": (value) => ({ "--tw-enter-translate-x": value }),
          "slide-out-to-top": (value) => ({ "--tw-exit-translate-y": `-${value}` }),
          "slide-out-to-bottom": (value) => ({ "--tw-exit-translate-y": value }),
          "slide-out-to-left": (value) => ({ "--tw-exit-translate-x": `-${value}` }),
          "slide-out-to-right": (value) => ({ "--tw-exit-translate-x": value }),
        },
        { values: theme("spacing") }
      );
    },
  ],
}

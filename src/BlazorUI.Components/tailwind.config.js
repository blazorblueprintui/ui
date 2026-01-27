/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: ["class"],
  content: [
    './Components/**/*.{razor,html,cs}',
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: "var(--font-sans, ui-sans-serif, system-ui, sans-serif)",
        serif: "var(--font-serif, ui-serif, Georgia, serif)",
        mono: "var(--font-mono, ui-monospace, monospace)",
      },
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
        alert: {
          success: {
            DEFAULT: "var(--alert-success)",
            foreground: "var(--alert-success-foreground)",
            bg: "var(--alert-success-bg)",
          },
          info: {
            DEFAULT: "var(--alert-info)",
            foreground: "var(--alert-info-foreground)",
            bg: "var(--alert-info-bg)",
          },
          warning: {
            DEFAULT: "var(--alert-warning)",
            foreground: "var(--alert-warning-foreground)",
            bg: "var(--alert-warning-bg)",
          },
          danger: {
            DEFAULT: "var(--alert-danger)",
            foreground: "var(--alert-danger-foreground)",
            bg: "var(--alert-danger-bg)",
          },
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
          to: { height: "var(--radix-accordion-content-height, auto)" },
        },
        "accordion-up": {
          from: { height: "var(--radix-accordion-content-height, auto)" },
          to: { height: "0" },
        },
        "collapsible-down": {
          from: { height: "0" },
          to: { height: "var(--radix-collapsible-content-height, auto)" },
        },
        "collapsible-up": {
          from: { height: "var(--radix-collapsible-content-height, auto)" },
          to: { height: "0" },
        },
      },
      animation: {
        "accordion-down": "accordion-down 0.2s ease-out",
        "accordion-up": "accordion-up 0.2s ease-out",
        "collapsible-down": "collapsible-down 0.2s ease-out",
        "collapsible-up": "collapsible-up 0.2s ease-out",
      },
    },
  },
  plugins: [],
}

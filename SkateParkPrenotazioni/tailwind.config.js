/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./Areas/**/*.cshtml",
  ],
  darkMode: "media",
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: "#1f7a4d",
          dark: "#155a38",
          light: "#2f9a64",
          foreground: "#ffffff",
        },
        accent: {
          DEFAULT: "#f0a202",
          dark: "#c48200",
          foreground: "#1a1a1a",
        },
        asphalt: {
          DEFAULT: "#2c3038",
          muted: "#5c6370",
          soft: "#8b919c",
        },
        park: {
          sky: "#d7eef8",
          mist: "#eef6f1",
          concrete: "#eceae6",
          rail: "#3d4450",
        },
      },
      fontFamily: {
        display: ["Oswald", "ui-sans-serif", "system-ui", "sans-serif"],
        sans: ["Figtree", "ui-sans-serif", "system-ui", "sans-serif"],
      },
      backgroundImage: {
        "park-hero":
          "linear-gradient(135deg, rgba(31,122,77,0.88) 0%, rgba(44,48,56,0.72) 48%, rgba(240,162,2,0.35) 100%), radial-gradient(ellipse at 20% 20%, rgba(215,238,248,0.55), transparent 55%)",
        "park-page":
          "linear-gradient(180deg, #d7eef8 0%, #eef6f1 42%, #eceae6 100%)",
      },
      keyframes: {
        "fade-up": {
          "0%": { opacity: "0", transform: "translateY(12px)" },
          "100%": { opacity: "1", transform: "translateY(0)" },
        },
        "soft-pulse": {
          "0%, 100%": { opacity: "0.55" },
          "50%": { opacity: "0.9" },
        },
      },
      animation: {
        "fade-up": "fade-up 0.7s ease-out both",
        "fade-up-delay": "fade-up 0.9s ease-out 0.15s both",
        "soft-pulse": "soft-pulse 4s ease-in-out infinite",
      },
    },
  },
  plugins: [],
};

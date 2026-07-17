import type { Config } from "tailwindcss";

const config: Config = {
  content: ["./app/**/*.{ts,tsx}", "./components/**/*.{ts,tsx}", "./lib/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        ink: "#17201b",
        moss: "#2f5d50",
        mint: "#d8efe5",
        paper: "#f7f5ef",
        line: "#d9d6ca"
      }
    }
  },
  plugins: []
};

export default config;

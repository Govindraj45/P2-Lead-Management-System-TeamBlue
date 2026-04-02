// Import the "defineConfig" helper from Vite — this gives us autocomplete and type checking
import { defineConfig } from 'vite'
// Import the React plugin so Vite knows how to handle React (JSX) files
import react from '@vitejs/plugin-react'
// Import the Tailwind CSS plugin so Vite can process our Tailwind styles
import tailwindcss from '@tailwindcss/vite'

// This is the main configuration object for Vite — the build tool that runs our frontend
export default defineConfig({
  // Plugins extend Vite's abilities — here we add React support and Tailwind CSS support
  plugins: [react(), tailwindcss()],

  // Settings for running tests with Vitest (a test runner built into Vite)
  test: {
    // Use "jsdom" to simulate a browser environment so tests can work with HTML elements
    environment: 'jsdom',
    // Make test functions like "describe" and "it" available everywhere without importing them
    globals: true,
    // Run this setup file before every test to prepare things like mocks
    setupFiles: './src/test/setup.js',
  },

  // Settings for the local development server (what runs when you type "npm run dev")
  server: {
    // Proxy settings redirect certain URL requests to a different server
    proxy: {
      // Any request starting with "/api" will be forwarded to the backend server at port 5000
      '/api': {
        target: 'http://localhost:5000',
        // "changeOrigin" makes the backend think the request came from its own domain
        changeOrigin: true,
      },
    },
  },
})

/*
 * FILE SUMMARY:
 * This is the Vite configuration file — Vite is the build tool that powers our frontend.
 * It tells Vite to use the React plugin (so we can write JSX), the Tailwind CSS plugin
 * (so we can use utility classes for styling), and sets up a proxy so that API calls
 * made during development are forwarded to the .NET backend running on port 5000.
 * It also configures Vitest so we can run unit tests in a simulated browser environment.
 */

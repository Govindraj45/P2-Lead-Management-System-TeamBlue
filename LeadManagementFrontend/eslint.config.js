// Import the base ESLint JavaScript rules (catches common coding mistakes)
import js from '@eslint/js'
// Import a list of global variables that exist in browsers (like "window", "document", etc.)
import globals from 'globals'
// Import the React Hooks plugin — it checks that we follow the "Rules of Hooks" in React
import reactHooks from 'eslint-plugin-react-hooks'
// Import the React Refresh plugin — it checks that components can hot-reload properly during development
import reactRefresh from 'eslint-plugin-react-refresh'
// Import helper functions to define ESLint configuration and ignore certain folders
import { defineConfig, globalIgnores } from 'eslint/config'

// This is the main ESLint configuration — ESLint is a tool that checks our code for mistakes
export default defineConfig([
  // Tell ESLint to completely ignore the "dist" folder (that's the built/compiled output)
  globalIgnores(['dist']),
  {
    // Apply these rules to all JavaScript and JSX files in the project
    files: ['**/*.{js,jsx}'],
    // Use recommended rule sets from ESLint, React Hooks, and React Refresh plugins
    extends: [
      js.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    // Tell ESLint what kind of JavaScript we're writing
    languageOptions: {
      // We're using modern JavaScript features from ECMAScript 2020
      ecmaVersion: 2020,
      // Our code runs in a browser, so browser globals like "window" and "document" are allowed
      globals: globals.browser,
      parserOptions: {
        ecmaVersion: 'latest',
        // Enable JSX syntax (the HTML-like syntax used in React components)
        ecmaFeatures: { jsx: true },
        // We're using ES modules (import/export) instead of CommonJS (require)
        sourceType: 'module',
      },
    },
    // Custom rules that override the defaults
    rules: {
      // Show an error for unused variables, but ignore variables starting with uppercase or underscore
      // (React components start with uppercase, and "_" is often used for intentionally unused values)
      'no-unused-vars': ['error', { varsIgnorePattern: '^[A-Z_]' }],
    },
  },
])

/*
 * FILE SUMMARY:
 * This is the ESLint configuration file — ESLint is a code quality tool that scans your code
 * for common bugs, style issues, and React-specific mistakes (like breaking the Rules of Hooks).
 * It tells ESLint to check all .js and .jsx files, use modern JavaScript features, and ignore
 * the "dist" build folder. It also allows uppercase and underscore-prefixed variables to be
 * "unused" without triggering errors, which is important for React component imports.
 */

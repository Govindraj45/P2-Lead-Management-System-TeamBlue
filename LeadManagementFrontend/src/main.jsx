// Import StrictMode — a helper that warns us about potential problems during development
import { StrictMode } from 'react'
// Import createRoot — the function that connects React to the actual web page
import { createRoot } from 'react-dom/client'
// Import global CSS styles that apply to the entire application
import './index.css'
// Import our main App component — the starting point of the whole application
import App from './App.jsx'

// Find the HTML element with id="root" on the page (in index.html) and render our React app inside it
// StrictMode wraps the app to highlight potential issues (only active during development, not in production)
createRoot(document.getElementById('root')).render(
  <StrictMode>
    {/* This is where our entire application starts — everything flows from the App component */}
    <App />
  </StrictMode>,
)

/*
  FILE SUMMARY — main.jsx
  This is the very first JavaScript file that runs when the application starts.
  Its only job is to find the empty <div id="root"> element in the HTML page
  and inject the entire React application into it. Think of it as the "power button"
  that boots up the app. Without this file, nothing would appear on screen.
*/

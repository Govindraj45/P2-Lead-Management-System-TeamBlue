// Bring in axios, a tool that helps us talk to the backend server over the internet
import axios from 'axios';

// Create a reusable "api" helper with some default settings
// baseURL: all requests will start with '/api' (e.g., /api/leads, /api/auth/login)
// headers: tells the server we are sending JSON data
const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
});

// Before EVERY request goes out, this code runs automatically
// It grabs the login token from the browser's storage and attaches it to the request
// This is how the server knows WHO is making the request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// After EVERY response comes back, this code runs automatically
// If the response is successful, just pass it through
// If the server says "401 Unauthorized" (meaning the user's login expired or is invalid),
// clear the saved login info and send the user back to the login page
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Make this api helper available for other files to import and use
export default api;

/*
 * FILE SUMMARY:
 * This file sets up a central "api" helper using axios that every other service file uses
 * to talk to the backend server. It automatically attaches the user's login token to every
 * request so the server knows who is making it. If the server ever says the user is not
 * authorized (401), it logs them out and redirects to the login page. This is the foundation
 * of all network communication in the frontend app.
 */

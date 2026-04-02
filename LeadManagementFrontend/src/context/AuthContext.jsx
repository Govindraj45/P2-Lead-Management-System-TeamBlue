// Import React tools we need:
// createContext — lets us share data across components without passing props manually
// useContext — lets any component read the shared data
// useState — lets us store and update values (like the logged-in user)
// useEffect — lets us run code when the component first appears on screen
import { createContext, useContext, useState, useEffect } from 'react';
// Import the authService which talks to the backend server to log users in
import { authService } from '../services/authService';

// Create a "context" — a shared storage space that any component in the app can access
// We start with null (no user info yet)
const AuthContext = createContext(null);

// This function takes a JWT token (a long encoded string) and decodes it to read the data inside
// JWT tokens have 3 parts separated by dots — we need the middle part (the "payload")
function parseJwt(token) {
  try {
    // Get the middle section of the token (the payload with user info)
    const base64Url = token.split('.')[1];
    // Convert from Base64URL format to regular Base64 format
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    // Decode the Base64 string into readable JSON text
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    // Parse the JSON text into a JavaScript object and return it
    return JSON.parse(jsonPayload);
  } catch {
    // If anything goes wrong during decoding, return null (no data)
    return null;
  }
}

// This function extracts the specific user details we care about from the decoded token
function getUserFromToken(token) {
  // First, decode the token to get the raw payload
  const payload = parseJwt(token);
  // If decoding failed, return null (no user)
  if (!payload) return null;
  // Pull out the user's ID, role (like Admin or SalesRep), salesRepId, and email
  return {
    userId: payload.UserId || payload.sub,
    role: payload.Role || payload.role,
    salesRepId: payload.SalesRepId,
    email: payload.Email || payload.email,
  };
}

// The AuthProvider component wraps the entire app and provides login/logout functionality to all pages
export function AuthProvider({ children }) {
  // Store the current user — on first load, check if there's already a token saved in the browser
  const [user, setUser] = useState(() => {
    const token = localStorage.getItem('token');
    // If a token exists, decode it to get the user; otherwise, user is null (not logged in)
    return token ? getUserFromToken(token) : null;
  });
  // Track whether a login request is currently in progress
  const [loading, setLoading] = useState(false);
  // Store any error messages (like "wrong password")
  const [error, setError] = useState(null);

  // The login function — called when a user submits their email and password
  const login = async (email, password) => {
    // Show a loading indicator while we wait for the server
    setLoading(true);
    // Clear any previous error messages
    setError(null);
    try {
      // Send the email and password to the backend server
      const response = await authService.login({ email, password });
      // The server sends back a token — extract it from the response
      const { token } = response.data;
      // Save the token in the browser so the user stays logged in even after refreshing
      localStorage.setItem('token', token);
      // Decode the token to get user details (name, role, etc.)
      const userData = getUserFromToken(token);
      // Update the app's state with the logged-in user's info
      setUser(userData);
      return userData;
    } catch (err) {
      // If login fails, extract the error message from the server response
      const message = err.response?.data?.message || 'Login failed';
      // Save the error so we can display it on the login page
      setError(message);
      throw new Error(message);
    } finally {
      // Whether login succeeded or failed, stop the loading indicator
      setLoading(false);
    }
  };

  // The logout function — clears the saved token and resets the user to null
  const logout = () => {
    localStorage.removeItem('token');
    setUser(null);
  };

  // A helper that checks if the current user has one of the allowed roles
  // Example: hasRole('Admin', 'SalesManager') returns true if the user is Admin or SalesManager
  const hasRole = (...roles) => {
    return user && roles.includes(user.role);
  };

  // Bundle all the auth-related data and functions into one object
  const value = { user, loading, error, login, logout, hasRole };

  // Provide the auth data to every component inside this wrapper
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// A custom hook — a shortcut for any component to access the auth data (user, login, logout, etc.)
export function useAuth() {
  const context = useContext(AuthContext);
  // If someone tries to use this hook outside of AuthProvider, throw a helpful error
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

/*
  FILE SUMMARY — AuthContext.jsx
  This file manages everything related to user authentication (login, logout, and
  checking who is logged in). It stores the user's login token in the browser's
  localStorage so they stay logged in across page refreshes. Any component in the
  app can call useAuth() to access the current user, log in, log out, or check user
  roles. It's the security backbone of the frontend — without it, no page would know
  who the user is or whether they're allowed to see certain content.
*/

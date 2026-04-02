// Bring in our api helper that knows how to talk to the backend server
import api from './api';

// This object holds all login-related actions
export const authService = {
  // Send the user's username and password to the server to log them in
  // The server will send back a token if the credentials are correct
  login: (credentials) => api.post('/auth/login', credentials),
};

/*
 * FILE SUMMARY:
 * This file provides the login function for the app. When a user types their username
 * and password and clicks "Login", this service sends those details to the backend server.
 * The server checks if they are correct and sends back a special token that proves the
 * user is logged in. This is the only authentication action the frontend needs.
 */

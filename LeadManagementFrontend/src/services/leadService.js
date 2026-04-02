// Bring in our api helper that knows how to talk to the backend server
import api from './api';

// This object holds all the actions we can do with "leads" (potential customers)
export const leadService = {
  // Get a list of all leads, with optional filters like page number, status, etc.
  getAll: (params) => api.get('/leads', { params }),

  // Get the full details of one specific lead using its unique ID
  getById: (id) => api.get(`/leads/${id}`),

  // Create a brand new lead by sending its info (name, email, etc.) to the server
  create: (data) => api.post('/leads', data),

  // Update an existing lead's information (like changing their phone number or source)
  update: (id, data) => api.put(`/leads/${id}`, data),

  // Permanently delete a lead from the system
  delete: (id) => api.delete(`/leads/${id}`),

  // Change a lead's status (e.g., from "New" to "Contacted")
  changeStatus: (id, data) => api.put(`/leads/${id}/status`, { newStatus: data.status }),

  // Convert a qualified lead into a customer — this is a one-way action
  convert: (id) => api.post(`/leads/${id}/convert`),
};

/*
 * FILE SUMMARY:
 * This file handles all communication with the backend about "leads" — the potential
 * customers that sales reps are trying to win over. It can fetch leads, create new ones,
 * update existing ones, delete them, change their status, or convert them into customers.
 * Every page in the app that shows or edits lead data relies on this service file.
 */

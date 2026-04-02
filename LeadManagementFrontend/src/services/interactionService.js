// Bring in our api helper that knows how to talk to the backend server
import api from './api';

// This object holds all the actions we can do with "interactions"
// An interaction is a record of contact with a lead (like a phone call, email, or meeting)
export const interactionService = {
  // Get all past interactions for a specific lead, using the lead's ID
  getByLeadId: (leadId) => api.get(`/leads/${leadId}/interactions`),

  // Log a new interaction for a specific lead (e.g., "Called on March 5, discussed pricing")
  create: (leadId, data) => api.post(`/leads/${leadId}/interactions`, data),
};

/*
 * FILE SUMMARY:
 * This file manages "interactions" — the history of all contact between a sales rep and
 * a lead. Every time someone makes a phone call, sends an email, or has a meeting with
 * a potential customer, it gets recorded here. This service lets the app fetch those
 * records and create new ones, so the team can track their communication history.
 */

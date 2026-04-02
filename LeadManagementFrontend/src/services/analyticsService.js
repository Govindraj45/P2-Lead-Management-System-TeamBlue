// Bring in our api helper that knows how to talk to the backend server
import api from './api';

// This object holds all the actions for fetching dashboard/analytics data
// Analytics = numbers and stats that help managers see how the team is performing
export const analyticsService = {
  // Get a count of leads grouped by where they came from (Website, Referral, ColdCall, etc.)
  getBySource: () => api.get('/leads/analytics/by-source'),

  // Get the percentage of leads that have been successfully converted into customers
  getConversionRate: () => api.get('/leads/analytics/conversion-rate'),

  // Get a count of leads grouped by their current status (New, Contacted, Qualified, etc.)
  getByStatus: () => api.get('/leads/analytics/by-status'),

  // Get a count of leads grouped by which sales rep is handling them
  getBySalesRep: () => api.get('/leads/analytics/by-salesrep'),
};

/*
 * FILE SUMMARY:
 * This file fetches analytics and reporting data from the backend. It powers the dashboard
 * charts and statistics that managers use to understand team performance — like how many
 * leads came from each source, what percentage got converted, the status breakdown, and
 * how leads are distributed across sales reps. These are read-only data requests with no
 * create/update/delete actions.
 */

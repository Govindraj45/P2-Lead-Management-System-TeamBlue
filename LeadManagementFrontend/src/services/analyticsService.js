import api from './api';

export const analyticsService = {
  getBySource: () => api.get('/leads/analytics/by-source'),
  getConversionRate: () => api.get('/leads/analytics/conversion-rate'),
  getByStatus: () => api.get('/leads/analytics/by-status'),
  getBySalesRep: () => api.get('/leads/analytics/by-salesrep'),
};

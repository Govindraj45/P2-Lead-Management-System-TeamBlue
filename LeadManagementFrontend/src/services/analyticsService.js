import api from './api';

export const analyticsService = {
  getBySource: () => api.get('/reports/by-source'),
  getConversionRate: () => api.get('/reports/conversion-rate'),
  getByStatus: () => api.get('/reports/status-distribution'),
  getBySalesRep: () => api.get('/reports/by-salesrep'),
};

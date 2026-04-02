import api from './api';

export const interactionService = {
  getByLeadId: (leadId) => api.get(`/leads/${leadId}/interactions`),
  create: (leadId, data) => api.post(`/leads/${leadId}/interactions`, data),
};

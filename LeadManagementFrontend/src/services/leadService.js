import api from './api';

export const leadService = {
  getAll: (params) => api.get('/leads', { params }),
  getById: (id) => api.get(`/leads/${id}`),
  create: (data) => api.post('/leads', data),
  update: (id, data) => api.put(`/leads/${id}`, data),
  delete: (id) => api.delete(`/leads/${id}`),
  changeStatus: (id, data) => api.put(`/leads/${id}/status`, { newStatus: data.status }),
  convert: (id) => api.post(`/leads/${id}/convert`),
};

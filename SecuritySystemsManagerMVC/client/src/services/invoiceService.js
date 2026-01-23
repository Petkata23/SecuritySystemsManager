import api from '../config/api';

export const invoiceService = {
  getAll: async (pageSize = 10, pageNumber = 1) => {
    const response = await api.get('/api/invoiceapi', {
      params: { pageSize, pageNumber },
    });
    return response.data;
  },

  getFiltered: async (filters = {}, pageSize = 10, pageNumber = 1) => {
    const params = {
      pageSize,
      pageNumber,
      ...filters,
    };
    const response = await api.get('/api/invoiceapi/filtered', { params });
    return response.data;
  },

  getById: async (id) => {
    const response = await api.get(`/api/invoiceapi/${id}`);
    return response.data;
  },

  create: async (invoiceData) => {
    const response = await api.post('/api/invoiceapi', invoiceData);
    return response.data;
  },

  update: async (id, invoiceData) => {
    const response = await api.put(`/api/invoiceapi/${id}`, invoiceData);
    return response.data;
  },

  delete: async (id) => {
    const response = await api.delete(`/api/invoiceapi/${id}`);
    return response.data;
  },

  markAsPaid: async (id) => {
    const response = await api.post(`/api/invoiceapi/${id}/mark-paid`);
    return response.data;
  },

  markAsUnpaid: async (id) => {
    const response = await api.post(`/api/invoiceapi/${id}/mark-unpaid`);
    return response.data;
  },
};

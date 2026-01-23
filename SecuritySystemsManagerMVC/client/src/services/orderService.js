import api from '../config/api';

export const orderService = {
  getAll: async (pageSize = 10, pageNumber = 1) => {
    const response = await api.get('/api/orderapi', {
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
    const response = await api.get('/api/orderapi/filtered', { params });
    return response.data;
  },

  getById: async (id) => {
    const response = await api.get(`/api/orderapi/${id}`);
    return response.data;
  },

  create: async (orderData) => {
    const response = await api.post('/api/orderapi', orderData);
    return response.data;
  },

  update: async (id, orderData) => {
    const response = await api.put(`/api/orderapi/${id}`, orderData);
    return response.data;
  },

  delete: async (id) => {
    const response = await api.delete(`/api/orderapi/${id}`);
    return response.data;
  },
};

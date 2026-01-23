import api from '../config/api';

export const locationService = {
  getAll: async (pageSize = 10, pageNumber = 1) => {
    const response = await api.get('/api/locationapi', {
      params: { pageSize, pageNumber },
    });
    return response.data;
  },

  getById: async (id) => {
    const response = await api.get(`/api/locationapi/${id}`);
    return response.data;
  },

  getAllLocations: async () => {
    const response = await api.get('/api/locationapi/all');
    return response.data;
  },

  create: async (locationData) => {
    const response = await api.post('/api/locationapi/create', locationData);
    return response.data;
  },

  update: async (id, locationData) => {
    const response = await api.put(`/api/locationapi/${id}`, locationData);
    return response.data;
  },

  delete: async (id) => {
    const response = await api.delete(`/api/locationapi/${id}`);
    return response.data;
  },
};

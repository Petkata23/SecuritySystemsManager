import api from '../config/api';

export const authService = {
  login: async (username, password) => {
    try {
      console.log('Attempting login for:', username);
      console.log('API base URL:', api.defaults.baseURL);
      console.log('Full URL will be:', `${api.defaults.baseURL}/api/authapi/login`);
      
      const response = await api.post('/api/authapi/login', { username, password });
      console.log('Login response status:', response.status);
      console.log('Login response data:', response.data);
      console.log('Login response data type:', typeof response.data);
      console.log('Login response.data.token:', response.data?.token);
      console.log('Login response.data.user:', response.data?.user);
      
      if (response.data && response.data.token) {
        localStorage.setItem('token', response.data.token);
        if (response.data.user) {
          localStorage.setItem('user', JSON.stringify(response.data.user));
          console.log('Token and user saved to localStorage');
        } else {
          console.warn('No user data in response');
        }
      } else {
        console.error('No token in response:', response.data);
      }
      return response.data;
    } catch (error) {
      console.error('Login error:', error);
      console.error('Error response:', error.response?.data);
      console.error('Error status:', error.response?.status);
      console.error('Error config:', error.config);
      throw error;
    }
  },

  register: async (userData) => {
      const response = await api.post('/api/authapi/register', userData);
    if (response.data.token) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data.user));
    }
    return response.data;
  },

  logout: async () => {
    try {
      await api.post('/api/authapi/logout');
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
  },

  getCurrentUser: async () => {
      const response = await api.get('/api/authapi/me');
    return response.data;
  },

  getStoredUser: () => {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },

  getToken: () => {
    return localStorage.getItem('token');
  },

  isAuthenticated: () => {
    return !!localStorage.getItem('token');
  },
};

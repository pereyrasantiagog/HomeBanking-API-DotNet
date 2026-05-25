import axios from 'axios';

const api = axios.create({
  baseURL: 'http://127.0.0.1:5284/api',
});

// Interceptor de peticiones para agregar el token JWT automáticamente
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export default api;

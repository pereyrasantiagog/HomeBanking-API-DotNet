import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5284/api', // Ajustar el puerto si el backend .NET usa uno diferente (ej. 5217 o 7000)
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

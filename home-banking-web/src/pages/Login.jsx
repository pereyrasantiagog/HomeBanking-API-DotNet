import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import api from '../api/axios';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errorMessage, setErrorMessage] = useState(''); // Estado para mostrar errores en pantalla
  const navigate = useNavigate();

  const handleSubmit = async (e) => { // Convertimos la función en asíncrona
    e.preventDefault();
    setErrorMessage(''); // Limpiamos el error si el usuario vuelve a intentar

    try {
      // 1. Le pegamos a la API de .NET
      const response = await api.post('/Auth/Login', { 
        email: email, 
        password: password 
      });

      // 2. Si sale bien, capturamos el Token JWT que nos devuelve el backend
      // (Asumiendo que tu DTO de respuesta tiene una propiedad 'token')
      const token = response.data.token; 
      
      // 3. Guardamos el token en la memoria del navegador (localStorage)
      localStorage.setItem('token', token);

      // 4. Lo mandamos directo a la pantalla de saldos
      navigate('/dashboard');

    } catch (error) {
      console.error('Error al iniciar sesión:', error);
      // Si el backend nos tira un 401 (No autorizado) o 404, caemos acá
      setErrorMessage('Credenciales incorrectas o problema de conexión.');
    }
  };

  return (
    <div className="login-container">
      <form className="login-form" onSubmit={handleSubmit}>
        <h2>Iniciar Sesión</h2>
        
        {/* Renderizado condicional: Si hay un error, mostramos este mensaje rojo */}
        {errorMessage && (
          <div style={{ color: 'red', backgroundColor: '#fee', padding: '10px', borderRadius: '5px', marginBottom: '15px', fontSize: '14px' }}>
            {errorMessage}
          </div>
        )}

        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input
            type="email"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            placeholder="correo@ejemplo.com"
          />
        </div>

        <div className="form-group">
          <label htmlFor="password">Contraseña</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            placeholder="********"
          />
        </div>

        <button type="submit" className="btn-primary">Ingresar</button>
        
        <p className="auth-link">
          ¿No tienes una cuenta? <Link to="/register">Regístrate aquí</Link>
        </p>
      </form>
    </div>
  );
};

export default Login;
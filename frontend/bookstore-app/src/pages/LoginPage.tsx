import { useState } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { Icon } from '../components/Icon';

const LoginPage = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await login(username, password);
      navigate('/', { replace: true });
    } catch {
      setError('Usuario o contraseña incorrectos');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-container">
        <div className="login-card">
          {/* Header */}
          <div className="login-header">
            <div className="login-logo">
              <Icon name="fa-solid fa-book-open" />
            </div>
            <h1>Ediciones Liber</h1>
            <p>Ingresá tus credenciales para acceder al sistema</p>
          </div>

          {/* Error Message */}
          {error && (
            <div className="login-error">
              <Icon name="fa-solid fa-circle-exclamation" />
              <span>{error}</span>
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit}>
            <div className="login-field">
              <label htmlFor="username">Usuario</label>
              <div className="login-input-wrapper">
                <Icon name="fa-solid fa-user" />
                <input
                  id="username"
                  type="text"
                  placeholder="Ingresá tu usuario"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  required
                  autoFocus
                  autoComplete="username"
                />
              </div>
            </div>

            <div className="login-field">
              <label htmlFor="password">Contraseña</label>
              <div className="login-input-wrapper">
                <Icon name="fa-solid fa-lock" />
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  placeholder="Ingresá tu contraseña"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  autoComplete="current-password"
                />
                <button
                  type="button"
                  className="login-toggle-password"
                  onClick={() => setShowPassword(!showPassword)}
                  tabIndex={-1}
                >
                  <Icon name={showPassword ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye'} />
                </button>
              </div>
            </div>

            <button
              type="submit"
              className="login-submit"
              disabled={loading || !username || !password}
            >
              {loading ? (
                <>
                  <div className="spinner-gradient" style={{ width: 20, height: 20, borderWidth: 2, borderTopColor: 'white' }} />
                  <span>Ingresando...</span>
                </>
              ) : (
                <>
                  <Icon name="fa-solid fa-right-to-bracket" />
                  <span>Iniciar Sesión</span>
                </>
              )}
            </button>
          </form>
        </div>

        <div className="login-footer">
          Sistema de Gestión - Ediciones Liber
        </div>
      </div>
    </div>
  );
};

export default LoginPage;

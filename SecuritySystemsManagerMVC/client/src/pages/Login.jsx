import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import '../styles/forms-mobile.css';

export default function Login() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  // Test API connection on mount
  useEffect(() => {
    const testConnection = async () => {
      try {
        const response = await fetch('/api/authapi/login', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ username: 'test', password: 'test' })
        });
        console.log('API Connection test - Status:', response.status);
      } catch (err) {
        console.error('API Connection test failed:', err);
      }
    };
    // testConnection(); // Uncomment to test
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    console.log('Login form submitted for:', username);
    
    try {
      const result = await login(username, password);
      console.log('Login result:', result);
      console.log('Login result.success:', result.success);
      console.log('Login result.user:', result.user);
      
      if (result.success) {
        console.log('Login successful, navigating to home');
        console.log('Current user state after login:', result.user);
        // Wait a moment for state to update, then navigate
        setTimeout(() => {
          console.log('Navigating to home page...');
          navigate('/', { replace: true });
        }, 300);
      } else {
        console.error('Login failed:', result.message);
        setError(result.message || 'Login failed');
        setLoading(false);
      }
    } catch (err) {
      console.error('Login exception:', err);
      console.error('Login exception details:', {
        message: err.message,
        response: err.response,
        stack: err.stack,
      });
      setError(err.message || 'An unexpected error occurred');
      setLoading(false);
    }
  };

  return (
    <div className="container-fluid" style={{ paddingTop: '2rem', paddingBottom: '2rem' }}>
      <div className="row justify-content-center">
        <div className="col-md-6 col-lg-4">
          <div className="card" style={{ background: 'var(--card-bg)', border: '1px solid var(--border-color)' }}>
            <div className="card-body p-4">
        <h2>Вход</h2>
        {error && (
          <div className="error-message">
            {error}
            <br />
            <small>Моля, проверете конзолата за повече детайли</small>
          </div>
        )}
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">Потребителско име</label>
            <input
              type="text"
              id="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              disabled={loading}
              autoComplete="username"
            />
          </div>
          <div className="form-group">
            <label htmlFor="password">Парола</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              disabled={loading}
              autoComplete="current-password"
            />
          </div>
          <button type="submit" disabled={loading} className="submit-button">
            {loading ? 'Влизане...' : 'Влез'}
          </button>
        </form>
        <p className="register-link">
          Нямате акаунт? <Link to="/register">Регистрирайте се</Link>
        </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

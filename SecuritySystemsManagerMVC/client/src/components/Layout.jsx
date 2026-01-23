import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import PageTitle from './PageTitle';
import './Layout.css';

export default function Layout({ children }) {
  const { user, logout, isAuthenticated, loading } = useAuth();
  const location = useLocation();

  // Debug logging
  useEffect(() => {
    console.log('Layout - User state:', { user, isAuthenticated, loading });
  }, [user, isAuthenticated, loading]);

  useEffect(() => {
    // Initialize Bootstrap dropdowns
    const initDropdowns = () => {
      // Use dynamic import for Bootstrap
      import('bootstrap').then((bootstrapModule) => {
        const bootstrap = bootstrapModule.default || bootstrapModule;
        
        // Initialize all dropdowns
        document.querySelectorAll('.dropdown-toggle').forEach((element) => {
          try {
            // Check if already initialized
            if (!bootstrap.Dropdown.getInstance(element)) {
              new bootstrap.Dropdown(element);
            }
          } catch (e) {
            console.warn('Dropdown init error:', e);
          }
        });
      }).catch((err) => {
        console.error('Bootstrap import error:', err);
      });
    };

    // Small delay to ensure DOM is ready
    const timer = setTimeout(initDropdowns, 100);
    return () => clearTimeout(timer);
  }, [location.pathname]);

  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login', { replace: true });
  };

  const isHomePage = location.pathname === '/';

  return (
    <div className="app-container">
      {/* Top Bar */}
      <div className="top-bar d-none d-md-block">
        <div className="container-fluid">
          <div className="row align-items-center">
            <div className="col-md-8">
              <ul className="list-inline top-bar-info mb-0">
                <li className="list-inline-item">
                  <i className="bi bi-envelope"></i> info@securityhristovi.com
                </li>
                <li className="list-inline-item">
                  <i className="bi bi-phone"></i> +359 888 123 456
                </li>
                <li className="list-inline-item">
                  <i className="bi bi-clock"></i> Пн-Пт: 9:00 - 18:00
                </li>
              </ul>
            </div>
            <div className="col-md-4">
              <ul className="list-inline top-bar-social mb-0 text-end">
                <li className="list-inline-item">
                  <a href="#"><i className="bi bi-facebook"></i></a>
                </li>
                <li className="list-inline-item">
                  <a href="#"><i className="bi bi-instagram"></i></a>
                </li>
                <li className="list-inline-item">
                  <a href="#"><i className="bi bi-linkedin"></i></a>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>

      {/* Main Header */}
      <header className="main-header">
        <nav className="navbar navbar-expand-lg">
          <div className="container-fluid">
            <Link className="navbar-brand" to="/">
              <i className="bi bi-shield-lock logo-icon"></i>
              <span className="logo-text">Security Systems Hristovi</span>
            </Link>
            <button
              className="navbar-toggler"
              type="button"
              data-bs-toggle="collapse"
              data-bs-target="#navbarNav"
              aria-controls="navbarNav"
              aria-expanded="false"
              aria-label="Toggle navigation"
            >
              <span className="navbar-toggler-icon"></span>
            </button>
            <div className="collapse navbar-collapse" id="navbarNav">
              <ul className="navbar-nav w-100 justify-content-between">
                <li className="nav-item">
                  <Link
                    className={`nav-link ${location.pathname === '/' ? 'active' : ''}`}
                    to="/"
                  >
                    <i className="bi bi-house-door me-2"></i>Начало
                  </Link>
                </li>

                {isAuthenticated && (
                  <>
                    <li className="nav-item dropdown">
                      <a
                        className="nav-link dropdown-toggle"
                        href="#"
                        id="businessOperationsDropdown"
                        role="button"
                        data-bs-toggle="dropdown"
                        aria-expanded="false"
                      >
                        <i className="bi bi-briefcase me-2"></i> Поръчки
                      </a>
                      <ul className="dropdown-menu" aria-labelledby="businessOperationsDropdown">
                        <li>
                          <Link className="dropdown-item" to="/orders">
                            <i className="bi bi-cart me-2"></i>Поръчки
                          </Link>
                        </li>
                        <li>
                          <Link className="dropdown-item" to="/invoices">
                            <i className="bi bi-receipt me-2"></i>Фактури
                          </Link>
                        </li>
                        <li>
                          <Link className="dropdown-item" to="/maintenance">
                            <i className="bi bi-tools me-2"></i>Поддръжка
                          </Link>
                        </li>
                      </ul>
                    </li>
                    <li className="nav-item">
                      <Link className="nav-link" to="/locations">
                        <i className="bi bi-geo-alt me-2"></i>Моите локации
                      </Link>
                    </li>
                  </>
                )}

                {user?.roles?.includes('Admin') && (
                  <li className="nav-item dropdown">
                    <a
                      className="nav-link dropdown-toggle"
                      href="#"
                      id="administrationDropdown"
                      role="button"
                      data-bs-toggle="dropdown"
                      aria-expanded="false"
                    >
                      <i className="bi bi-gear me-2"></i>Администрация
                    </a>
                    <ul className="dropdown-menu" aria-labelledby="administrationDropdown">
                      <li>
                        <Link className="dropdown-item" to="/users">
                          <i className="bi bi-people me-2"></i>Потребители
                        </Link>
                      </li>
                      <li>
                        <Link className="dropdown-item" to="/roles">
                          <i className="bi bi-person-badge me-2"></i>Роли
                        </Link>
                      </li>
                    </ul>
                  </li>
                )}

                <li className="nav-item d-flex align-items-center d-none d-lg-flex">
                  {isAuthenticated ? (
                    <>
                      <div className="nav-divider-vertical d-none d-lg-block"></div>
                      <div className="user-profile-dropdown">
                        <a
                          className="nav-link dropdown-toggle"
                          href="#"
                          id="userProfileDropdown"
                          role="button"
                          data-bs-toggle="dropdown"
                          aria-expanded="false"
                        >
                          {user?.profileImage ? (
                            <img
                              src={user.profileImage}
                              alt={user.username}
                              className="user-avatar-small"
                            />
                          ) : (
                            <i className="bi bi-person-circle"></i>
                          )}
                          <span className="ms-2">{user?.firstName || user?.username}</span>
                        </a>
                        <ul className="dropdown-menu dropdown-menu-end" aria-labelledby="userProfileDropdown">
                          <li>
                            <Link className="dropdown-item" to="/account">
                              <i className="bi bi-person me-2"></i>Моят профил
                            </Link>
                          </li>
                          <li>
                            <hr className="dropdown-divider" />
                          </li>
                          <li>
                            <button className="dropdown-item" onClick={handleLogout}>
                              <i className="bi bi-box-arrow-right me-2"></i>Изход
                            </button>
                          </li>
                        </ul>
                      </div>
                    </>
                  ) : (
                    <div className="auth-buttons">
                      <Link className="btn btn-outline-light btn-sm me-2" to="/login">
                        Вход
                      </Link>
                      <Link className="btn btn-primary btn-sm" to="/register">
                        Регистрация
                      </Link>
                    </div>
                  )}
                </li>
              </ul>
            </div>
          </div>
        </nav>
      </header>

      {/* Page Title Section */}
      <PageTitle />

      {/* Main Content */}
      <main className={`main-content ${isHomePage ? 'home-main-content' : ''}`}>
        <div className={isHomePage ? '' : 'container-fluid'}>
          {children}
        </div>
      </main>

      {/* Footer */}
      <footer className="footer">
        <div className="footer-bottom text-center">
          <div className="container-fluid">
            <div className="row">
              <div className="col-12">
                <div className="copyright">
                  &copy; {new Date().getFullYear()}{' '}
                  <strong>
                    <span>Security Systems Hristovi</span>
                  </strong>
                  . Всички права запазени
                  <div className="mt-2">
                    <Link to="/privacy" className="text-muted text-decoration-none">
                      Политика за поверителност
                    </Link>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}

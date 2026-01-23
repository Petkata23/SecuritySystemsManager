import { Link, useNavigate } from 'react-router-dom';
import { useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';

export default function Home() {
  const { isAuthenticated, user, loading } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    console.log('Home component - Auth state:', { isAuthenticated, user, loading });
  }, [isAuthenticated, user, loading]);

  // Show welcome message if authenticated
  if (isAuthenticated && user) {
    return (
      <>
        <div className="container-fluid" style={{ padding: '2rem' }}>
          <div className="alert alert-success" role="alert">
            <h4 className="alert-heading">
              <i className="bi bi-check-circle me-2"></i>Добре дошли, {user.firstName || user.username}!
            </h4>
            <p>Успешно влязохте в системата. Можете да започнете да използвате всички функции.</p>
            <hr />
            <div className="d-flex gap-2">
              <Link to="/orders" className="btn btn-primary">
                <i className="bi bi-cart me-2"></i>Моите поръчки
              </Link>
              <Link to="/locations" className="btn btn-outline-primary">
                <i className="bi bi-geo-alt me-2"></i>Моите локации
              </Link>
              <Link to="/invoices" className="btn btn-outline-primary">
                <i className="bi bi-receipt me-2"></i>Фактури
              </Link>
            </div>
          </div>
        </div>
        {/* Original Home content */}
        <section className="hero-section">
          <div className="container-fluid">
            <div className="row align-items-center">
              <div className="col-lg-6">
                <h1 className="hero-title">Модерни системи за сигурност за съвременна защита</h1>
                <p className="hero-text">
                  Security Systems Hristovi предлага иновативни решения за сигурност за бизнеси и домове. 
                  Нашите интегрирани системи осигуряват пълна защита и спокойствие.
                </p>
                <div className="hero-buttons">
                  <a href="#services" className="btn btn-primary btn-lg">
                    Разгледай решения
                  </a>
                  <Link to="/orders" className="btn btn-outline-secondary btn-lg">
                    Моите поръчки
                  </Link>
                </div>
              </div>
              <div className="col-lg-6">
                <div className="hero-image-container">
                  <img
                    src="/img/security-pattern.svg"
                    alt="Security Systems"
                    className="img-fluid hero-image"
                    onError={(e) => {
                      e.target.style.display = 'none';
                    }}
                  />
                  <div className="hero-shape-1"></div>
                  <div className="hero-shape-2"></div>
                </div>
              </div>
            </div>
          </div>
        </section>
        {/* Rest of home content */}
      </>
    );
  }

  return (
    <>
      {/* Hero Section */}
      <section className="hero-section">
        <div className="container-fluid">
          <div className="row align-items-center">
            <div className="col-lg-6">
              <h1 className="hero-title">Модерни системи за сигурност за съвременна защита</h1>
              <p className="hero-text">
                Security Systems Hristovi предлага иновативни решения за сигурност за бизнеси и домове. 
                Нашите интегрирани системи осигуряват пълна защита и спокойствие.
              </p>
              <div className="hero-buttons">
                <a href="#services" className="btn btn-primary btn-lg">
                  Разгледай решения
                </a>
                {isAuthenticated ? (
                  <Link to="/orders" className="btn btn-outline-secondary btn-lg">
                    Моите поръчки
                  </Link>
                ) : (
                  <Link to="/login" className="btn btn-outline-secondary btn-lg">
                    Свържи се с нас
                  </Link>
                )}
              </div>
              <div className="hero-stats mt-5">
                <div className="row">
                  <div className="col-4">
                    <div className="stat-item">
                      <div className="stat-number">15+</div>
                      <div className="stat-label">Години опит</div>
                    </div>
                  </div>
                  <div className="col-4">
                    <div className="stat-item">
                      <div className="stat-number">5000+</div>
                      <div className="stat-label">Инсталации</div>
                    </div>
                  </div>
                  <div className="col-4">
                    <div className="stat-item">
                      <div className="stat-number">24/7</div>
                      <div className="stat-label">Поддръжка</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div className="col-lg-6">
              <div className="hero-image-container">
                  <img
                  src="/img/security-pattern.svg"
                  alt="Security Systems"
                  className="img-fluid hero-image"
                  onError={(e) => {
                    e.target.style.display = 'none';
                  }}
                />
                <div className="hero-shape-1"></div>
                <div className="hero-shape-2"></div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="features-section" id="features">
        <div className="container-fluid">
          <div className="section-header text-center">
            <h2>Защо да ни изберете</h2>
            <p>Предлагаме цялостни решения за сигурност с модерна технология и експертен сервиз</p>
          </div>
          <div className="row">
            <div className="col-md-4 mb-4">
              <div className="feature-card">
                <div className="feature-icon">
                  <i className="bi bi-shield-check"></i>
                </div>
                <h3>Модерна технология</h3>
                <p>
                  Нашите системи използват най-новите технологии за сигурност с аналитика, задвижвана от 
                  изкуствен интелект, и интелигентни възможности за откриване.
                </p>
              </div>
            </div>
            <div className="col-md-4 mb-4">
              <div className="feature-card">
                <div className="feature-icon">
                  <i className="bi bi-phone"></i>
                </div>
                <h3>Дистанционно наблюдение</h3>
                <p>
                  Контролирайте и наблюдавайте вашите системи за сигурност от всяко място чрез нашето 
                  мобилно приложение с известия в реално време.
                </p>
              </div>
            </div>
            <div className="col-md-4 mb-4">
              <div className="feature-card">
                <div className="feature-icon">
                  <i className="bi bi-headset"></i>
                </div>
                <h3>Експертна поддръжка</h3>
                <p>
                  Нашият професионален екип осигурява техническа поддръжка 24/7 и бърза реакция на всички 
                  проблеми със сигурността.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Services Section */}
      <section className="services-section" id="services">
        <div className="container-fluid">
          <div className="section-header text-center">
            <h2>Нашите решения за сигурност</h2>
            <p>Цялостна защита за жилищни и търговски имоти</p>
          </div>
          <div className="row">
            <div className="col-lg-4 col-md-6 mb-4">
              <div className="service-card">
                <div className="service-image">
                  <img src="/img/cctv.jpg" alt="CCTV Systems" className="img-fluid" onError={(e) => e.target.style.display = 'none'} />
                  <div className="service-overlay">
                    <i className="bi bi-camera-video"></i>
                  </div>
                </div>
                <div className="service-content">
                  <h3>Видеонаблюдение</h3>
                  <p>
                    Камери за наблюдение с висока резолюция с напреднали възможности за откриване на 
                    движение и нощно виждане.
                  </p>
                </div>
              </div>
            </div>
            <div className="col-lg-4 col-md-6 mb-4">
              <div className="service-card">
                <div className="service-image">
                  <img src="/img/alarm_system.jpg" alt="Alarm Systems" className="img-fluid" onError={(e) => e.target.style.display = 'none'} />
                  <div className="service-overlay">
                    <i className="bi bi-bell"></i>
                  </div>
                </div>
                <div className="service-content">
                  <h3>Алармни системи</h3>
                  <p>
                    Напреднало откриване на проникване с незабавни известия и интегрирани протоколи за реакция.
                  </p>
                </div>
              </div>
            </div>
            <div className="col-lg-4 col-md-6 mb-4">
              <div className="service-card">
                <div className="service-image">
                  <img src="/img/access_control.jpg" alt="Access Control" className="img-fluid" onError={(e) => e.target.style.display = 'none'} />
                  <div className="service-overlay">
                    <i className="bi bi-fingerprint"></i>
                  </div>
                </div>
                <div className="service-content">
                  <h3>Контрол на достъпа</h3>
                  <p>
                    Сигурни системи за вход с биометрично удостоверяване и мобилно управление на достъпа.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="cta-section">
        <div className="container-fluid">
          <div className="cta-content">
            <h2 className="cta-title">Готови ли сте да подобрите сигурността си?</h2>
            <p className="cta-text">
              Свържете се с нас днес за безплатна консултация и персонализирана оценка на сигурността.
            </p>
            <div className="cta-buttons">
              {isAuthenticated ? (
                <Link to="/orders" className="btn btn-cta-primary">
                  Моите поръчки
                </Link>
              ) : (
                <Link to="/login" className="btn btn-cta-primary">
                  Започни
                </Link>
              )}
              <a href="#services" className="btn btn-cta-secondary">
                Научи повече
              </a>
            </div>
          </div>
        </div>
      </section>
    </>
  );
}

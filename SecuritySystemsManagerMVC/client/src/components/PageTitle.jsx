import { Link, useLocation } from 'react-router-dom';

// Map routes to page titles and breadcrumb names (exactly as in original)
const routeMap = {
  '/': { title: 'Начало', breadcrumb: null, controller: 'Home' },
  '/orders': { title: 'Поръчки', breadcrumb: 'Поръчки', controller: 'SecuritySystemOrder' },
  '/invoices': { title: 'Фактури', breadcrumb: 'Фактури', controller: 'Invoice' },
  '/maintenance': { title: 'Поддръжка', breadcrumb: 'Поддръжка', controller: 'MaintenanceLog' },
  '/locations': { title: 'Локации', breadcrumb: 'Локации', controller: 'Location' },
  '/users': { title: 'Потребители', breadcrumb: 'Потребители', controller: 'User' },
  '/roles': { title: 'Роли', breadcrumb: 'Роли', controller: 'Role' },
  '/account': { title: 'Профил', breadcrumb: 'Профил', controller: 'Account' },
  '/login': { title: 'Вход', breadcrumb: null, controller: 'Auth' },
  '/register': { title: 'Регистрация', breadcrumb: null, controller: 'Auth' },
  '/privacy': { title: 'Политика за поверителност', breadcrumb: null, controller: null },
};

export default function PageTitle({ title: customTitle }) {
  const location = useLocation();
  const isHomePage = location.pathname === '/';
  
  // Don't show page title section on home page (exactly as in original)
  if (isHomePage) {
    return null;
  }

  // Get title from custom prop or route map
  const pageInfo = routeMap[location.pathname] || { 
    title: customTitle || 'Страница', 
    breadcrumb: null, 
    controller: null 
  };
  const pageTitle = customTitle || pageInfo.title;
  const breadcrumbName = pageInfo.breadcrumb || pageTitle;
  const controllerName = pageInfo.controller;

  // Determine breadcrumb structure (exactly as in original)
  // For list/index pages, show only the controller name
  // For other pages (details, edit, create), show controller link + current page
  const isListPage = location.pathname === '/orders' || 
                     location.pathname === '/invoices' || 
                     location.pathname === '/maintenance' ||
                     location.pathname === '/locations' ||
                     location.pathname === '/users' ||
                     location.pathname === '/roles';

  return (
    <div className="page-title-section">
      <div className="container-fluid">
        <div className="row">
          <div className="col-12">
            <h1 className="page-title">{pageTitle}</h1>
            {controllerName && (
              <nav aria-label="breadcrumb">
                <ol className="breadcrumb">
                  <li className="breadcrumb-item">
                    <Link to="/">
                      <i className="bi bi-house-door me-1"></i>Начало
                    </Link>
                  </li>
                  {isListPage ? (
                    // For list pages, just show the controller name as active
                    <li className="breadcrumb-item active" aria-current="page">
                      {breadcrumbName}
                    </li>
                  ) : (
                    // For other pages, show controller link + current page
                    <>
                      <li className="breadcrumb-item">
                        <Link to={getListRoute(location.pathname)}>
                          {breadcrumbName}
                        </Link>
                      </li>
                      <li className="breadcrumb-item active" aria-current="page">
                        {pageTitle}
                      </li>
                    </>
                  )}
                </ol>
              </nav>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

// Helper function to get list route from current path
function getListRoute(pathname) {
  const routeMap = {
    '/orders': '/orders',
    '/invoices': '/invoices',
    '/maintenance': '/maintenance',
    '/locations': '/locations',
    '/users': '/users',
    '/roles': '/roles',
  };
  
  // Extract base route (e.g., '/orders/123' -> '/orders')
  const baseRoute = pathname.split('/').slice(0, 2).join('/');
  return routeMap[baseRoute] || '/';
}

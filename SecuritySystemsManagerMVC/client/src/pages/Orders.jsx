import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { orderService } from '../services/orderService';
import '../styles/orders-list.css';

export default function Orders() {
  const { user } = useAuth();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState({
    searchTerm: '',
    startDate: '',
    endDate: '',
    status: '',
  });
  const [showFilters, setShowFilters] = useState(false);
  const [viewMode, setViewMode] = useState('table');
  const [pagination, setPagination] = useState({
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 1,
  });

  const isClient = user?.roles?.includes('Client');
  const isAdminOrManager = user?.roles?.includes('Admin') || user?.roles?.includes('Manager');
  const isTechnician = user?.roles?.includes('Technician');
  const canCreateOrder = !isTechnician;

  useEffect(() => {
    loadOrders();
  }, [pagination.pageNumber, filters]);

  const loadOrders = async () => {
    try {
      setLoading(true);
      console.log('Loading orders with filters:', filters);
      console.log('Pagination:', pagination);
      
      const hasFilters = filters.searchTerm || filters.startDate || filters.endDate || filters.status;
      const response = hasFilters
        ? await orderService.getFiltered(filters, pagination.pageSize, pagination.pageNumber)
        : await orderService.getAll(pagination.pageSize, pagination.pageNumber);

      console.log('Orders response:', response);
      console.log('Orders response.data:', response.data);
      console.log('Orders response.pagination:', response.pagination);

      const ordersData = Array.isArray(response.data) ? response.data : (Array.isArray(response) ? response : []);
      setOrders(ordersData);
      
      if (response.pagination) {
        setPagination((prev) => ({ ...prev, ...response.pagination }));
      } else if (response.totalCount !== undefined) {
        // Fallback if pagination is at root level
        setPagination((prev) => ({
          ...prev,
          totalCount: response.totalCount,
          totalPages: response.totalPages || Math.ceil(response.totalCount / pagination.pageSize),
        }));
      }
      
      console.log('Orders loaded:', ordersData.length);
    } catch (error) {
      console.error('Error loading orders:', error);
      console.error('Error details:', {
        message: error.message,
        response: error.response?.data,
        status: error.response?.status,
      });
      setOrders([]);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (key, value) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
    setPagination((prev) => ({ ...prev, pageNumber: 1 }));
  };

  const handleSearch = () => {
    loadOrders();
  };

  const getStatusBadgeClass = (status) => {
    const statusMap = {
      Pending: 'warning',
      InProgress: 'primary',
      Completed: 'success',
      Cancelled: 'danger',
    };
    return statusMap[status] || 'secondary';
  };

  const getStatusDisplayName = (status) => {
    return status === 'InProgress' ? 'In Progress' : status;
  };

  const getStatusIcon = (status) => {
    const iconMap = {
      Pending: 'bi-hourglass-split',
      InProgress: 'bi-gear-wide-connected',
      Completed: 'bi-check-circle',
      Cancelled: 'bi-x-circle',
    };
    return iconMap[status] || 'bi-question-circle';
  };

  const formatDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('bg-BG', { year: 'numeric', month: 'short', day: 'numeric' });
  };

  const stats = {
    total: orders.length,
    pending: orders.filter((o) => o.status === 'Pending').length,
    inProgress: orders.filter((o) => o.status === 'InProgress').length,
    completed: orders.filter((o) => o.status === 'Completed').length,
    cancelled: orders.filter((o) => o.status === 'Cancelled').length,
  };

  if (loading) {
    return (
      <div className="orders-container" style={{ padding: '2rem' }}>
        <div className="text-center">
          <div className="spinner-border" role="status">
            <span className="visually-hidden">Зареждане...</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="orders-container">
      {/* Enhanced Header Banner */}
      <div className="orders-banner">
        <div className="banner-content">
          <div className="banner-text">
            <h1 className="banner-title">Поръчки за системи за сигурност</h1>
            <p className="banner-description">
              {orders.length > 0
                ? 'Управлявай и проследявай всички поръчки за инсталации и поддръжка на системи за сигурност.'
                : 'Започни да защитаваш имотите си, като създадеш първата си поръчка за система за сигурност.'}
            </p>
          </div>
          <div className="banner-actions">
            {canCreateOrder && (
              <Link to="/orders/create" className="btn btn-primary btn-create-banner">
                <i className="bi bi-plus-lg me-2"></i>Нова поръчка
              </Link>
            )}
            {orders.length > 0 && (
              <div className="order-counts">
                <div className="count-item">
                  <span className="count-value">{stats.total}</span>
                  <span className="count-label">Total</span>
                </div>
                <div className="count-divider"></div>
                <div className={`count-item ${stats.pending > 0 ? 'has-count' : ''}`}>
                  <span className="count-value">{stats.pending}</span>
                  <span className="count-label">Pending</span>
                </div>
                <div className="count-divider"></div>
                <div className={`count-item ${stats.inProgress > 0 ? 'has-count' : ''}`}>
                  <span className="count-value">{stats.inProgress}</span>
                  <span className="count-label">In Progress</span>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {orders.length > 0 ? (
        <>
          {/* Filter Card */}
          <div className="card filter-card">
            <div className="card-header d-flex justify-content-between align-items-center">
              <h5>
                <i className="bi bi-funnel"></i> Filter Orders
              </h5>
              <button
                className="btn btn-sm btn-outline-secondary btn-filter-toggle"
                type="button"
                onClick={() => setShowFilters(!showFilters)}
              >
                <i className="bi bi-sliders me-1"></i>Toggle Filters
              </button>
            </div>
            {showFilters && (
              <div className="card-body">
                <form className="row g-3">
                  <div className="col-md-4">
                    <label className="form-label">Status</label>
                    <select
                      className="form-select"
                      value={filters.status}
                      onChange={(e) => handleFilterChange('status', e.target.value)}
                    >
                      <option value="">All Statuses</option>
                      <option value="Pending">Pending</option>
                      <option value="InProgress">In Progress</option>
                      <option value="Completed">Completed</option>
                      <option value="Cancelled">Cancelled</option>
                    </select>
                  </div>
                  <div className="col-md-4">
                    <label className="form-label">Date Range</label>
                    <div className="input-group">
                      <input
                        type="date"
                        className="form-control"
                        value={filters.startDate}
                        onChange={(e) => handleFilterChange('startDate', e.target.value)}
                      />
                      <span className="input-group-text">to</span>
                      <input
                        type="date"
                        className="form-control"
                        value={filters.endDate}
                        onChange={(e) => handleFilterChange('endDate', e.target.value)}
                      />
                    </div>
                  </div>
                  <div className="col-md-4">
                    <label className="form-label">Search</label>
                    <div className="input-group">
                      <input
                        type="text"
                        className="form-control"
                        placeholder="Search orders..."
                        value={filters.searchTerm}
                        onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                      />
                      <button className="btn btn-primary" type="button" onClick={handleSearch}>
                        <i className="bi bi-search"></i>
                      </button>
                    </div>
                  </div>
                </form>
              </div>
            )}
          </div>

          {/* Orders Table Card */}
          <div className="card orders-card">
            <div className="card-header">
              <div className="d-flex justify-content-between align-items-center">
                <h5>
                  <i className="bi bi-list-check"></i> Управление на поръчки
                </h5>
                <div className="header-actions">
                  <div className="btn-group view-toggle" role="group">
                    <button
                      type="button"
                      className={`btn btn-outline-primary ${viewMode === 'table' ? 'active' : ''}`}
                      onClick={() => setViewMode('table')}
                    >
                      <i className="bi bi-table"></i> Таблица
                    </button>
                    <button
                      type="button"
                      className={`btn btn-outline-primary ${viewMode === 'card' ? 'active' : ''}`}
                      onClick={() => setViewMode('card')}
                    >
                      <i className="bi bi-grid-3x3-gap"></i> Карти
                    </button>
                  </div>
                </div>
              </div>
            </div>

            {viewMode === 'table' ? (
              <div className="card-body p-0">
                <div className="table-responsive">
                  <table className="table table-hover mb-0">
                    <thead>
                      <tr>
                        <th>ID</th>
                        <th>Заглавие</th>
                        <th>Клиент</th>
                        <th>Заявена дата</th>
                        <th>Статус</th>
                        <th>Действия</th>
                      </tr>
                    </thead>
                    <tbody>
                      {orders.map((order) => (
                        <tr key={order.id}>
                          <td>
                            <span className="order-id">#{order.id}</span>
                          </td>
                          <td className="order-title">{order.title}</td>
                          <td>
                            <div className="client-name">
                              {order.client ? (
                                <>
                                  <div className="client-avatar">
                                    {order.client.profileImage ? (
                                      <img
                                        src={order.client.profileImage}
                                        alt={`${order.client.firstName} ${order.client.lastName}`}
                                      />
                                    ) : (
                                      <span>
                                        {order.client.firstName?.[0]}
                                        {order.client.lastName?.[0]}
                                      </span>
                                    )}
                                  </div>
                                  <span>
                                    {order.client.firstName} {order.client.lastName}
                                  </span>
                                </>
                              ) : (
                                <span className="text-muted">Not assigned</span>
                              )}
                            </div>
                          </td>
                          <td className="order-date">
                            <i className="bi bi-calendar3 me-1"></i>
                            {formatDate(order.requestedDate)}
                          </td>
                          <td>
                            <span className={`status-badge bg-${getStatusBadgeClass(order.status)}`}>
                              <i className={`bi ${getStatusIcon(order.status)}`}></i>
                              {getStatusDisplayName(order.status)}
                            </span>
                          </td>
                          <td>
                            <div className="btn-group">
                              <Link
                                to={`/orders/${order.id}`}
                                className="btn btn-sm btn-info btn-action"
                                title="View Details"
                              >
                                <i className="bi bi-info-circle"></i>
                              </Link>
                              {isAdminOrManager && (
                                <>
                                  <Link
                                    to={`/orders/${order.id}/edit`}
                                    className="btn btn-sm btn-primary btn-action"
                                    title="Edit Order"
                                  >
                                    <i className="bi bi-pencil"></i>
                                  </Link>
                                  <Link
                                    to={`/orders/${order.id}/delete`}
                                    className="btn btn-sm btn-danger btn-action"
                                    title="Delete Order"
                                  >
                                    <i className="bi bi-trash"></i>
                                  </Link>
                                </>
                              )}
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            ) : (
              <div className="card-body p-3">
                <div className="row">
                  {orders.map((order) => (
                    <div key={order.id} className="col-xl-4 col-md-6 mb-4">
                      <div className="order-card-item">
                        <div className={`order-card-header status-${order.status.toLowerCase()}`}>
                          <span className="status-indicator">
                            <i className={`bi ${getStatusIcon(order.status)}`}></i>
                            {getStatusDisplayName(order.status)}
                          </span>
                          <span className="order-number">#{order.id}</span>
                        </div>
                        <div className="order-card-body">
                          <h5 className="order-card-title">{order.title}</h5>
                          <div className="order-card-info">
                            <div className="info-row">
                              <i className="bi bi-person"></i>
                              <span>
                                {order.client
                                  ? `${order.client.firstName} ${order.client.lastName}`
                                  : 'Not assigned'}
                              </span>
                            </div>
                            <div className="info-row">
                              <i className="bi bi-calendar3"></i>
                              <span>{formatDate(order.requestedDate)}</span>
                            </div>
                            {order.description && (
                              <div className="info-row description">
                                <i className="bi bi-card-text"></i>
                                <span>
                                  {order.description.length > 50
                                    ? `${order.description.substring(0, 50)}...`
                                    : order.description}
                                </span>
                              </div>
                            )}
                          </div>
                        </div>
                        <div className="order-card-footer">
                          <Link to={`/orders/${order.id}`} className="btn btn-sm btn-outline-info">
                            <i className="bi bi-info-circle me-1"></i> Details
                          </Link>
                          {isAdminOrManager && (
                            <Link
                              to={`/orders/${order.id}/edit`}
                              className="btn btn-sm btn-outline-primary"
                            >
                              <i className="bi bi-pencil me-1"></i> Edit
                            </Link>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Pagination */}
            {pagination.totalPages > 1 && (
              <div className="pagination-container">
                <nav aria-label="Page navigation">
                  <ul className="pagination justify-content-center mb-0">
                    <li className={`page-item ${pagination.pageNumber === 1 ? 'disabled' : ''}`}>
                      <button
                        className="page-link"
                        onClick={() =>
                          setPagination((prev) => ({ ...prev, pageNumber: prev.pageNumber - 1 }))
                        }
                        disabled={pagination.pageNumber === 1}
                      >
                        <span aria-hidden="true">&laquo;</span>
                      </button>
                    </li>
                    {[...Array(pagination.totalPages)].map((_, i) => (
                      <li
                        key={i + 1}
                        className={`page-item ${pagination.pageNumber === i + 1 ? 'active' : ''}`}
                      >
                        <button
                          className="page-link"
                          onClick={() => setPagination((prev) => ({ ...prev, pageNumber: i + 1 }))}
                        >
                          {i + 1}
                        </button>
                      </li>
                    ))}
                    <li
                      className={`page-item ${
                        pagination.pageNumber === pagination.totalPages ? 'disabled' : ''
                      }`}
                    >
                      <button
                        className="page-link"
                        onClick={() =>
                          setPagination((prev) => ({ ...prev, pageNumber: prev.pageNumber + 1 }))
                        }
                        disabled={pagination.pageNumber === pagination.totalPages}
                      >
                        <span aria-hidden="true">&raquo;</span>
                      </button>
                    </li>
                  </ul>
                </nav>
              </div>
            )}
          </div>
        </>
      ) : (
        <div className="empty-state">
          <div className="empty-state-icon">
            <i className="bi bi-shield-check"></i>
          </div>
          <h4>No Orders Yet</h4>
          <p>
            You haven't created any security system orders yet. Start by creating your first order
            to secure your properties.
          </p>
          {canCreateOrder && (
            <Link to="/orders/create" className="btn btn-primary btn-create-first">
              <i className="bi bi-plus-lg me-2"></i>Create Your First Order
            </Link>
          )}
        </div>
      )}
    </div>
  );
}

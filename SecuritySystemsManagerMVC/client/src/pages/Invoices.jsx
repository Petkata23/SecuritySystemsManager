import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { invoiceService } from '../services/invoiceService';
import '../styles/invoice/invoice.css';

export default function Invoices() {
  const { user } = useAuth();
  const [invoices, setInvoices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState({
    searchTerm: '',
    paymentStatus: '',
  });
  const [showFilters, setShowFilters] = useState(false);
  const [pagination, setPagination] = useState({
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 1,
  });

  const userRole = user?.roles?.[0] || 'Client';
  const isClient = userRole === 'Client';
  const canCreateInvoices =
    userRole === 'Admin' || userRole === 'Manager' || userRole === 'Technician';
  const canManageInvoices = userRole === 'Admin' || userRole === 'Manager';

  useEffect(() => {
    loadInvoices();
  }, [pagination.pageNumber, filters]);

  const loadInvoices = async () => {
    try {
      setLoading(true);
      console.log('Loading invoices with filters:', filters);
      console.log('Pagination:', pagination);
      
      const hasFilters = filters.searchTerm || filters.paymentStatus;
      const response = hasFilters
        ? await invoiceService.getFiltered(filters, pagination.pageSize, pagination.pageNumber)
        : await invoiceService.getAll(pagination.pageSize, pagination.pageNumber);

      console.log('Invoices response:', response);
      console.log('Invoices response.data:', response.data);
      console.log('Invoices response.pagination:', response.pagination);

      const invoicesData = Array.isArray(response.data) ? response.data : (Array.isArray(response) ? response : []);
      setInvoices(invoicesData);
      
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
      
      console.log('Invoices loaded:', invoicesData.length);
    } catch (error) {
      console.error('Error loading invoices:', error);
      console.error('Error details:', {
        message: error.message,
        response: error.response?.data,
        status: error.response?.status,
      });
      setInvoices([]);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (key, value) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
    setPagination((prev) => ({ ...prev, pageNumber: 1 }));
  };

  const formatDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('bg-BG');
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('bg-BG', {
      style: 'currency',
      currency: 'BGN',
    }).format(amount);
  };

  const stats = {
    total: invoices.length,
    paid: invoices.filter((i) => i.isPaid).length,
    unpaid: invoices.filter((i) => !i.isPaid).length,
    totalAmount: invoices.reduce((sum, i) => sum + (i.totalAmount || 0), 0),
  };

  if (loading) {
    return (
      <div className="container mt-4" style={{ padding: '2rem' }}>
        <div className="text-center">
          <div className="spinner-border" role="status">
            <span className="visually-hidden">Зареждане...</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mt-4">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="mb-0">
            <i className="bi bi-receipt me-2"></i>Фактури
          </h1>
          <p className="text-muted">Управлявай и проследявай всички системни фактури</p>
        </div>
        {canCreateInvoices && (
          <div>
            <Link to="/invoices/create" className="btn btn-primary">
              <i className="bi bi-plus-circle me-2"></i>Създай нова фактура
            </Link>
          </div>
        )}
      </div>

      {/* Statistics Cards */}
      <div className="row mb-4">
        <div className="col-md-3">
          <div className="card bg-primary text-white">
            <div className="card-body">
              <h5 className="card-title">Общо фактури</h5>
              <h2 className="mb-0">{stats.total}</h2>
            </div>
          </div>
        </div>
        <div className="col-md-3">
          <div className="card bg-success text-white">
            <div className="card-body">
              <h5 className="card-title">Платени фактури</h5>
              <h2 className="mb-0">{stats.paid}</h2>
            </div>
          </div>
        </div>
        <div className="col-md-3">
          <div className="card bg-danger text-white">
            <div className="card-body">
              <h5 className="card-title">Неплатени фактури</h5>
              <h2 className="mb-0">{stats.unpaid}</h2>
            </div>
          </div>
        </div>
        <div className="col-md-3">
          <div className="card" style={{ backgroundColor: 'var(--info-color)', color: 'white' }}>
            <div className="card-body">
              <h5 className="card-title">Обща сума</h5>
              <h2 className="mb-0">{formatCurrency(stats.totalAmount)}</h2>
            </div>
          </div>
        </div>
      </div>

      {invoices.length > 0 ? (
        <>
          {/* Filter Card */}
          <div className="card filter-card">
            <div className="card-header d-flex justify-content-between align-items-center">
              <h5>
                <i className="bi bi-funnel"></i> Filter Invoices
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
                  <div className="col-md-6">
                    <label className="form-label">Payment Status</label>
                    <select
                      className="form-select"
                      value={filters.paymentStatus}
                      onChange={(e) => handleFilterChange('paymentStatus', e.target.value)}
                    >
                      <option value="">All Statuses</option>
                      <option value="paid">Paid</option>
                      <option value="unpaid">Unpaid</option>
                    </select>
                  </div>
                  <div className="col-md-6">
                    <label className="form-label">Search</label>
                    <div className="input-group">
                      <input
                        type="text"
                        className="form-control"
                        placeholder="Search invoices..."
                        value={filters.searchTerm}
                        onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                      />
                      <button
                        className="btn btn-primary"
                        type="button"
                        onClick={() => loadInvoices()}
                      >
                        <i className="bi bi-search"></i>
                      </button>
                    </div>
                  </div>
                </form>
              </div>
            )}
          </div>

          {/* Invoices Table Card */}
          <div className="card invoices-card">
            <div className="card-header">
              <div className="d-flex justify-content-between align-items-center">
                <h5>
                  <i className="bi bi-receipt"></i> Управление на фактури
                </h5>
                <div className="header-actions">
                  <span className="text-muted">Показване на {invoices.length} фактури</span>
                </div>
              </div>
            </div>

            <div className="card-body p-0">
              <div className="invoice-list-container">
                <div className="invoice-list-body">
                  {invoices.map((invoice) => (
                    <div key={invoice.id} className="invoice-list-item">
                      <div className="invoice-list-item-info">
                        <div className="invoice-list-item-number">
                          Invoice #{invoice.id.toString().padStart(6, '0')}
                        </div>
                        <div className="invoice-list-item-date">
                          <i className="bi bi-calendar-event me-1"></i>
                          {formatDate(invoice.issuedOn)}
                        </div>
                        <div className="invoice-list-item-client">{invoice.orderTitle}</div>
                      </div>
                      <div className="d-flex align-items-center gap-4">
                        <div className="invoice-list-item-amount">
                          {formatCurrency(invoice.totalAmount)}
                        </div>
                        <div
                          className={`invoice-status ${invoice.isPaid ? 'status-paid' : 'status-unpaid'}`}
                        >
                          <i
                            className={`bi ${invoice.isPaid ? 'bi-check-circle' : 'bi-clock'} me-1`}
                          ></i>
                          {invoice.isPaid ? 'Платена' : 'Неплатена'}
                        </div>
                        <div className="invoice-list-item-actions">
                          <Link
                            to={`/invoices/${invoice.id}`}
                            className="btn btn-sm btn-outline-primary"
                            title="Виж детайли"
                          >
                            <i className="bi bi-eye"></i>
                          </Link>
                          {canManageInvoices && (
                            <>
                              <Link
                                to={`/invoices/${invoice.id}/edit`}
                                className="btn btn-sm btn-outline-secondary"
                                title="Редактирай"
                              >
                                <i className="bi bi-pencil"></i>
                              </Link>
                              <Link
                                to={`/invoices/${invoice.id}/delete`}
                                className="btn btn-sm btn-outline-danger"
                                title="Delete"
                              >
                                <i className="bi bi-trash"></i>
                              </Link>
                            </>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Pagination */}
            {pagination.totalPages > 1 && (
              <div className="card-footer">
                <div className="d-flex justify-content-between align-items-center">
                  <div>
                    <span className="text-muted">Showing {invoices.length} invoices</span>
                  </div>
                  <nav aria-label="Invoice pagination">
                    <ul className="pagination mb-0">
                      {pagination.pageNumber > 1 && (
                        <li className="page-item">
                          <button
                            className="page-link"
                            onClick={() =>
                              setPagination((prev) => ({ ...prev, pageNumber: prev.pageNumber - 1 }))
                            }
                          >
                            <span aria-hidden="true">&laquo;</span>
                          </button>
                        </li>
                      )}

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

                      {pagination.pageNumber < pagination.totalPages && (
                        <li className="page-item">
                          <button
                            className="page-link"
                            onClick={() =>
                              setPagination((prev) => ({ ...prev, pageNumber: prev.pageNumber + 1 }))
                            }
                          >
                            <span aria-hidden="true">&raquo;</span>
                          </button>
                        </li>
                      )}
                    </ul>
                  </nav>
                </div>
              </div>
            )}
          </div>
        </>
      ) : (
        <div className="text-center py-5" style={{ backgroundColor: 'var(--card-bg)' }}>
          <i className="bi bi-receipt text-muted" style={{ fontSize: '3rem' }}></i>
          <h4 className="mt-3">No Invoices Found</h4>
          <p className="text-muted">There are no invoices in the system for you yet.</p>
          {canCreateInvoices && (
            <Link to="/invoices/create" className="btn btn-primary mt-2">
              <i className="bi bi-plus-circle me-2"></i>Create First Invoice
            </Link>
          )}
        </div>
      )}
    </div>
  );
}

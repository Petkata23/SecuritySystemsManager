import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
// Import Bootstrap CSS first (before custom styles)
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap-icons/font/bootstrap-icons.css'
// Import base styles
import './index.css'
// Import all application styles in order
import './styles/site.css'
import './styles/home.css'
import './styles/orders-list.css'
import './styles/invoice/invoice.css'
import './styles/locations.css'
import './styles/navbar-mobile.css'
import './styles/forms-mobile.css'
import './styles/tables-mobile.css'
import './styles/mobile-enhancements.css'
import App from './App.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)

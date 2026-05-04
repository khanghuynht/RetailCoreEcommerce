import { createBrowserRouter, Navigate } from 'react-router-dom'
import AdminLayout from '@/components/layout/AdminLayout'
import ProtectedRoute from './ProtectedRoute'
import LoginPage from '@/features/auth/LoginPage'
import DashboardPage from '@/features/dashboard/DashboardPage'
import CategoryListPage from '@/features/categories/CategoryListPage'
import ProductListPage from '@/features/products/ProductListPage'
import CustomerListPage from '@/features/customers/CustomerListPage'

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AdminLayout />,
        children: [
          { path: '/', element: <Navigate to="/dashboard" replace /> },
          { path: '/dashboard', element: <DashboardPage /> },
          { path: '/categories', element: <CategoryListPage /> },
          { path: '/products', element: <ProductListPage /> },
          { path: '/customers', element: <CustomerListPage /> },
        ],
      },
    ],
  },
])

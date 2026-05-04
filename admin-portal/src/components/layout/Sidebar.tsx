import { NavLink } from 'react-router-dom'
import { LayoutDashboard, Tag, Package, Users, LogOut } from 'lucide-react'
import { useAuthStore } from '@/store/authStore'
import { cn } from '@/utils/formatters'

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/categories', label: 'Categories', icon: Tag },
  { to: '/products', label: 'Products', icon: Package },
  { to: '/customers', label: 'Customers', icon: Users },
]

export default function Sidebar() {
  const { logout, user } = useAuthStore()

  return (
    <aside className="flex h-screen w-60 flex-col bg-gray-900 text-white">
      <div className="flex items-center gap-3 px-5 py-5 border-b border-gray-700">
        <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-blue-600">
          <Package size={18} />
        </div>
        <div>
          <p className="font-semibold text-sm leading-tight">Admin Portal</p>
          <p className="text-xs text-gray-400">Retail Core</p>
        </div>
      </div>

      <nav className="flex-1 px-3 py-4 space-y-0.5">
        {navItems.map(({ to, label, icon: Icon }) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-blue-600 text-white'
                  : 'text-gray-300 hover:bg-gray-800 hover:text-white'
              )
            }
          >
            <Icon size={17} />
            {label}
          </NavLink>
        ))}
      </nav>

      <div className="border-t border-gray-700 px-3 py-3">
        <div className="px-3 py-2 mb-1">
          <p className="text-sm font-medium truncate">{user?.name ?? 'Admin'}</p>
          <p className="text-xs text-gray-400 truncate">{user?.email ?? ''}</p>
        </div>
        <button
          onClick={logout}
          className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm text-gray-300 hover:bg-gray-800 hover:text-white transition-colors"
        >
          <LogOut size={16} />
          Logout
        </button>
      </div>
    </aside>
  )
}

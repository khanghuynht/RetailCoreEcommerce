import { useLocation, useNavigate } from 'react-router-dom'
import { LogOut, UserCircle } from 'lucide-react'
import { authApi } from '@/api/authApi'
import { useAuthStore } from '@/store/authStore'

const titleMap: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/categories': 'Category Management',
  '/products': 'Product Management',
  '/customers': 'Customer Oversight',
}

export default function Header() {
  const { pathname } = useLocation()
  const navigate = useNavigate()
  const title = Object.entries(titleMap).find(([key]) => pathname.startsWith(key))?.[1] ?? 'Admin Portal'

  const user = useAuthStore((s) => s.user)
  const accessToken = useAuthStore((s) => s.accessToken)
  const refreshToken = useAuthStore((s) => s.refreshToken)
  const clearSession = useAuthStore((s) => s.clearSession)

  const handleLogout = async () => {
    try {
      if (accessToken && refreshToken) {
        await authApi.logout({ accessToken, refreshToken })
      }
    } catch {
      // Proceed with client-side logout even if API call fails
    } finally {
      clearSession()
      navigate('/login', { replace: true })
    }
  }

  return (
    <header className="flex items-center justify-between border-b border-gray-200 bg-white px-6 py-4">
      <h1 className="text-xl font-semibold text-gray-800">{title}</h1>

      <div className="flex items-center gap-4">
        <span className="text-sm text-gray-400">
          {new Intl.DateTimeFormat('en-US', { dateStyle: 'full' }).format(new Date())}
        </span>

        {user && (
          <div className="flex items-center gap-3 border-l border-gray-200 pl-4">
            <div className="flex items-center gap-2 text-sm text-gray-700">
              <UserCircle size={18} className="text-gray-400" />
              <span className="font-medium">{user.fullName}</span>
              <span className="rounded-full bg-blue-50 px-2 py-0.5 text-xs font-medium text-blue-600">
                {user.role}
              </span>
            </div>
            <button
              onClick={handleLogout}
              className="flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-sm text-gray-500 hover:bg-red-50 hover:text-red-600 transition-colors"
              title="Sign out"
            >
              <LogOut size={15} />
              Sign out
            </button>
          </div>
        )}
      </div>
    </header>
  )
}

import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { LoginResponse } from '@/api/authApi'

export interface AuthUser {
  id: string
  fullName: string
  role: string
  registeredAt: string | null
}

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  user: AuthUser | null
  isAuthenticated: boolean
  setSession: (response: LoginResponse) => void
  clearSession: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      isAuthenticated: false,

      setSession: (response: LoginResponse) =>
        set({
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          user: {
            id: response.id,
            fullName: response.fullName,
            role: response.role,
            registeredAt: response.registeredAt,
          },
          isAuthenticated: true,
        }),

      clearSession: () =>
        set({
          accessToken: null,
          refreshToken: null,
          user: null,
          isAuthenticated: false,
        }),
    }),
    { name: 'admin-auth' }
  )
)

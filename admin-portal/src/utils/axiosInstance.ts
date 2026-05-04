import axios, { type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
import type { ApiResult } from '@/types/api'
import { useAuthStore } from '@/store/authStore'
import type { LoginResponse } from '@/api/authApi'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

const axiosInstance = axios.create({
  baseURL: BASE_URL,
  timeout: 15000,
  headers: { 'Content-Type': 'application/json' },
})

// ── Request: attach access token from store ──────────────────────────────────
axiosInstance.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const { accessToken } = useAuthStore.getState()
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }
  return config
})

// ── Response: unwrap Result<T> envelope + silent token refresh on 401 ────────

/** Prevents multiple simultaneous refresh calls */
let isRefreshing = false
/** Queued requests waiting for the new token */
let waitQueue: Array<(newToken: string) => void> = []

function drainQueue(newToken: string) {
  waitQueue.forEach((resolve) => resolve(newToken))
  waitQueue = []
}

function abortQueue() {
  waitQueue = []
}

axiosInstance.interceptors.response.use(
  (response: AxiosResponse<unknown>) => {
    const body = response.data as ApiResult<unknown>
    if (body && typeof body === 'object' && 'isSuccess' in body) {
      if (body.isSuccess) {
        // Replace envelope with inner payload — Axios generics can't express this, so widen `unknown`
        return { ...response, data: body.data } as AxiosResponse
      }
      return Promise.reject(new Error(body.error?.message ?? 'Request failed'))
    }
    return response
  },

  async (error) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    // ── 401 handling: try silent refresh once ────────────────────────────────
    if (error.response?.status === 401 && !originalRequest._retry) {
      const { accessToken, refreshToken, setSession, clearSession } = useAuthStore.getState()

      if (!refreshToken) {
        clearSession()
        window.location.href = '/login'
        return Promise.reject(error)
      }

      // Another request is already refreshing — queue this one
      if (isRefreshing) {
        return new Promise<AxiosResponse>((resolve) => {
          waitQueue.push((newToken) => {
            originalRequest.headers.Authorization = `Bearer ${newToken}`
            resolve(axiosInstance(originalRequest))
          })
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        // Use raw axios to bypass our interceptors and avoid infinite loops
        const { data } = await axios.post<ApiResult<LoginResponse>>(
          `${BASE_URL}/v1/auth/refresh`,
          { accessToken, refreshToken },
          { headers: { 'Content-Type': 'application/json' } },
        )

        const session = data.data!
        setSession(session)
        drainQueue(session.accessToken)

        originalRequest.headers.Authorization = `Bearer ${session.accessToken}`
        return axiosInstance(originalRequest)
      } catch {
        abortQueue()
        clearSession()
        window.location.href = '/login'
        return Promise.reject(error)
      } finally {
        isRefreshing = false
      }
    }

    // Surface backend error message when available
    const apiMessage = error.response?.data?.error?.message
    if (apiMessage) error.message = apiMessage

    return Promise.reject(error)
  },
)

export default axiosInstance

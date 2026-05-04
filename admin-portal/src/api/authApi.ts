import axiosInstance from '@/utils/axiosInstance'

export interface LoginPayload {
  loginIdentifier: string
  password: string
}

export interface LoginResponse {
  id: string
  fullName: string
  role: string
  registeredAt: string | null
  accessToken: string
  refreshToken: string
}

export interface RefreshPayload {
  accessToken: string
  refreshToken: string
}

export const authApi = {
  login: async (payload: LoginPayload): Promise<LoginResponse> => {
    const { data } = await axiosInstance.post<LoginResponse>('v1/auth/login', payload)
    return data
  },

  refresh: async (payload: RefreshPayload): Promise<LoginResponse> => {
    const { data } = await axiosInstance.post<LoginResponse>('v1/auth/refresh', payload)
    return data
  },

  logout: async (payload: RefreshPayload): Promise<void> => {
    await axiosInstance.post('v1/auth/logout', payload)
  },
}

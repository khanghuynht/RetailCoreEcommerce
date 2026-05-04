import axiosInstance from '@/utils/axiosInstance'
import type { Customer } from '@/types/customer'
import type { PaginationResult } from '@/types/api'

export interface GetCustomersParams {
  pageNumber?: number
  pageSize?: number
  name?: string   // searches firstName or lastName
  email?: string
}

export const customerApi = {
  /** GET /v1/users/customers — paginated */
  getPaged: async (params: GetCustomersParams = {}): Promise<PaginationResult<Customer>> => {
    const { data } = await axiosInstance.get('/v1/users/customers', { params })
    return data as PaginationResult<Customer>
  },
}

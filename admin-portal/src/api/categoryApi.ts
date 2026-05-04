import axiosInstance from '@/utils/axiosInstance'
import type { Category, CategoryDetail, CreateCategoryPayload, UpdateCategoryPayload } from '@/types/category'
import type { PaginationResult } from '@/types/api'

export interface GetCategoriesParams {
  pageNumber?: number
  pageSize?: number
  name?: string
  parentId?: string
  isRootOnly?: boolean
}

export const categoryApi = {
  /** GET /v1/categories — paginated */
  getPaged: async (params: GetCategoriesParams = {}): Promise<PaginationResult<Category>> => {
    const { data } = await axiosInstance.get('/v1/categories', { params })
    return data as PaginationResult<Category>
  },

  /** GET /v1/categories/{id} */
  getById: async (id: string): Promise<CategoryDetail> => {
    const { data } = await axiosInstance.get(`/v1/categories/${id}`)
    return data as CategoryDetail
  },

  /** POST /v1/categories */
  create: async (payload: CreateCategoryPayload): Promise<CategoryDetail> => {
    const { data } = await axiosInstance.post('/v1/categories', payload)
    return data as CategoryDetail
  },

  /** PATCH /v1/categories/{id} */
  update: async (id: string, payload: UpdateCategoryPayload): Promise<CategoryDetail> => {
    const { data } = await axiosInstance.patch(`/v1/categories/${id}`, payload)
    return data as CategoryDetail
  },

  /** DELETE /v1/categories/{id} */
  remove: async (id: string): Promise<void> => {
    await axiosInstance.delete(`/v1/categories/${id}`)
  },
}

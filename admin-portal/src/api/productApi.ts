import axiosInstance from '@/utils/axiosInstance'
import type { Product, ProductListItem, ProductImage, CreateProductPayload, UpdateProductPayload, UpdateInventoryPayload } from '@/types/product'
import type { PaginationResult } from '@/types/api'

export interface GetProductsParams {
  pageNumber?: number
  pageSize?: number
  name?: string
  categoryId?: string
  isActive?: boolean
}

export const productApi = {
  /** GET /v1/products — paginated list */
  getPaged: async (params: GetProductsParams = {}): Promise<PaginationResult<ProductListItem>> => {
    const { data } = await axiosInstance.get('/v1/products', { params })
    return data as PaginationResult<ProductListItem>
  },

  /** GET /v1/products/{id} — full detail with images */
  getById: async (id: string): Promise<Product> => {
    const { data } = await axiosInstance.get(`/v1/products/${id}`)
    return data as Product
  },

  /** POST /v1/products — JSON body, no images */
  create: async (payload: CreateProductPayload): Promise<Product> => {
    const { data } = await axiosInstance.post('/v1/products', payload)
    return data as Product
  },

  /** PATCH /v1/products/{id} — JSON partial update */
  update: async (id: string, payload: UpdateProductPayload): Promise<Product> => {
    const { data } = await axiosInstance.patch(`/v1/products/${id}`, payload)
    return data as Product
  },

  /** DELETE /v1/products/{id} */
  remove: async (id: string): Promise<void> => {
    await axiosInstance.delete(`/v1/products/${id}`)
  },

  /** POST /v1/products/{id}/thumbnail — single IFormFile */
  uploadThumbnail: async (id: string, file: File): Promise<Product> => {
    const fd = new FormData()
    fd.append('file', file)
    const { data } = await axiosInstance.post(`/v1/products/${id}/thumbnail`, fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return data as Product
  },

  /** POST /v1/products/{id}/images — single IFormFile added to gallery */
  addImage: async (id: string, file: File): Promise<ProductImage> => {
    const fd = new FormData()
    fd.append('file', file)
    const { data } = await axiosInstance.post(`/v1/products/${id}/images`, fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return data as ProductImage
  },

  /** DELETE /v1/products/{productId}/images/{imageId} */
  deleteImage: async (productId: string, imageId: string): Promise<void> => {
    await axiosInstance.delete(`/v1/products/${productId}/images/${imageId}`)
  },

  /** PATCH /v1/products/{id}/images/reorder — ordered list of imageIds */
  reorderImages: async (productId: string, orderedImageIds: string[]): Promise<void> => {
    await axiosInstance.patch(`/v1/products/${productId}/images/reorder`, orderedImageIds)
  },

  /** PATCH /v1/products/{id}/inventory */
  updateInventory: async (id: string, payload: UpdateInventoryPayload): Promise<void> => {
    await axiosInstance.patch(`/v1/products/${id}/inventory`, payload)
  },
}

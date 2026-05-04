/** Matches ProductImageResponse */
export interface ProductImage {
  id: string
  imageUrl: string
  name?: string
  position: number
}

/** Matches GetAllProductResponse (list item) */
export interface ProductListItem {
  id: string
  categoryId: string
  title: string
  sku: string
  originalPrice: number
  salePrice?: number
  thumbnailUrl?: string
  isActive: boolean
}

/** Matches GetProductResponse (detail / after create) */
export interface Product extends ProductListItem {
  categoryName?: string
  name: string
  description?: string
  length: number
  width: number
  height: number
  status: string
  stockQuantity: number
  reservedQuantity: number
  soldQuantity: number
  images: ProductImage[]
}

/** Matches CreateProductRequest */
export interface CreateProductPayload {
  categoryId: string
  title: string
  sku: string
  name: string
  originalPrice: number
  salePrice?: number
  description?: string
  length: number
  width: number
  height: number
  initialStock: number
}

/** Matches UpdateProductRequest */
export interface UpdateProductPayload {
  categoryId?: string
  title?: string
  name?: string
  originalPrice?: number
  salePrice?: number
  description?: string
  length?: number
  width?: number
  height?: number
  isActive?: boolean
}

/** Matches UpdateInventoryQuantityRequest */
export interface UpdateInventoryPayload {
  stockQuantity: number
}

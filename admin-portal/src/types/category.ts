/** Matches GetPagedCategoryResponse (list item) */
export interface Category {
  id: string
  name: string
  description?: string
  parentId?: string
}

/** Matches GetCategoryResponse (detail) */
export interface CategoryDetail extends Category {
  parentName?: string
  childrenCount: number
}

/** Matches CreateCategoryRequest */
export interface CreateCategoryPayload {
  name: string
  description?: string
  parentId?: string
}

/** Matches UpdateCategoryRequest */
export interface UpdateCategoryPayload {
  name?: string
  description?: string
  parentId?: string
}

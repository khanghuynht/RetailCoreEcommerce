/** Matches backend Result<T> wrapper returned by BaseApiController */
export interface ApiResult<T> {
  isSuccess: boolean
  data?: T
  error?: { code: string; message: string }
}

/** Matches backend PaginationResult<T> */
export interface PaginationResult<T> {
  items: T[]
  totalItems: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface ApiError {
  code: string
  message: string
}

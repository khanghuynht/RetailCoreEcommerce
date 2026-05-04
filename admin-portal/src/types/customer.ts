/** Matches GetPagedUserResponse */
export interface Customer {
  id: string
  username: string
  email: string
  firstName: string
  lastName: string
  phoneNumber?: string
  address?: string
  province?: string
  ward?: string
  registeredAt: string
}

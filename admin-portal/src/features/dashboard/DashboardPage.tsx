import { useQuery } from '@tanstack/react-query'
import { Tag, Package, Users } from 'lucide-react'
import { categoryApi } from '@/api/categoryApi'
import { productApi } from '@/api/productApi'
import { customerApi } from '@/api/customerApi'
import { Link } from 'react-router-dom'

interface StatCardProps {
  label: string
  value: number | string
  icon: React.ReactNode
  to: string
  color: string
}

function StatCard({ label, value, icon, to, color }: StatCardProps) {
  return (
    <Link
      to={to}
      className="flex items-center gap-5 rounded-2xl bg-white p-6 shadow-sm border border-gray-100 hover:shadow-md transition-shadow"
    >
      <div className={`flex h-14 w-14 items-center justify-center rounded-xl ${color}`}>
        {icon}
      </div>
      <div>
        <p className="text-3xl font-bold text-gray-900">{value}</p>
        <p className="text-sm text-gray-500 mt-0.5">{label}</p>
      </div>
    </Link>
  )
}

export default function DashboardPage() {
  const { data: categories } = useQuery({
    queryKey: ['categories', 1, ''],
    queryFn: () => categoryApi.getPaged({ pageNumber: 1, pageSize: 1 }),
  })

  const { data: products } = useQuery({
    queryKey: ['products', 1, ''],
    queryFn: () => productApi.getPaged({ pageNumber: 1, pageSize: 1 }),
  })

  const { data: customers } = useQuery({
    queryKey: ['customers', 1, ''],
    queryFn: () => customerApi.getPaged({ pageNumber: 1, pageSize: 1 }),
  })

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-lg font-semibold text-gray-800 mb-1">Overview</h2>
        <p className="text-sm text-gray-500">Welcome back to the Admin Portal.</p>
      </div>

      <div className="grid grid-cols-1 gap-5 sm:grid-cols-3">
        <StatCard
          label="Total Categories"
          value={categories?.totalItems ?? '—'}
          icon={<Tag size={24} className="text-blue-600" />}
          to="/categories"
          color="bg-blue-50"
        />
        <StatCard
          label="Total Products"
          value={products?.totalItems ?? '—'}
          icon={<Package size={24} className="text-violet-600" />}
          to="/products"
          color="bg-violet-50"
        />
        <StatCard
          label="Registered Customers"
          value={customers?.totalItems ?? '—'}
          icon={<Users size={24} className="text-emerald-600" />}
          to="/customers"
          color="bg-emerald-50"
        />
      </div>
    </div>
  )
}

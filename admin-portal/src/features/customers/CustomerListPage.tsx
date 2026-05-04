import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { customerApi } from '@/api/customerApi'
import type { Customer } from '@/types/customer'
import DataTable, { type Column } from '@/components/shared/DataTable'
import Pagination from '@/components/shared/Pagination'
import { formatDate } from '@/utils/formatters'
import { useDebounce } from '@/hooks/useDebounce'

const PAGE_SIZE = 10

export default function CustomerListPage() {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebounce(search, 400)

  const { data, isLoading } = useQuery({
    queryKey: ['customers', page, debouncedSearch],
    queryFn: () =>
      customerApi.getPaged({
        pageNumber: page,
        pageSize: PAGE_SIZE,
        name: debouncedSearch || undefined,
      }),
    placeholderData: (prev) => prev,
  })

  const handleSearch = (val: string) => {
    setSearch(val)
    setPage(1)
  }

  const columns: Column<Customer>[] = [
    {
      key: 'name',
      header: 'Full Name',
      sortable: true,
      render: (row) => (
        <div>
          <p className="font-medium text-gray-900">
            {row.firstName} {row.lastName}
          </p>
          <p className="text-xs text-gray-400">@{row.username}</p>
        </div>
      ),
    },
    {
      key: 'email',
      header: 'Email',
      render: (row) => (
        <a href={`mailto:${row.email}`} className="text-blue-600 hover:underline text-sm">
          {row.email}
        </a>
      ),
    },
    {
      key: 'phoneNumber',
      header: 'Phone',
      render: (row) =>
        row.phoneNumber ? (
          <span className="font-mono text-sm">{row.phoneNumber}</span>
        ) : (
          <span className="text-gray-300">—</span>
        ),
    },
    {
      key: 'address',
      header: 'Location',
      render: (row) => {
        const parts = [row.ward, row.province].filter(Boolean)
        return parts.length > 0 ? (
          <span className="text-sm text-gray-600">{parts.join(', ')}</span>
        ) : (
          <span className="text-gray-300">—</span>
        )
      },
    },
    {
      key: 'registeredAt',
      header: 'Registered',
      sortable: true,
      render: (row) => (
        <span className="text-gray-500 text-sm">{formatDate(row.registeredAt)}</span>
      ),
    },
  ]

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-base font-semibold text-gray-800">Registered Customers</h2>
          <p className="text-sm text-gray-500">{data?.totalItems ?? 0} total customers</p>
        </div>
      </div>

      <div className="relative w-full max-w-sm">
        <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
        <input
          type="text"
          placeholder="Search by name…"
          value={search}
          onChange={(e) => handleSearch(e.target.value)}
          className="w-full rounded-lg border border-gray-300 py-2 pl-9 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <DataTable
        columns={columns}
        data={data?.items ?? []}
        keyField="id"
        loading={isLoading}
        emptyText="No customers found."
      />

      {data && data.totalItems > 0 && (
        <Pagination
          page={page}
          totalPages={data.totalPages}
          total={data.totalItems}
          pageSize={PAGE_SIZE}
          onPageChange={setPage}
        />
      )}
    </div>
  )
}

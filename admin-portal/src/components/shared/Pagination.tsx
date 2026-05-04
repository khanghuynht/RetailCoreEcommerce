import { ChevronLeft, ChevronRight } from 'lucide-react'
import { cn } from '@/utils/formatters'

interface PaginationProps {
  page: number
  totalPages: number
  total: number
  pageSize: number
  onPageChange: (page: number) => void
}

export default function Pagination({ page, totalPages, total, pageSize, onPageChange }: PaginationProps) {
  const start = (page - 1) * pageSize + 1
  const end = Math.min(page * pageSize, total)

  const pages = Array.from({ length: totalPages }, (_, i) => i + 1).filter(
    (p) => p === 1 || p === totalPages || Math.abs(p - page) <= 1
  )

  return (
    <div className="flex items-center justify-between px-1 py-2 text-sm text-gray-600">
      <span>
        Showing {start}–{end} of {total} results
      </span>
      <div className="flex items-center gap-1">
        <button
          disabled={page === 1}
          onClick={() => onPageChange(page - 1)}
          className="rounded-lg p-1.5 hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
        >
          <ChevronLeft size={16} />
        </button>
        {pages.map((p, i) => {
          const prev = pages[i - 1]
          return (
            <span key={p} className="flex items-center gap-1">
              {prev && p - prev > 1 && <span className="px-1 text-gray-400">…</span>}
              <button
                onClick={() => onPageChange(p)}
                className={cn(
                  'h-8 w-8 rounded-lg text-sm font-medium',
                  p === page
                    ? 'bg-blue-600 text-white'
                    : 'hover:bg-gray-100 text-gray-700'
                )}
              >
                {p}
              </button>
            </span>
          )
        })}
        <button
          disabled={page === totalPages}
          onClick={() => onPageChange(page + 1)}
          className="rounded-lg p-1.5 hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
        >
          <ChevronRight size={16} />
        </button>
      </div>
    </div>
  )
}

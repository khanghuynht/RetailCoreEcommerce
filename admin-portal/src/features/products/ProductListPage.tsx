import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Pencil, Trash2, ImageOff, Images, Boxes } from 'lucide-react'
import { productApi } from '@/api/productApi'
import type { Product, ProductListItem } from '@/types/product'
import DataTable, { type Column } from '@/components/shared/DataTable'
import ConfirmDialog from '@/components/shared/ConfirmDialog'
import Pagination from '@/components/shared/Pagination'
import Modal from '@/components/ui/Modal'
import Button from '@/components/ui/Button'
import Badge from '@/components/ui/Badge'
import { formatCurrency, formatDate } from '@/utils/formatters'
import ProductForm, { type ProductFormData } from './ProductForm'
import ProductImageManager from './ProductImageManager'
import InventoryManager from './InventoryManager'
import { useDebounce } from '@/hooks/useDebounce'

const PAGE_SIZE = 10

export default function ProductListPage() {
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebounce(search, 400)
  const [createOpen, setCreateOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<Product | null>(null)
  const [imageTarget, setImageTarget] = useState<ProductListItem | null>(null)
  const [inventoryTarget, setInventoryTarget] = useState<Product | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<ProductListItem | null>(null)

  const { data, isLoading } = useQuery({
    queryKey: ['products', page, debouncedSearch],
    queryFn: () =>
      productApi.getPaged({ pageNumber: page, pageSize: PAGE_SIZE, name: debouncedSearch || undefined }),
    placeholderData: (prev) => prev,
  })

  /** Full detail query used by image manager */
  const { data: editDetail } = useQuery({
    queryKey: ['product', editTarget?.id],
    queryFn: () => productApi.getById(editTarget!.id),
    enabled: !!editTarget,
  })

  const { data: imageDetail } = useQuery({
    queryKey: ['product', imageTarget?.id],
    queryFn: () => productApi.getById(imageTarget!.id),
    enabled: !!imageTarget,
  })

  const createMutation = useMutation({
    mutationFn: (payload: ProductFormData) =>
      productApi.create({
        categoryId: payload.categoryId,
        title: payload.title,
        name: payload.name,
        sku: payload.sku,
        originalPrice: payload.originalPrice,
        salePrice: payload.salePrice,
        description: payload.description,
        length: payload.length ?? 0,
        width: payload.width ?? 0,
        height: payload.height ?? 0,
        initialStock: payload.initialStock ?? 0,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      setCreateOpen(false)
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: ProductFormData }) =>
      productApi.update(id, {
        categoryId: payload.categoryId,
        title: payload.title,
        name: payload.name,
        originalPrice: payload.originalPrice,
        salePrice: payload.salePrice,
        description: payload.description,
        length: payload.length,
        width: payload.width,
        height: payload.height,
        isActive: payload.isActive,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      setEditTarget(null)
    },
  })

  const deleteMutation = useMutation({
    mutationFn: productApi.remove,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      setDeleteTarget(null)
    },
  })

  const columns: Column<ProductListItem>[] = [
    {
      key: 'thumbnailUrl',
      header: '',
      className: 'w-14',
      render: (row) =>
        row.thumbnailUrl ? (
          <img
            src={row.thumbnailUrl}
            alt={row.title}
            className="h-10 w-10 rounded-lg object-cover border border-gray-100"
          />
        ) : (
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-gray-100">
            <ImageOff size={16} className="text-gray-400" />
          </div>
        ),
    },
    {
      key: 'title',
      header: 'Title',
      sortable: true,
      render: (row) => (
        <div>
          <p className="font-medium text-gray-900">{row.title}</p>
          <p className="text-xs text-gray-400 font-mono">{row.sku}</p>
        </div>
      ),
    },
    {
      key: 'originalPrice',
      header: 'Price',
      sortable: true,
      render: (row) => (
        <div>
          <p className="font-medium text-gray-900">{formatCurrency(row.originalPrice)}</p>
          {row.salePrice != null && (
            <p className="text-xs text-green-600">{formatCurrency(row.salePrice)} sale</p>
          )}
        </div>
      ),
    },
    {
      key: 'isActive',
      header: 'Status',
      render: (row) => (
        <Badge variant={row.isActive ? 'green' : 'gray'}>
          {row.isActive ? 'Active' : 'Inactive'}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: 'Actions',
      className: 'w-44 text-right',
      render: (row) => (
        <div className="flex items-center justify-end gap-1">
          <button
            onClick={(e) => { e.stopPropagation(); setImageTarget(row) }}
            className="rounded-lg p-1.5 text-gray-400 hover:bg-violet-50 hover:text-violet-600 transition-colors"
            title="Manage Images"
          >
            <Images size={15} />
          </button>
          <button
            onClick={async (e) => {
              e.stopPropagation()
              const detail = await productApi.getById(row.id)
              setInventoryTarget(detail)
            }}
            className="rounded-lg p-1.5 text-gray-400 hover:bg-emerald-50 hover:text-emerald-600 transition-colors"
            title="Manage Inventory"
          >
            <Boxes size={15} />
          </button>
          <button
            onClick={async (e) => {
              e.stopPropagation()
              const detail = await productApi.getById(row.id)
              setEditTarget(detail)
            }}
            className="rounded-lg p-1.5 text-gray-400 hover:bg-blue-50 hover:text-blue-600 transition-colors"
            title="Edit"
          >
            <Pencil size={15} />
          </button>
          <button
            onClick={(e) => { e.stopPropagation(); setDeleteTarget(row) }}
            className="rounded-lg p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-600 transition-colors"
            title="Delete"
          >
            <Trash2 size={15} />
          </button>
        </div>
      ),
    },
  ]

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-base font-semibold text-gray-800">All Products</h2>
          <p className="text-sm text-gray-500">{data?.totalItems ?? 0} total</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus size={16} /> New Product
        </Button>
      </div>

      <input
        type="text"
        placeholder="Search by name…"
        value={search}
        onChange={(e) => { setSearch(e.target.value); setPage(1) }}
        className="w-full max-w-xs rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
      />

      <DataTable
        columns={columns}
        data={data?.items ?? []}
        keyField="id"
        loading={isLoading}
        emptyText="No products found."
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

      {/* Create Modal */}
      <Modal open={createOpen} onClose={() => setCreateOpen(false)} title="New Product" size="lg">
        <ProductForm
          onCancel={() => setCreateOpen(false)}
          onSubmit={(payload) => createMutation.mutateAsync(payload)}
        />
      </Modal>

      {/* Edit Modal */}
      <Modal
        open={!!editTarget}
        onClose={() => setEditTarget(null)}
        title="Edit Product"
        size="lg"
      >
        {editTarget && (
          <ProductForm
            defaultValues={editTarget}
            isEdit
            onCancel={() => setEditTarget(null)}
            onSubmit={(payload) =>
              updateMutation.mutateAsync({ id: editTarget.id, payload })
            }
          />
        )}
      </Modal>

      {/* Image Manager Modal */}
      <Modal
        open={!!imageTarget}
        onClose={() => setImageTarget(null)}
        title="Manage Images"
        size="lg"
      >
        {imageTarget && imageDetail && (
          <ProductImageManager
            productId={imageTarget.id}
            thumbnailUrl={imageDetail.thumbnailUrl}
            images={imageDetail.images}
          />
        )}
      </Modal>

      {/* Inventory Modal */}
      <Modal
        open={!!inventoryTarget}
        onClose={() => setInventoryTarget(null)}
        title={`Inventory — ${inventoryTarget?.title ?? ''}`}
        size="md"
      >
        {inventoryTarget && (
          <InventoryManager product={inventoryTarget} />
        )}
      </Modal>

      {/* Delete Confirm */}
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}
        message={`Delete product "${deleteTarget?.title}"? This cannot be undone.`}
        loading={deleteMutation.isPending}
      />
    </div>
  )
}

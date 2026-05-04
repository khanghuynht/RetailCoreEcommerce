import { useForm, Controller } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import Input from '@/components/ui/Input'
import Textarea from '@/components/ui/Textarea'
import Select from '@/components/ui/Select'
import Button from '@/components/ui/Button'
import { categoryApi } from '@/api/categoryApi'
import type { Product } from '@/types/product'

const schema = z.object({
  title: z.string().min(1, 'Title is required').max(200),
  name: z.string().min(1, 'Name is required').max(200),
  sku: z.string().min(1, 'SKU is required'),
  categoryId: z.string().min(1, 'Category is required'),
  description: z.string().optional(),
  originalPrice: z.coerce.number().min(0, 'Price must be ≥ 0'),
  salePrice: z.coerce.number().min(0).optional().or(z.literal('')).transform((v) => v === '' ? undefined : v as number | undefined),
  length: z.coerce.number().min(0).default(0),
  width: z.coerce.number().min(0).default(0),
  height: z.coerce.number().min(0).default(0),
  initialStock: z.coerce.number().min(0).default(0),
  isActive: z.boolean().optional(),
})

// Input type = raw HTML form values (z.coerce fields accept unknown/string)
type ProductFormInput = z.input<typeof schema>
// Output type = validated + transformed values (numbers are numbers)
export type ProductFormData = z.output<typeof schema>

interface ProductFormProps {
  defaultValues?: Partial<Product>
  isEdit?: boolean
  onSubmit: (data: ProductFormData) => Promise<unknown>
  onCancel: () => void
}

export default function ProductForm({ defaultValues, isEdit = false, onSubmit, onCancel }: ProductFormProps) {
  const [showDimensions, setShowDimensions] = useState(false)

  const { data: categoriesData } = useQuery({
    queryKey: ['categories', 1, ''],
    queryFn: () => categoryApi.getPaged({ pageNumber: 1, pageSize: 50 }),
  })

  const categories = categoriesData?.items ?? []

  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ProductFormInput, unknown, ProductFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      title: defaultValues?.title ?? '',
      name: defaultValues?.name ?? '',
      sku: defaultValues?.sku ?? '',
      categoryId: defaultValues?.categoryId ?? '',
      description: defaultValues?.description ?? '',
      originalPrice: defaultValues?.originalPrice ?? 0,
      salePrice: defaultValues?.salePrice ?? undefined,
      length: defaultValues?.length ?? 0,
      width: defaultValues?.width ?? 0,
      height: defaultValues?.height ?? 0,
      initialStock: 0,
      isActive: defaultValues?.isActive ?? true,
    },
  })

  const categoryOptions = categories.map((c) => ({ value: c.id, label: c.name }))

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid grid-cols-2 gap-3">
        <Input
          label="Title"
          placeholder="Product display title"
          error={errors.title?.message}
          {...register('title')}
        />
        <Input
          label="Name"
          placeholder="Internal product name"
          error={errors.name?.message}
          {...register('name')}
        />
      </div>

      <div className="grid grid-cols-2 gap-3">
        <Input
          label="SKU"
          placeholder="e.g. WH-1000XM5"
          error={errors.sku?.message}
          {...register('sku')}
        />
        <Controller
          name="categoryId"
          control={control}
          render={({ field }) => (
            <Select
              label="Category"
              options={categoryOptions}
              placeholder="Select category…"
              error={errors.categoryId?.message}
              value={field.value || ''}
              onChange={field.onChange}
            />
          )}
        />
      </div>

      <Textarea
        label="Description"
        placeholder="Describe the product…"
        error={errors.description?.message}
        {...register('description')}
      />

      <div className="grid grid-cols-2 gap-3">
        <Input
          label="Original Price (USD)"
          type="number"
          step="0.01"
          min="0"
          placeholder="0.00"
          error={errors.originalPrice?.message}
          {...register('originalPrice')}
        />
        <Input
          label="Sale Price (USD, optional)"
          type="number"
          step="0.01"
          min="0"
          placeholder="0.00"
          error={errors.salePrice?.message}
          {...register('salePrice')}
        />
      </div>

      {!isEdit && (
        <Input
          label="Initial Stock"
          type="number"
          min="0"
          placeholder="0"
          error={errors.initialStock?.message}
          {...register('initialStock')}
        />
      )}

      <div>
        <button
          type="button"
          onClick={() => setShowDimensions((v) => !v)}
          className="text-sm text-blue-600 hover:underline"
        >
          {showDimensions ? '− Hide' : '+ Set'} dimensions (L × W × H)
        </button>
        {showDimensions && (
          <div className="mt-3 grid grid-cols-3 gap-3">
            <Input label="Length (cm)" type="number" min="0" {...register('length')} />
            <Input label="Width (cm)" type="number" min="0" {...register('width')} />
            <Input label="Height (cm)" type="number" min="0" {...register('height')} />
          </div>
        )}
      </div>

      {isEdit && (
        <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
          <input type="checkbox" className="rounded" {...register('isActive')} />
          Active (visible to customers)
        </label>
      )}

      <div className="flex gap-3 pt-2">
        <Button type="button" variant="secondary" className="flex-1" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" className="flex-1" loading={isSubmitting}>
          {isEdit ? 'Save Changes' : 'Create Product'}
        </Button>
      </div>
    </form>
  )
}

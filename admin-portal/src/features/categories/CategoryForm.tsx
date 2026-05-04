import { useForm, Controller } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useQuery } from '@tanstack/react-query'
import Input from '@/components/ui/Input'
import Textarea from '@/components/ui/Textarea'
import Select from '@/components/ui/Select'
import Button from '@/components/ui/Button'
import { categoryApi } from '@/api/categoryApi'
import type { Category } from '@/types/category'

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  description: z.string().optional(),
  parentId: z.string().optional(),
})

export type CategoryFormData = z.infer<typeof schema>

interface CategoryFormProps {
  defaultValues?: Partial<Category>
  onSubmit: (data: CategoryFormData) => Promise<unknown>
  onCancel: () => void
}

export default function CategoryForm({ defaultValues, onSubmit, onCancel }: CategoryFormProps) {
  /** Load root categories to use as parent options */
  const { data: rootData } = useQuery({
    queryKey: ['categories', 'roots'],
    queryFn: () => categoryApi.getPaged({ pageNumber: 1, pageSize: 100, isRootOnly: true }),
  })

  const rootOptions = (rootData?.items ?? [])
    // Prevent a category from being its own parent
    .filter((c) => c.id !== defaultValues?.id)
    .map((c) => ({ value: c.id, label: c.name }))

  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CategoryFormData, unknown, CategoryFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: defaultValues?.name ?? '',
      description: defaultValues?.description ?? '',
      parentId: defaultValues?.parentId ?? '',
    },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input
        label="Name"
        placeholder="e.g. Smartphones"
        error={errors.name?.message}
        {...register('name')}
      />

      <Textarea
        label="Description"
        placeholder="Brief description of the category…"
        error={errors.description?.message}
        {...register('description')}
      />

      <Controller
        name="parentId"
        control={control}
        render={({ field }) => (
          <Select
            label="Parent Category"
            options={rootOptions}
            placeholder="None (root category)"
            error={errors.parentId?.message}
            value={field.value ?? ''}
            onChange={field.onChange}
          />
        )}
      />
      <p className="text-xs text-gray-400 -mt-2">
        Leave blank to create a root category. Select a parent to create a sub-category.
      </p>

      <div className="flex gap-3 pt-2">
        <Button type="button" variant="secondary" className="flex-1" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" className="flex-1" loading={isSubmitting}>
          {defaultValues?.id ? 'Save Changes' : 'Create Category'}
        </Button>
      </div>
    </form>
  )
}

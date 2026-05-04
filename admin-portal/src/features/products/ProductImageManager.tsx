import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Trash2, Upload, ImageOff } from 'lucide-react'
import { productApi } from '@/api/productApi'
import type { ProductImage } from '@/types/product'
import Button from '@/components/ui/Button'

interface ProductImageManagerProps {
  productId: string
  thumbnailUrl?: string
  images: ProductImage[]
}

export default function ProductImageManager({
  productId,
  thumbnailUrl,
  images,
}: ProductImageManagerProps) {
  const queryClient = useQueryClient()
  const [uploading, setUploading] = useState(false)

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['products'] })
    queryClient.invalidateQueries({ queryKey: ['product', productId] })
  }

  const thumbnailMutation = useMutation({
    mutationFn: (file: File) => productApi.uploadThumbnail(productId, file),
    onSuccess: invalidate,
  })

  const addImageMutation = useMutation({
    mutationFn: (file: File) => productApi.addImage(productId, file),
    onSuccess: invalidate,
  })

  const deleteImageMutation = useMutation({
    mutationFn: (imageId: string) => productApi.deleteImage(productId, imageId),
    onSuccess: invalidate,
  })

  const handleFileUpload = async (
    e: React.ChangeEvent<HTMLInputElement>,
    type: 'thumbnail' | 'gallery'
  ) => {
    const file = e.target.files?.[0]
    if (!file) return
    setUploading(true)
    try {
      if (type === 'thumbnail') {
        await thumbnailMutation.mutateAsync(file)
      } else {
        await addImageMutation.mutateAsync(file)
      }
    } finally {
      setUploading(false)
      e.target.value = ''
    }
  }

  return (
    <div className="space-y-5 pt-2">
      {/* Thumbnail */}
      <div>
        <p className="text-sm font-medium text-gray-700 mb-2">Thumbnail</p>
        <div className="flex items-center gap-4">
          <div className="flex h-20 w-20 items-center justify-center rounded-xl border border-gray-200 overflow-hidden bg-gray-50">
            {thumbnailUrl ? (
              <img src={thumbnailUrl} alt="thumbnail" className="h-full w-full object-cover" />
            ) : (
              <ImageOff size={24} className="text-gray-300" />
            )}
          </div>
          <label className="cursor-pointer">
            <span className="inline-flex items-center gap-2 rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 transition-colors">
              <Upload size={14} />
              {thumbnailUrl ? 'Replace' : 'Upload'} Thumbnail
            </span>
            <input
              type="file"
              accept="image/*"
              className="hidden"
              disabled={uploading}
              onChange={(e) => handleFileUpload(e, 'thumbnail')}
            />
          </label>
        </div>
      </div>

      {/* Gallery */}
      <div>
        <p className="text-sm font-medium text-gray-700 mb-2">
          Gallery Images ({images.length})
        </p>
        <div className="flex flex-wrap gap-3">
          {images.map((img) => (
            <div
              key={img.id}
              className="relative h-20 w-20 rounded-xl overflow-hidden border border-gray-200 group"
            >
              <img src={img.imageUrl} alt={img.name ?? ''} className="h-full w-full object-cover" />
              <button
                type="button"
                onClick={() => deleteImageMutation.mutate(img.id)}
                disabled={deleteImageMutation.isPending}
                className="absolute inset-0 flex items-center justify-center bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity"
              >
                <Trash2 size={16} className="text-white" />
              </button>
            </div>
          ))}

          {/* Add image */}
          <label className="flex h-20 w-20 cursor-pointer items-center justify-center rounded-xl border-2 border-dashed border-gray-300 hover:border-blue-400 hover:bg-blue-50 transition-colors">
            <Upload size={18} className="text-gray-400" />
            <input
              type="file"
              accept="image/*"
              className="hidden"
              disabled={uploading}
              onChange={(e) => handleFileUpload(e, 'gallery')}
            />
          </label>
        </div>
      </div>

      {uploading && (
        <p className="text-xs text-blue-600 flex items-center gap-1">
          <svg className="animate-spin h-3 w-3" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
          </svg>
          Uploading…
        </p>
      )}
    </div>
  )
}

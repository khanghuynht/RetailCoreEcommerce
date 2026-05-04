import { useRef, useState } from 'react'
import { Upload, X } from 'lucide-react'
import { cn } from '@/utils/formatters'

interface ImageUploadProps {
  value: File[]
  onChange: (files: File[]) => void
  existingUrls?: string[]
  onRemoveExisting?: (url: string) => void
  error?: string
  maxFiles?: number
}

export default function ImageUpload({
  value,
  onChange,
  existingUrls = [],
  onRemoveExisting,
  error,
  maxFiles = 5,
}: ImageUploadProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [dragging, setDragging] = useState(false)

  const addFiles = (incoming: FileList | null) => {
    if (!incoming) return
    const next = [...value, ...Array.from(incoming)].slice(0, maxFiles - existingUrls.length)
    onChange(next)
  }

  const removeNew = (index: number) => {
    onChange(value.filter((_, i) => i !== index))
  }

  return (
    <div className="flex flex-col gap-2">
      <div
        onDragOver={(e) => { e.preventDefault(); setDragging(true) }}
        onDragLeave={() => setDragging(false)}
        onDrop={(e) => { e.preventDefault(); setDragging(false); addFiles(e.dataTransfer.files) }}
        onClick={() => inputRef.current?.click()}
        className={cn(
          'flex flex-col items-center justify-center rounded-xl border-2 border-dashed',
          'cursor-pointer p-6 text-sm text-gray-500 transition-colors',
          dragging ? 'border-blue-400 bg-blue-50' : 'border-gray-300 hover:border-blue-400 hover:bg-gray-50',
          error && 'border-red-400'
        )}
      >
        <Upload size={24} className="mb-2 text-gray-400" />
        <span>Drop images here or <span className="text-blue-600 font-medium">browse</span></span>
        <span className="text-xs text-gray-400 mt-1">PNG, JPG, WEBP up to 5MB each</span>
        <input
          ref={inputRef}
          type="file"
          accept="image/*"
          multiple
          className="hidden"
          onChange={(e) => addFiles(e.target.files)}
        />
      </div>

      {(existingUrls.length > 0 || value.length > 0) && (
        <div className="flex flex-wrap gap-2">
          {existingUrls.map((url) => (
            <div key={url} className="relative h-20 w-20 rounded-lg overflow-hidden border border-gray-200">
              <img src={url} alt="" className="h-full w-full object-cover" />
              {onRemoveExisting && (
                <button
                  type="button"
                  onClick={() => onRemoveExisting(url)}
                  className="absolute top-0.5 right-0.5 flex h-5 w-5 items-center justify-center rounded-full bg-red-500 text-white shadow"
                >
                  <X size={10} />
                </button>
              )}
            </div>
          ))}
          {value.map((file, i) => (
            <div key={i} className="relative h-20 w-20 rounded-lg overflow-hidden border border-gray-200">
              <img src={URL.createObjectURL(file)} alt="" className="h-full w-full object-cover" />
              <button
                type="button"
                onClick={() => removeNew(i)}
                className="absolute top-0.5 right-0.5 flex h-5 w-5 items-center justify-center rounded-full bg-red-500 text-white shadow"
              >
                <X size={10} />
              </button>
            </div>
          ))}
        </div>
      )}
      {error && <span className="text-xs text-red-500">{error}</span>}
    </div>
  )
}

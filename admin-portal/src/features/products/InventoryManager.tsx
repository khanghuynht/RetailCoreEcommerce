import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Package, ShoppingCart, Lock, PlusCircle } from "lucide-react";
import { productApi } from "@/api/productApi";
import type { Product } from "@/types/product";
import Button from "@/components/ui/Button";
import Input from "@/components/ui/Input";

interface StatTileProps {
  label: string;
  value: number;
  icon: React.ReactNode;
  colorClass: string;
}

function StatTile({ label, value, icon, colorClass }: StatTileProps) {
  return (
    <div className={`flex flex-col gap-1 rounded-xl p-4 ${colorClass}`}>
      <div className="flex items-center gap-2 text-sm font-medium opacity-80">
        {icon}
        {label}
      </div>
      <p className="text-2xl font-bold">{value}</p>
    </div>
  );
}

const schema = z.object({
  stockQuantity: z
    .number({ message: "Must be a number" })
    .refine((n) => !Number.isNaN(n), { message: "Must be a number" })
    .int({ message: "Must be a whole number" })
    .min(0, "Cannot be negative"),
});
type FormData = z.infer<typeof schema>;

interface InventoryManagerProps {
  product: Product;
}

export default function InventoryManager({ product }: InventoryManagerProps) {
  const queryClient = useQueryClient();
  const [editOpen, setEditOpen] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { stockQuantity: product.stockQuantity },
  });

  const updateMutation = useMutation({
    mutationFn: (payload: FormData) =>
      productApi.updateInventory(product.id, {
        stockQuantity: payload.stockQuantity,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["products"] });
      queryClient.invalidateQueries({ queryKey: ["product", product.id] });
      setEditOpen(false);
    },
  });

  const handleOpen = () => {
    reset({ stockQuantity: product.stockQuantity });
    setEditOpen(true);
  };

  return (
    <div className="space-y-5 pt-1">
      {/* Stats grid */}
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
        <StatTile
          label="Total Stock"
          value={product.stockQuantity}
          icon={<Package size={14} />}
          colorClass="bg-blue-50 text-blue-700"
        />
        <StatTile
          label="Reserved"
          value={product.reservedQuantity}
          icon={<Lock size={14} />}
          colorClass="bg-yellow-50 text-yellow-700"
        />
        <StatTile
          label="Sold"
          value={product.soldQuantity}
          icon={<ShoppingCart size={14} />}
          colorClass="bg-violet-50 text-violet-700"
        />
      </div>

      {/* Update stock */}
      {!editOpen ? (
        <Button variant="secondary" onClick={handleOpen} size="sm">
          <PlusCircle size={15} /> Update Stock Quantity
        </Button>
      ) : (
        <form
          onSubmit={handleSubmit((data) => updateMutation.mutateAsync(data))}
          className="flex items-end gap-3 rounded-xl border border-blue-100 bg-blue-50/50 p-4"
        >
          <div className="flex-1">
            <Input
              label="New Stock Quantity"
              type="number"
              min="0"
              error={errors.stockQuantity?.message}
              {...register("stockQuantity", { valueAsNumber: true })}
            />
          </div>
          <Button type="submit" loading={isSubmitting} size="md">
            Save
          </Button>
          <Button
            type="button"
            variant="ghost"
            size="md"
            onClick={() => setEditOpen(false)}
            disabled={isSubmitting}
          >
            Cancel
          </Button>
        </form>
      )}
    </div>
  );
}

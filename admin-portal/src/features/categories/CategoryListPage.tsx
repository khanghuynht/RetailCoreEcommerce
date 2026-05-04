import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  Plus,
  Pencil,
  Trash2,
  Search,
  ChevronRight,
  FolderOpen,
  Folder,
  Loader2,
} from "lucide-react";
import { categoryApi } from "@/api/categoryApi";
import type { Category } from "@/types/category";
import ConfirmDialog from "@/components/shared/ConfirmDialog";
import Pagination from "@/components/shared/Pagination";
import Modal from "@/components/ui/Modal";
import Button from "@/components/ui/Button";
import Badge from "@/components/ui/Badge";
import CategoryForm, { type CategoryFormData } from "./CategoryForm";
import { useDebounce } from "@/hooks/useDebounce";
import { cn } from "@/utils/formatters";

const PAGE_SIZE = 10;
type ViewMode = "all" | "roots" | "sub";

/** Lazily fetches and renders children for one expanded root row */
function ChildRows({
  rootId,
  onEdit,
  onDelete,
}: {
  rootId: string;
  onEdit: (c: Category) => void;
  onDelete: (c: Category) => void;
}) {
  const { data, isLoading } = useQuery({
    queryKey: ["categories", "children", rootId],
    queryFn: () =>
      categoryApi.getPaged({ pageNumber: 1, pageSize: 50, parentId: rootId }),
  });

  if (isLoading) {
    return (
      <tr className="bg-gray-50/60">
        <td
          colSpan={4}
          className="pl-14 py-2 text-xs text-gray-400 flex items-center gap-1"
        >
          <Loader2 size={12} className="animate-spin" /> Loading…
        </td>
      </tr>
    );
  }

  return (
    <>
      {(data?.items ?? []).map((child) => (
        <tr
          key={child.id}
          className="border-b border-gray-50 last:border-0 bg-gray-50/40"
        >
          <td className="px-4 py-2.5">
            <div className="flex items-center gap-2 pl-8">
              <ChevronRight size={13} className="text-gray-300 shrink-0" />
              <span className="text-gray-700 text-sm">{child.name}</span>
            </div>
          </td>
          <td className="px-4 py-2.5 text-gray-400 text-sm">
            {child.description || "—"}
          </td>
          <td className="px-4 py-2.5">
            <Badge variant="gray">Sub-category</Badge>
          </td>
          <td className="px-4 py-2.5 text-right">
            <div className="flex items-center justify-end gap-1">
              <button
                onClick={() => onEdit(child)}
                className="rounded-lg p-1.5 text-gray-400 hover:bg-blue-50 hover:text-blue-600 transition-colors"
                title="Edit"
              >
                <Pencil size={14} />
              </button>
              <button
                onClick={() => onDelete(child)}
                className="rounded-lg p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-600 transition-colors"
                title="Delete"
              >
                <Trash2 size={14} />
              </button>
            </div>
          </td>
        </tr>
      ))}
    </>
  );
}

export default function CategoryListPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [viewMode, setViewMode] = useState<ViewMode>("all");
  const [expandedRoots, setExpandedRoots] = useState<Set<string>>(new Set());
  const debouncedSearch = useDebounce(search, 400);
  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<Category | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Category | null>(null);

  const isTreeMode = viewMode === "all" && !debouncedSearch;

  /* ── Tree mode: paginated root-only query ── */
  const { data: treeData, isLoading: treeLoading } = useQuery({
    queryKey: ["categories", "tree", page],
    queryFn: () =>
      categoryApi.getPaged({
        pageNumber: page,
        pageSize: PAGE_SIZE,
        isRootOnly: true,
      }),
    enabled: isTreeMode,
    placeholderData: (prev) => prev,
  });

  /* ── Flat mode: search / roots-only / sub-only ── */
  const { data: flatData, isLoading: flatLoading } = useQuery({
    queryKey: ["categories", "flat", page, debouncedSearch, viewMode],
    queryFn: () =>
      categoryApi.getPaged({
        pageNumber: page,
        pageSize: PAGE_SIZE,
        name: debouncedSearch || undefined,
        isRootOnly: viewMode === "roots" ? true : undefined,
      }),
    enabled: !isTreeMode,
    placeholderData: (prev) => prev,
  });

  const data = isTreeMode ? treeData : flatData;
  const isLoading = isTreeMode ? treeLoading : flatLoading;

  const handleSearch = (val: string) => {
    setSearch(val);
    setPage(1);
  };
  const handleViewMode = (mode: ViewMode) => {
    setViewMode(mode);
    setPage(1);
  };
  const toggleExpand = (id: string) =>
    setExpandedRoots((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: ["categories"] });

  const createMutation = useMutation({
    mutationFn: (fd: CategoryFormData) =>
      categoryApi.create({
        name: fd.name,
        description: fd.description,
        parentId: fd.parentId || undefined,
      }),
    onSuccess: () => {
      invalidate();
      setCreateOpen(false);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, fd }: { id: string; fd: CategoryFormData }) =>
      categoryApi.update(id, {
        name: fd.name,
        description: fd.description,
        parentId: fd.parentId || undefined,
      }),
    onSuccess: () => {
      invalidate();
      setEditTarget(null);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: categoryApi.remove,
    onSuccess: () => {
      invalidate();
      setDeleteTarget(null);
    },
  });

  const RootActionButtons = ({ row }: { row: Category }) => (
    <div className="flex items-center justify-end gap-1">
      <button
        onClick={() => setEditTarget(row)}
        className="rounded-lg p-1.5 text-gray-400 hover:bg-blue-50 hover:text-blue-600 transition-colors"
        title="Edit"
      >
        <Pencil size={15} />
      </button>
      <button
        onClick={() => setDeleteTarget(row)}
        className="rounded-lg p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-600 transition-colors"
        title="Delete"
      >
        <Trash2 size={15} />
      </button>
    </div>
  );

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-base font-semibold text-gray-800">
            All Categories
          </h2>
          <p className="text-sm text-gray-500">{data?.totalItems ?? 0} total</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus size={16} /> New Category
        </Button>
      </div>

      {/* Toolbar */}
      <div className="flex flex-wrap items-center gap-3">
        <div className="relative w-full max-w-xs">
          <Search
            size={15}
            className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"
          />
          <input
            type="text"
            placeholder="Search by name…"
            value={search}
            onChange={(e) => handleSearch(e.target.value)}
            className="w-full rounded-lg border border-gray-300 py-2 pl-9 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div className="flex rounded-lg border border-gray-200 overflow-hidden text-sm">
          {(["all", "roots", "sub"] as ViewMode[]).map((mode) => (
            <button
              key={mode}
              onClick={() => handleViewMode(mode)}
              className={cn(
                "px-3 py-1.5 transition-colors",
                viewMode === mode
                  ? "bg-blue-600 text-white"
                  : "bg-white text-gray-600 hover:bg-gray-50",
              )}
            >
              {mode === "all"
                ? "Tree View"
                : mode === "roots"
                  ? "Root only"
                  : "Sub-categories"}
            </button>
          ))}
        </div>
      </div>

      {/* Table */}
      <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white shadow-sm">
        <table className="min-w-full text-sm">
          <thead>
            <tr className="border-b border-gray-100 bg-gray-50">
              <th className="px-4 py-3 text-left font-semibold text-gray-600">
                Name
              </th>
              <th className="px-4 py-3 text-left font-semibold text-gray-600">
                Description
              </th>
              <th className="px-4 py-3 text-left font-semibold text-gray-600">
                Level
              </th>
              <th className="px-4 py-3 text-right font-semibold text-gray-600 w-28">
                Actions
              </th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr>
                <td colSpan={4} className="py-12 text-center text-gray-400">
                  <div className="flex items-center justify-center gap-2">
                    <Loader2 size={16} className="animate-spin text-blue-500" />{" "}
                    Loading…
                  </div>
                </td>
              </tr>
            ) : (data?.items ?? []).length === 0 ? (
              <tr>
                <td colSpan={4} className="py-12 text-center text-gray-400">
                  No categories found.
                </td>
              </tr>
            ) : isTreeMode ? (
              /* ── Tree rows: roots + lazy children ── */
              (treeData?.items ?? []).map((root) => {
                const isExpanded = expandedRoots.has(root.id);
                return (
                  <>
                    <tr
                      key={root.id}
                      className="border-b border-gray-50 hover:bg-blue-50/30 transition-colors"
                    >
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => toggleExpand(root.id)}
                            className="text-gray-400 hover:text-blue-600 transition-colors"
                            title={
                              isExpanded ? "Collapse" : "Expand sub-categories"
                            }
                          >
                            {isExpanded ? (
                              <FolderOpen size={16} className="text-blue-500" />
                            ) : (
                              <Folder size={16} />
                            )}
                          </button>
                          <span className="font-medium text-gray-900">
                            {root.name}
                          </span>
                        </div>
                      </td>
                      <td className="px-4 py-3 text-gray-500">
                        {root.description || "—"}
                      </td>
                      <td className="px-4 py-3">
                        <Badge variant="blue">Root</Badge>
                      </td>
                      <td className="px-4 py-3 text-right">
                        <RootActionButtons row={root} />
                      </td>
                    </tr>
                    {isExpanded && (
                      <ChildRows
                        key={`children-${root.id}`}
                        rootId={root.id}
                        onEdit={setEditTarget}
                        onDelete={setDeleteTarget}
                      />
                    )}
                  </>
                );
              })
            ) : (
              /* ── Flat rows ── */
              (flatData?.items ?? []).map((row) => (
                <tr
                  key={row.id}
                  className="border-b border-gray-50 last:border-0 hover:bg-blue-50/30 transition-colors"
                >
                  <td className="px-4 py-3 font-medium text-gray-900">
                    {row.name}
                  </td>
                  <td className="px-4 py-3 text-gray-500">
                    {row.description || "—"}
                  </td>
                  <td className="px-4 py-3">
                    <Badge variant={row.parentId ? "gray" : "blue"}>
                      {row.parentId ? "Sub-category" : "Root"}
                    </Badge>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <RootActionButtons row={row} />
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination — always visible when there is data */}
      {data && data.totalItems > 0 && (
        <Pagination
          page={page}
          totalPages={data.totalPages}
          total={data.totalItems}
          pageSize={PAGE_SIZE}
          onPageChange={(p) => {
            setPage(p);
            setExpandedRoots(new Set());
          }}
        />
      )}

      {/* Create Modal */}
      <Modal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        title="New Category"
      >
        <CategoryForm
          onCancel={() => setCreateOpen(false)}
          onSubmit={(fd) => createMutation.mutateAsync(fd)}
        />
      </Modal>

      {/* Edit Modal */}
      <Modal
        open={!!editTarget}
        onClose={() => setEditTarget(null)}
        title="Edit Category"
      >
        {editTarget && (
          <CategoryForm
            defaultValues={editTarget}
            onCancel={() => setEditTarget(null)}
            onSubmit={(fd) =>
              updateMutation.mutateAsync({ id: editTarget.id, fd })
            }
          />
        )}
      </Modal>

      {/* Delete Confirm */}
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}
        message={`Delete category "${deleteTarget?.name}"? This cannot be undone.`}
        loading={deleteMutation.isPending}
      />
    </div>
  );
}

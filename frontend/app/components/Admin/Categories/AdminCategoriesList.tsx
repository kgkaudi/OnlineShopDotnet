"use client";

import { Category } from "@/src/lib/api";
import AdminCategoryRow from "./AdminCategoryRow";

interface Props {
  categories: Category[];
  loading: boolean;
  editingCategoryId: string | null;
  savingId: string | null;
  deletingId: string | null;
  editingName: string;
  onStartEdit: (category: Category) => void;
  onChangeName: (value: string) => void;
  onSave: (categoryId: string) => void;
  onCancel: () => void;
  onDelete: (category: Category) => void;
}

export default function AdminCategoriesList({
  categories,
  loading,
  editingCategoryId,
  savingId,
  deletingId,
  editingName,
  onStartEdit,
  onChangeName,
  onSave,
  onCancel,
  onDelete,
}: Props) {
  if (loading) {
    return (
      <div className="space-y-4 p-6">
        {[1, 2, 3, 4].map((item) => (
          <div key={item} className="flex animate-pulse items-center justify-between gap-4">
            <div className="h-5 w-48 rounded bg-gray-200" />
            <div className="h-9 w-32 rounded bg-gray-200" />
          </div>
        ))}
      </div>
    );
  }

  if (categories.length === 0) {
    return (
      <div className="px-6 py-12 text-center">
        <div className="text-4xl">📂</div>
        <h3 className="mt-4 font-semibold text-gray-900">No categories yet</h3>
        <p className="mt-2 text-sm text-gray-500">
          Create your first category using the form above.
        </p>
      </div>
    );
  }

  return (
    <div className="divide-y divide-gray-100">
      {categories.map((category) => {
        const isEditing = editingCategoryId === category.id;
        const isSaving = savingId === category.id;
        const isDeleting = deletingId === category.id;

        return (
          <AdminCategoryRow
            key={category.id}
            category={category}
            isEditing={isEditing}
            isSaving={isSaving}
            isDeleting={isDeleting}
            editingName={editingName}
            onStartEdit={onStartEdit}
            onChangeName={onChangeName}
            onSave={() => onSave(category.id)}
            onCancel={onCancel}
            onDelete={() => onDelete(category)}
          />
        );
      })}
    </div>
  );
}

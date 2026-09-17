"use client";

import { Category } from "@/src/lib/api";

interface Props {
  category: Category;
  isEditing: boolean;
  isSaving: boolean;
  isDeleting: boolean;
  editingName: string;
  onStartEdit: (category: Category) => void;
  onChangeName: (value: string) => void;
  onSave: () => void;
  onCancel: () => void;
  onDelete: () => void;
}

export default function AdminCategoryRow({
  category,
  isEditing,
  isSaving,
  isDeleting,
  editingName,
  onStartEdit,
  onChangeName,
  onSave,
  onCancel,
  onDelete,
}: Props) {
  return (
    <div className="flex flex-col gap-4 px-6 py-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="min-w-0 flex-1">
        {isEditing ? (
          <input
            type="text"
            value={editingName}
            onChange={(e) => onChangeName(e.target.value)}
            maxLength={100}
            autoFocus
            disabled={isSaving}
            className="w-full max-w-md rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-gray-500 focus:ring-2 focus:ring-gray-200 disabled:bg-gray-100"
          />
        ) : (
          <div>
            <p className="font-medium text-gray-900">{category.name}</p>
            <p className="mt-1 truncate text-xs text-gray-400">ID: {category.id}</p>
          </div>
        )}
      </div>

      <div className="flex shrink-0 items-center gap-2">
        {isEditing ? (
          <>
            <button
              type="button"
              onClick={onSave}
              disabled={isSaving || !editingName.trim()}
              className="rounded-lg bg-black px-4 py-2 text-sm font-medium text-white transition hover:bg-gray-800 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {isSaving ? "Saving..." : "Save"}
            </button>

            <button
              type="button"
              onClick={onCancel}
              disabled={isSaving}
              className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Cancel
            </button>
          </>
        ) : (
          <>
            <button
              type="button"
              onClick={() => onStartEdit(category)}
              disabled={isDeleting}
              className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Edit
            </button>

            <button
              type="button"
              onClick={onDelete}
              disabled={isDeleting}
              className="rounded-lg border border-red-200 px-4 py-2 text-sm font-medium text-red-600 transition hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {isDeleting ? "Deleting..." : "Delete"}
            </button>
          </>
        )}
      </div>
    </div>
  );
}

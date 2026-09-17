"use client";

import { Product, Category } from "@/src/lib/api";
import CategoryDropdown from "./CategoryDropdown";

interface Props {
  product: Product;
  editingForm: any;
  categories: Category[];
  isUpdating: boolean;
  isDeleting: boolean;
  onChange: (field: string, value: string) => void;
  onSave: () => void;
  onCancel: () => void;
}

export default function AdminProductsEditRow({
  product,
  editingForm,
  categories,
  isUpdating,
  isDeleting,
  onChange,
  onSave,
  onCancel,
}: Props) {
  return (
    <>
      <td className="px-4 py-4 align-top bg-gray-50">
        <input
          type="text"
          value={editingForm.name}
          onChange={(e) => onChange("name", e.target.value)}
          className="w-full rounded border px-3 py-2"
          disabled={isUpdating}
        />
      </td>

      <td className="px-4 py-4 align-top bg-gray-50">
        <textarea
          value={editingForm.description}
          onChange={(e) => onChange("description", e.target.value)}
          rows={3}
          className="w-full rounded border px-3 py-2"
          disabled={isUpdating}
        />
      </td>

      <td className="px-4 py-4 align-top bg-gray-50">
        <input
          type="number"
          min="0.01"
          step="0.01"
          value={editingForm.price}
          onChange={(e) => onChange("price", e.target.value)}
          className="w-full rounded border px-3 py-2"
          disabled={isUpdating}
        />
      </td>

      <td className="px-4 py-4 align-top bg-gray-50">
        <input
          type="number"
          min="0"
          step="1"
          value={editingForm.stockQuantity}
          onChange={(e) => onChange("stockQuantity", e.target.value)}
          className="w-full rounded border px-3 py-2"
          disabled={isUpdating}
        />
      </td>

      <td className="px-4 py-4 align-top bg-gray-50">
        <CategoryDropdown
          value={editingForm.categoryId}
          categories={categories}
          disabled={isUpdating}
          onChange={(id) => onChange("categoryId", id)}
        />
      </td>

      <td className="px-4 py-4 align-top bg-gray-50">
        <div className="flex flex-col items-end gap-2">
          <button
            type="button"
            disabled={isUpdating || isDeleting}
            onClick={onSave}
            className="w-full px-3 py-2 rounded bg-green-600 text-white hover:bg-green-700 disabled:opacity-50"
          >
            {isUpdating ? "Saving..." : "Save"}
          </button>

          <button
            type="button"
            disabled={isUpdating}
            onClick={onCancel}
            className="w-full px-3 py-2 rounded border bg-white hover:bg-gray-100"
          >
            Cancel
          </button>
        </div>
      </td>
    </>
  );
}

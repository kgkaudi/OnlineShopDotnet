"use client";

import { Product } from "@/src/lib/api";

interface Props {
  product: Product;
  isDeleting: boolean;
  isUpdating: boolean;
  editingProductId: string | null;
  onEdit: (product: Product) => void;
  onDelete: (product: Product) => void;
  getCategoryName: (id?: string | null) => string;
}

export default function AdminProductRow({
  product,
  isDeleting,
  isUpdating,
  editingProductId,
  onEdit,
  onDelete,
  getCategoryName,
}: Props) {
  return (
    <tr className="border-b last:border-b-0">
      <td className="px-4 py-4 font-medium">{product.name}</td>

      <td className="px-4 py-4 max-w-xs">
        <span className="line-clamp-2 text-gray-600">
          {product.description || "-"}
        </span>
      </td>

      <td className="px-4 py-4">{Number(product.price).toFixed(2)}</td>

      <td className="px-4 py-4">{product.stockQuantity ?? 0}</td>

      <td className="px-4 py-4">{getCategoryName(product.categoryId)}</td>

      <td className="px-4 py-4">
        <div className="flex justify-end gap-2">
          <button
            type="button"
            disabled={isDeleting || isUpdating || editingProductId !== null}
            onClick={() => onEdit(product)}
            className="px-3 py-2 rounded bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
          >
            Edit
          </button>

          <button
            type="button"
            disabled={isDeleting || isUpdating}
            onClick={() => onDelete(product)}
            className="px-3 py-2 rounded bg-red-600 text-white hover:bg-red-700 disabled:opacity-50"
          >
            {isDeleting ? "Deleting..." : "Delete"}
          </button>
        </div>
      </td>
    </tr>
  );
}

"use client";

import { Product, Category } from "@/src/lib/api";
import AdminProductsEditRow from "./AdminProductsEditRow";
import AdminProductRow from "./AdminProductRow";

interface Props {
  products: Product[];
  categories: Category[];
  editingProductId: string | null;
  editingForm: any;
  updatingProductId: string | null;
  deletingProductId: string | null;
  onEdit: (product: Product) => void;
  onCancelEdit: () => void;
  onSaveEdit: (product: Product) => void;
  onDelete: (product: Product) => void;
  onChangeEditField: (field: string, value: string) => void;
  getCategoryName: (id?: string | null) => string;
}

export default function AdminProductsTable({
  products,
  categories,
  editingProductId,
  editingForm,
  updatingProductId,
  deletingProductId,
  onEdit,
  onCancelEdit,
  onSaveEdit,
  onDelete,
  onChangeEditField,
  getCategoryName,
}: Props) {
  return (
    <section>
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-semibold">Products</h2>
        <span className="text-sm text-gray-500">Total: {products.length}</span>
      </div>

      <div className="border rounded-lg overflow-hidden bg-white">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-100 border-b">
              <tr>
                <th className="text-left px-4 py-3">Name</th>
                <th className="text-left px-4 py-3">Description</th>
                <th className="text-left px-4 py-3">Price</th>
                <th className="text-left px-4 py-3">Stock</th>
                <th className="text-left px-4 py-3">Category</th>
                <th className="text-right px-4 py-3">Actions</th>
              </tr>
            </thead>

            <tbody>
              {products.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center text-gray-500">
                    No products found.
                  </td>
                </tr>
              ) : (
                products.map((product) => {
                  const isEditing = editingProductId === product.id;
                  const isUpdating = updatingProductId === product.id;
                  const isDeleting = deletingProductId === product.id;

                  if (isEditing) {
                    return (
                      <AdminProductsEditRow
                        key={product.id}
                        product={product}
                        editingForm={editingForm}
                        categories={categories}
                        isUpdating={isUpdating}
                        isDeleting={isDeleting}
                        onChange={onChangeEditField}
                        onSave={() => onSaveEdit(product)}
                        onCancel={onCancelEdit}
                      />
                    );
                  }

                  return (
                    <AdminProductRow
                      key={product.id}
                      product={product}
                      isDeleting={isDeleting}
                      isUpdating={isUpdating}
                      editingProductId={editingProductId}
                      onEdit={onEdit}
                      onDelete={onDelete}
                      getCategoryName={getCategoryName}
                    />
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>
    </section>
  );
}

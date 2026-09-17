"use client";

import { useState } from "react";
import { Product, Category } from "@/src/lib/api";
import AdminProductsEditRow from "./AdminProductsEditRow";
import AdminProductRow from "./AdminProductRow";
import DeleteProductModal from "./DeleteProductModal";

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

  // FIX: replace onDelete with onDeleteRequest
  onDeleteRequest: (product: Product) => void;

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

  // FIX
  onDeleteRequest,

  onChangeEditField,
  getCategoryName,
}: Props) {

  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);

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
                      <tr key={product.id} className="border-b last:border-b-0">
                        <AdminProductsEditRow
                          product={product}
                          editingForm={editingForm}
                          categories={categories}
                          isUpdating={isUpdating}
                          isDeleting={isDeleting}
                          onChange={onChangeEditField}
                          onSave={() => onSaveEdit(product)}
                          onCancel={onCancelEdit}
                        />
                      </tr>
                    );
                  }

                  return (
                    <tr key={product.id} className="border-b last:border-b-0">
                      <AdminProductRow
                        product={product}
                        isDeleting={isDeleting}
                        isUpdating={isUpdating}
                        editingProductId={editingProductId}
                        onEdit={onEdit}

                        // FIX: use onDeleteRequest
                        onDeleteRequest={(p) => setSelectedProduct(p)}

                        getCategoryName={getCategoryName}
                      />
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>

      {selectedProduct && (
        <DeleteProductModal
          productName={selectedProduct.name}
          onConfirm={() => {
            // FIX: call onDeleteRequest instead of onDelete
            onDeleteRequest(selectedProduct);
            setSelectedProduct(null);
          }}
          onCancel={() => setSelectedProduct(null)}
        />
      )}
    </section>
  );
}

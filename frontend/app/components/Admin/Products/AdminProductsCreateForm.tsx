"use client";

import { Category } from "@/src/lib/api";
import CategoryDropdown from "./CategoryDropdown";

interface Props {
  form: any;
  categories: Category[];
  creating: boolean;
  onChange: (field: string, value: string) => void;
  onSubmit: (e: React.FormEvent<HTMLFormElement>) => void;
}

export default function AdminProductsCreateForm({
  form,
  categories,
  creating,
  onChange,
  onSubmit,
}: Props) {
  return (
    <section className="mb-10 rounded-lg border bg-white p-6">
      <h2 className="text-xl font-semibold mb-6">Create Product</h2>

      <form onSubmit={onSubmit} className="space-y-5">
        <div>
          <label className="block text-sm font-medium mb-1">Name</label>
          <input
            type="text"
            value={form.name}
            onChange={(e) => onChange("name", e.target.value)}
            className="w-full rounded border px-3 py-2"
            disabled={creating}
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Description</label>
          <textarea
            value={form.description}
            onChange={(e) => onChange("description", e.target.value)}
            rows={4}
            className="w-full rounded border px-3 py-2"
            disabled={creating}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          <div>
            <label className="block text-sm font-medium mb-1">Price</label>
            <input
              type="number"
              min="0.01"
              step="0.01"
              value={form.price}
              onChange={(e) => onChange("price", e.target.value)}
              className="w-full rounded border px-3 py-2"
              disabled={creating}
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Stock Quantity</label>
            <input
              type="number"
              min="0"
              step="1"
              value={form.stockQuantity}
              onChange={(e) => onChange("stockQuantity", e.target.value)}
              className="w-full rounded border px-3 py-2"
              disabled={creating}
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Category</label>

          <CategoryDropdown
            value={form.categoryId}
            categories={categories}
            disabled={creating}
            onChange={(id) => onChange("categoryId", id)}
          />
        </div>

        <div className="flex justify-end">
          <button
            type="submit"
            disabled={creating}
            className="px-5 py-2.5 rounded bg-black text-white hover:bg-gray-800 disabled:opacity-50"
          >
            {creating ? "Creating..." : "Create Product"}
          </button>
        </div>
      </form>
    </section>
  );
}

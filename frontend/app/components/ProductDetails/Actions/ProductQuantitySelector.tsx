// frontend/app/components/ProductDetails/Actions/ProductQuantitySelector.tsx
"use client";

import { Product } from "@/src/lib/api";

interface Props {
  product: Product;
  quantity: number;
  hasStockLimit: boolean;
  outOfStock: boolean;
  updateQuantity: (value: number) => void;
}

export default function ProductQuantitySelector({
  product,
  quantity,
  hasStockLimit,
  outOfStock,
  updateQuantity,
}: Props) {
  if (outOfStock) return null;

  return (
    <div className="mt-7">
      <label
        htmlFor="product-quantity"
        className="block text-sm font-semibold mb-2"
      >
        Quantity
      </label>

      <div className="flex items-center gap-3">
        <button
          type="button"
          onClick={() => updateQuantity(quantity - 1)}
          disabled={quantity <= 1}
          className="w-12 h-12 border rounded-lg text-xl hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
          aria-label="Decrease quantity"
        >
          −
        </button>

        <input
          id="product-quantity"
          type="number"
          min={1}
          max={product.stockQuantity}
          value={quantity}
          onChange={(event) => updateQuantity(Number(event.target.value))}
          className="w-24 h-12 text-center border rounded-lg text-lg font-semibold"
        />

        <button
          type="button"
          onClick={() => updateQuantity(quantity + 1)}
          disabled={hasStockLimit && quantity >= product.stockQuantity!}
          className="w-12 h-12 border rounded-lg text-xl hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
          aria-label="Increase quantity"
        >
          +
        </button>
      </div>
    </div>
  );
}

// frontend/app/components/ProductDetails/Info/ProductStockInfo.tsx
"use client";

import { Product } from "@/src/lib/api";

interface Props {
  product: Product;
  outOfStock: boolean;
  hasStockLimit: boolean;
}

export default function ProductStockInfo({
  product,
  outOfStock,
  hasStockLimit,
}: Props) {
  if (!hasStockLimit) return null;

  return (
    <div className="mt-6 rounded-lg bg-gray-50 border p-4">
      <p className="font-medium">
        {outOfStock
          ? "Currently unavailable"
          : `${product.stockQuantity} item(s) available`}
      </p>
      {!outOfStock && (
        <p className="text-sm text-gray-500 mt-1">
          Choose the quantity you want to add to your cart.
        </p>
      )}
    </div>
  );
}

"use client";

import { Product } from "@/src/lib/api";

interface Props {
  product: Product;
  categoryName: string | null;
  averageRating: string | null;
  reviewCount: number;
  outOfStock: boolean;
}

export default function ProductHeader({
  product,
  categoryName,
  averageRating,
  reviewCount,
  outOfStock,
}: Props) {
  return (
    <div className="flex items-start justify-between gap-4">
      <div>
        <p className="text-sm font-medium text-gray-500 uppercase tracking-wide">
          Product details
        </p>
        <h1 className="text-3xl sm:text-4xl font-bold mt-2">{product.name}</h1>
        {categoryName && (
          <p className="text-sm text-gray-500 mt-1">
            Category: {categoryName}
          </p>
        )}
        {averageRating && (
          <p className="mt-1 text-lg text-yellow-500 font-semibold">
            ⭐ {averageRating}/5
            <span className="text-gray-600 text-sm ml-2">
              ({reviewCount} {reviewCount === 1 ? "review" : "reviews"})
            </span>
          </p>
        )}
      </div>

      {outOfStock ? (
        <span className="shrink-0 bg-red-100 text-red-700 text-sm font-semibold px-3 py-1.5 rounded-full">
          Out of stock
        </span>
      ) : (
        <span className="shrink-0 bg-green-100 text-green-700 text-sm font-semibold px-3 py-1.5 rounded-full">
          In stock
        </span>
      )}
    </div>
  );
}

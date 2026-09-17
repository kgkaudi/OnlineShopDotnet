"use client";

import { Product } from "@/src/lib/api";

interface Props {
  product: Product;
}

export default function ProductDescription({ product }: Props) {
  return (
    <div className="mt-6 border-t pt-6">
      <h2 className="font-semibold text-lg">Description</h2>
      <p className="text-gray-600 leading-7 mt-2">
        {product.description || "No description is available for this product."}
      </p>
    </div>
  );
}

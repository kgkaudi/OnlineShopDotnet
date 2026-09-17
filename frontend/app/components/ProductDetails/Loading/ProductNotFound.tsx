"use client";

import Link from "next/link";

interface Props {
  error: string | null;
}

export default function ProductNotFound({ error }: Props) {
  return (
    <div className="max-w-2xl mx-auto py-12 text-center">
      <div className="text-6xl mb-4">😕</div>
      <h1 className="text-2xl font-bold">Product not found</h1>
      <p className="text-gray-600 mt-2">
        {error || "We couldn't find the product you're looking for."}
      </p>
      <Link
        href="/products"
        className="inline-block mt-6 bg-black text-white px-6 py-3 rounded-lg hover:bg-gray-800"
      >
        Back to Products
      </Link>
    </div>
  );
}

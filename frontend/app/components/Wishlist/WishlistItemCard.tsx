"use client";

import Link from "next/link";

interface WishlistItemCardProps {
  item: any;
  onRemove: (productId: string) => void;
}

export default function WishlistItemCard({ item, onRemove }: WishlistItemCardProps) {
  return (
    <div className="border rounded p-4 bg-white shadow-sm hover:shadow-md transition">
      <h2 className="font-semibold text-lg">
        <Link
          href={`/products/${item.productId}`}
          className="hover:underline hover:text-blue-600 transition"
        >
          {item.productName}
        </Link>
      </h2>

      {item.productDescription && (
        <p className="text-gray-600 mt-1">{item.productDescription}</p>
      )}

      <p className="text-black font-bold mt-3">{item.productPrice} €</p>

      <Link
        href={`/products/${item.productId}`}
        className="mt-4 block w-full border border-blue-400 text-blue-600 py-3 rounded text-sm sm:text-base text-center hover:bg-blue-50 transition"
      >
        View Product Details →
      </Link>

      <button
        onClick={() => onRemove(item.productId)}
        className="mt-2 w-full border border-red-400 text-red-600 py-3 rounded text-sm sm:text-base hover:bg-red-50"
      >
        Remove
      </button>
    </div>
  );
}

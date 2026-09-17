"use client";

import Link from "next/link";

export default function EmptyWishlist() {
  return (
    <div className="border rounded-lg bg-white p-8 text-center">
      <p className="text-gray-600 mb-4">Your wishlist is empty.</p>

      <Link
        href="/products"
        className="inline-block bg-black text-white px-5 py-2 rounded hover:bg-gray-800"
      >
        Browse Products
      </Link>
    </div>
  );
}

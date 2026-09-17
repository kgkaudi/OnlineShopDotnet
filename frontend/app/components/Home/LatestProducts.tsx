"use client";

import Link from "next/link";
import { Product } from "@/src/lib/api";

interface LatestProductsProps {
  products: Product[];
  loading: boolean;
}

export default function LatestProducts({ products, loading }: LatestProductsProps) {
  return (
    <section className="mb-10">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold">Latest Products</h2>
        <Link href="/products" className="text-sm text-blue-600 hover:underline">
          View All
        </Link>
      </div>

      {loading ? (
        <p className="text-gray-600">Loading products...</p>
      ) : products.length === 0 ? (
        <p className="text-gray-600">No products available.</p>
      ) : (
        <div className="grid gap-6 grid-cols-1 sm:grid-cols-2 md:grid-cols-3">
          {products.slice(0, 6).map((product) => (
            <Link
              key={product.id}
              href={`/products/${product.id}`}
              className="border rounded-lg p-4 bg-white shadow-sm hover:shadow-md transition block"
            >
              <h3 className="font-semibold text-lg">{product.name}</h3>

              <p className="text-gray-600 mt-1 line-clamp-2">
                {product.description ?? "No description available."}
              </p>

              <p className="text-black font-bold mt-3">
                €{product.price.toFixed(2)}
              </p>

              <button className="mt-4 w-full bg-black text-white py-2 rounded">
                View Product
              </button>
            </Link>
          ))}
        </div>
      )}
    </section>
  );
}

"use client";

import { useEffect, useState } from "react";
import Container from "./components/Container";
import { api, type Product } from "@/src/lib/api";
import Link from "next/link";

export default function HomePage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.getProducts()
      .then(setProducts)
      .finally(() => setLoading(false));
  }, []);

  return (
    <Container>
      {/* Hero Banner */}
      <section className="relative mb-10 rounded-xl overflow-hidden bg-gradient-to-r from-black to-gray-800 text-white p-10 shadow-lg">
        <h1 className="text-4xl font-bold mb-4">Welcome to OnlineShop</h1>
        <p className="text-gray-300 max-w-xl">
          Discover the latest products, manage your cart, and enjoy a smooth shopping experience.
        </p>

        <Link
          href="/products"
          className="inline-block mt-6 px-6 py-3 bg-white text-black font-semibold rounded hover:bg-gray-200"
        >
          Browse Products
        </Link>
      </section>

      {/* Latest Products */}
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

      {/* Promotional Banners */}
      <section className="grid gap-6 grid-cols-1 md:grid-cols-2 mb-10">
        <div className="rounded-xl p-8 bg-blue-600 text-white shadow-lg">
          <h3 className="text-xl font-bold mb-2">Seasonal Discounts</h3>
          <p className="text-blue-100 mb-4">
            Save big on selected items. Limited time only.
          </p>
          <Link
            href="/products"
            className="inline-block px-5 py-2 bg-white text-blue-600 font-semibold rounded hover:bg-gray-100"
          >
            Shop Now
          </Link>
        </div>

        <div className="rounded-xl p-8 bg-green-600 text-white shadow-lg">
          <h3 className="text-xl font-bold mb-2">New Arrivals</h3>
          <p className="text-green-100 mb-4">
            Fresh products added weekly. Check out what's new.
          </p>
          <Link
            href="/products"
            className="inline-block px-5 py-2 bg-white text-green-600 font-semibold rounded hover:bg-gray-100"
          >
            Explore
          </Link>
        </div>
      </section>
    </Container>
  );
}

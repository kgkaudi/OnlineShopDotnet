"use client";

import Link from "next/link";

export default function HeroBanner() {
  return (
    <section className="relative mb-10 rounded-xl overflow-hidden bg-linear-to-r from-black to-gray-800 text-white p-10 shadow-lg">
      <h1 className="text-4xl font-bold mb-4">Welcome to OnlineShop</h1>

      <p className="text-gray-300 max-w-xl">
        Discover the latest products, manage your cart, and enjoy a smooth
        shopping experience.
      </p>

      <Link
        href="/products"
        className="inline-block mt-6 px-6 py-3 bg-white text-black font-semibold rounded hover:bg-gray-200"
      >
        Browse Products
      </Link>
    </section>
  );
}

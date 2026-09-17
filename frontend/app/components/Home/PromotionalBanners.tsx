"use client";

import Link from "next/link";

export default function PromotionalBanners() {
  return (
    <section className="grid gap-6 grid-cols-1 md:grid-cols-2 mb-10">
      {/* Seasonal Discounts */}
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

      {/* New Arrivals */}
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
  );
}

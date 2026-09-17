"use client";

export default function EmptyCart() {
  return (
    <div className="border rounded-lg bg-white p-8 text-center">
      <p className="text-gray-600 mb-4">Your cart is empty.</p>

      <a
        href="/products"
        className="inline-block bg-black text-white px-5 py-2 rounded hover:bg-gray-800"
      >
        Continue Shopping
      </a>
    </div>
  );
}

"use client";

export default function EmptyOrders() {
  return (
    <div className="rounded-xl border border-gray-200 bg-white px-6 py-12 text-center">
      <div className="text-4xl">📦</div>

      <h2 className="mt-4 text-xl font-semibold text-gray-900">No orders yet</h2>

      <p className="mt-2 text-sm text-gray-600">
        Your completed purchases will appear here.
      </p>
    </div>
  );
}

"use client";

import Link from "next/link";

export default function AdminOrdersHeader() {
  return (
    <div className="flex items-center justify-between mb-8">
      <div>
        <h1 className="text-3xl font-bold">Orders Admin</h1>
        <p className="text-gray-600 mt-2">View and manage all orders.</p>
      </div>

      <Link
        href="/admin/users"
        className="px-4 py-2 border rounded hover:bg-gray-50"
      >
        Users
      </Link>
    </div>
  );
}

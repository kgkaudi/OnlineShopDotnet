"use client";

import { useRouter } from "next/navigation";

export default function AdminProductsHeader() {
  const router = useRouter();

  return (
    <div className="flex items-center justify-between mb-8">
      <div>
        <h1 className="text-3xl font-bold">Product Management</h1>
        <p className="text-gray-600 mt-2">Create and manage products.</p>
      </div>

      <button
        type="button"
        onClick={() => router.push("/admin/users")}
        className="px-4 py-2 border rounded hover:bg-gray-50"
      >
        Users
      </button>
    </div>
  );
}

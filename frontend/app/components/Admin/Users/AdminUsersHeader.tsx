"use client";

import { useRouter } from "next/navigation";

export default function AdminUsersHeader() {
  const router = useRouter();

  return (
    <div className="flex items-center justify-between mb-8">
      <div>
        <h1 className="text-3xl font-bold">User Management</h1>
        <p className="text-gray-600 mt-2">Manage registered users and roles.</p>
      </div>

      <button
        type="button"
        onClick={() => router.push("/")}
        className="px-4 py-2 border rounded hover:bg-gray-50"
      >
        Back
      </button>
    </div>
  );
}

"use client";

import Link from "next/link";

export default function ProfileHeader() {
  return (
    <div className="mb-8">
      <Link
        href="/products"
        className="text-sm text-gray-600 hover:text-black"
      >
        ← Back to Products
      </Link>

      <h1 className="text-3xl font-bold mt-4">My Profile</h1>

      <p className="text-gray-600 mt-2">
        Update your personal and delivery details.
      </p>
    </div>
  );
}

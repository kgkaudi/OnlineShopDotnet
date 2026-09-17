"use client";

export default function AdminCouponsError({ message }: { message: string }) {
  return (
    <p className="text-red-600 bg-red-100 px-3 py-2 rounded mb-6">
      {message}
    </p>
  );
}

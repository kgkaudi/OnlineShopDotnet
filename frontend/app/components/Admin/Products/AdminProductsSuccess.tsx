"use client";

export default function AdminProductsSuccess({ message }: { message: string }) {
  return (
    <div className="mb-6 rounded border border-green-300 bg-green-50 px-4 py-3 text-green-700">
      {message}
    </div>
  );
}

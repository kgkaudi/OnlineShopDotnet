"use client";

export default function AdminCategoriesSuccess({ message }: { message: string }) {
  return (
    <div className="mb-6 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
      {message}
    </div>
  );
}

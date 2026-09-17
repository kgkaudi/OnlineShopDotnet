"use client";

interface Props {
  creating: boolean;
  newCategoryName: string;
  onChange: (value: string) => void;
  onSubmit: (e: React.FormEvent<HTMLFormElement>) => void;
}

export default function AdminCategoriesCreateForm({
  creating,
  newCategoryName,
  onChange,
  onSubmit,
}: Props) {
  return (
    <section className="mb-8 rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
      <h2 className="text-lg font-semibold text-gray-900">Add Category</h2>

      <form onSubmit={onSubmit} className="mt-4 flex flex-col gap-3 sm:flex-row">
        <input
          type="text"
          value={newCategoryName}
          onChange={(e) => onChange(e.target.value)}
          placeholder="Category name"
          maxLength={100}
          disabled={creating}
          className="min-w-0 flex-1 rounded-lg border border-gray-300 px-4 py-2.5 text-sm outline-none transition focus:border-gray-500 focus:ring-2 focus:ring-gray-200 disabled:bg-gray-100"
        />

        <button
          type="submit"
          disabled={creating || !newCategoryName.trim()}
          className="rounded-lg bg-black px-5 py-2.5 text-sm font-medium text-white transition hover:bg-gray-800 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {creating ? "Adding..." : "Add Category"}
        </button>
      </form>
    </section>
  );
}

"use client";

import { FormEvent, useEffect, useState } from "react";
import { api, Category } from "@/src/lib/api";

export default function AdminCategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);

  const [newCategoryName, setNewCategoryName] = useState("");

  const [editingCategoryId, setEditingCategoryId] = useState<string | null>(
    null,
  );

  const [editingName, setEditingName] = useState("");

  const [categoryToDelete, setCategoryToDelete] = useState<Category | null>(
    null,
  );

  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [savingId, setSavingId] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  useEffect(() => {
    loadCategories();
  }, []);

  async function loadCategories() {
    try {
      setLoading(true);
      setError("");

      const result = await api.getCategories();

      setCategories(result ?? []);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to load categories.",
      );
    } finally {
      setLoading(false);
    }
  }

  function clearMessages() {
    setError("");
    setSuccess("");
  }

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    clearMessages();

    const name = newCategoryName.trim();

    if (!name) {
      setError("Category name is required.");
      return;
    }

    if (
      categories.some(
        (category) => category.name.trim().toLowerCase() === name.toLowerCase(),
      )
    ) {
      setError("A category with this name already exists.");
      return;
    }

    try {
      setCreating(true);

      const created = await api.createCategory({
        name,
      });

      setCategories((current) => [...current, created]);

      setNewCategoryName("");
      setSuccess("Category created successfully.");
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to create category.",
      );
    } finally {
      setCreating(false);
    }
  }

  function startEditing(category: Category) {
    clearMessages();

    setEditingCategoryId(category.id);
    setEditingName(category.name);
  }

  function cancelEditing() {
    setEditingCategoryId(null);
    setEditingName("");
  }

  async function handleUpdate(categoryId: string) {
    clearMessages();

    const name = editingName.trim();

    if (!name) {
      setError("Category name is required.");
      return;
    }

    const duplicate = categories.some(
      (category) =>
        category.id !== categoryId &&
        category.name.trim().toLowerCase() === name.toLowerCase(),
    );

    if (duplicate) {
      setError("A category with this name already exists.");
      return;
    }

    const currentCategory = categories.find(
      (category) => category.id === categoryId,
    );

    if (!currentCategory) {
      setError("Category not found.");
      return;
    }

    if (currentCategory.name.trim().toLowerCase() === name.toLowerCase()) {
      cancelEditing();
      return;
    }

    try {
      setSavingId(categoryId);

      await api.updateCategory(categoryId, {
        name,
      });

      setCategories((current) =>
        current.map((category) =>
          category.id === categoryId
            ? {
                ...category,
                name,
              }
            : category,
        ),
      );

      cancelEditing();
      setSuccess("Category updated successfully.");
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to update category.",
      );
    } finally {
      setSavingId(null);
    }
  }

  function openDeleteConfirmation(category: Category) {
    clearMessages();
    setCategoryToDelete(category);
  }

  function closeDeleteConfirmation() {
    if (deletingId !== null) {
      return;
    }

    setCategoryToDelete(null);
  }

  async function confirmDelete() {
    if (!categoryToDelete) {
      return;
    }

    const category = categoryToDelete;

    try {
      setDeletingId(category.id);

      await api.deleteCategory(category.id);

      setCategories((current) =>
        current.filter((item) => item.id !== category.id),
      );

      if (editingCategoryId === category.id) {
        cancelEditing();
      }

      setCategoryToDelete(null);
      setSuccess("Category deleted successfully.");
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to delete category.",
      );
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <>
      <main className="mx-auto max-w-5xl px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">
            Manage Categories
          </h1>

          <p className="mt-2 text-sm text-gray-600">
            Create, edit, and delete product categories.
          </p>
        </div>

        {/* Messages */}
        {error && (
          <div className="mb-6 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {error}
          </div>
        )}

        {success && (
          <div className="mb-6 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
            {success}
          </div>
        )}

        {/* Create category */}
        <section className="mb-8 rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
          <h2 className="text-lg font-semibold text-gray-900">Add Category</h2>

          <form
            onSubmit={handleCreate}
            className="mt-4 flex flex-col gap-3 sm:flex-row"
          >
            <input
              type="text"
              value={newCategoryName}
              onChange={(event) => setNewCategoryName(event.target.value)}
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

        {/* Categories */}
        <section className="rounded-xl border border-gray-200 bg-white shadow-sm">
          <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4">
            <div>
              <h2 className="font-semibold text-gray-900">Categories</h2>

              {!loading && (
                <p className="mt-1 text-xs text-gray-500">
                  {categories.length}{" "}
                  {categories.length === 1 ? "category" : "categories"}
                </p>
              )}
            </div>
          </div>

          {loading ? (
            <div className="space-y-4 p-6">
              {[1, 2, 3, 4].map((item) => (
                <div
                  key={item}
                  className="flex animate-pulse items-center justify-between gap-4"
                >
                  <div className="h-5 w-48 rounded bg-gray-200" />
                  <div className="h-9 w-32 rounded bg-gray-200" />
                </div>
              ))}
            </div>
          ) : categories.length === 0 ? (
            <div className="px-6 py-12 text-center">
              <div className="text-4xl">📂</div>

              <h3 className="mt-4 font-semibold text-gray-900">
                No categories yet
              </h3>

              <p className="mt-2 text-sm text-gray-500">
                Create your first category using the form above.
              </p>
            </div>
          ) : (
            <div className="divide-y divide-gray-100">
              {categories.map((category) => {
                const isEditing = editingCategoryId === category.id;

                const isSaving = savingId === category.id;

                const isDeleting = deletingId === category.id;

                return (
                  <div
                    key={category.id}
                    className="flex flex-col gap-4 px-6 py-4 sm:flex-row sm:items-center sm:justify-between"
                  >
                    {/* Name */}
                    <div className="min-w-0 flex-1">
                      {isEditing ? (
                        <input
                          type="text"
                          value={editingName}
                          onChange={(event) =>
                            setEditingName(event.target.value)
                          }
                          onKeyDown={(event) => {
                            if (event.key === "Enter") {
                              event.preventDefault();

                              if (!isSaving) {
                                handleUpdate(category.id);
                              }
                            }

                            if (event.key === "Escape" && !isSaving) {
                              cancelEditing();
                            }
                          }}
                          maxLength={100}
                          autoFocus
                          disabled={isSaving}
                          className="w-full max-w-md rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-gray-500 focus:ring-2 focus:ring-gray-200 disabled:bg-gray-100"
                        />
                      ) : (
                        <div>
                          <p className="font-medium text-gray-900">
                            {category.name}
                          </p>

                          <p className="mt-1 truncate text-xs text-gray-400">
                            ID: {category.id}
                          </p>
                        </div>
                      )}
                    </div>

                    {/* Actions */}
                    <div className="flex shrink-0 items-center gap-2">
                      {isEditing ? (
                        <>
                          <button
                            type="button"
                            onClick={() => handleUpdate(category.id)}
                            disabled={isSaving || !editingName.trim()}
                            className="rounded-lg bg-black px-4 py-2 text-sm font-medium text-white transition hover:bg-gray-800 disabled:cursor-not-allowed disabled:opacity-50"
                          >
                            {isSaving ? "Saving..." : "Save"}
                          </button>

                          <button
                            type="button"
                            onClick={cancelEditing}
                            disabled={isSaving}
                            className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
                          >
                            Cancel
                          </button>
                        </>
                      ) : (
                        <>
                          <button
                            type="button"
                            onClick={() => startEditing(category)}
                            disabled={isDeleting || deletingId !== null}
                            className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
                          >
                            Edit
                          </button>

                          <button
                            type="button"
                            onClick={() => openDeleteConfirmation(category)}
                            disabled={
                              isDeleting ||
                              deletingId !== null ||
                              editingCategoryId !== null
                            }
                            className="rounded-lg border border-red-200 px-4 py-2 text-sm font-medium text-red-600 transition hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-50"
                          >
                            {isDeleting ? "Deleting..." : "Delete"}
                          </button>
                        </>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </section>
      </main>

      {/* Delete confirmation modal */}
      {categoryToDelete && (
        <div
          className="fixed inset-0 z-[100] flex items-center justify-center bg-black/40 px-4 backdrop-blur-sm"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget) {
              closeDeleteConfirmation();
            }
          }}
          role="presentation"
        >
          <div
            className="w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl"
            role="dialog"
            aria-modal="true"
            aria-labelledby="delete-category-title"
            aria-describedby="delete-category-description"
          >
            {/* Icon */}
            <div className="flex h-12 w-12 items-center justify-center rounded-full bg-red-100">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                className="h-6 w-6 text-red-600"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M12 9v4m0 4h.01M10.29 3.86 1.82 18a2 2 0 0 0 1.72 3h16.92a2 2 0 0 0 1.72-3L13.71 3.86a2 2 0 0 0-3.42 0Z"
                />
              </svg>
            </div>

            {/* Content */}
            <h2
              id="delete-category-title"
              className="mt-5 text-xl font-semibold text-gray-900"
            >
              Delete category?
            </h2>

            <p
              id="delete-category-description"
              className="mt-2 text-sm leading-6 text-gray-600"
            >
              Are you sure you want to delete{" "}
              <span className="font-semibold text-gray-900">
                "{categoryToDelete.name}"
              </span>
              ? This action cannot be undone.
            </p>

            {/* Actions */}
            <div className="mt-6 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
              <button
                type="button"
                onClick={closeDeleteConfirmation}
                disabled={deletingId !== null}
                className="rounded-lg border border-gray-300 px-5 py-2.5 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Cancel
              </button>

              <button
                type="button"
                onClick={confirmDelete}
                disabled={deletingId !== null}
                className="rounded-lg bg-red-600 px-5 py-2.5 text-sm font-medium text-white transition hover:bg-red-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {deletingId !== null ? "Deleting..." : "Delete category"}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

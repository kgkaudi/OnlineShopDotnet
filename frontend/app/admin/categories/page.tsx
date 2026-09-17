"use client";

import { FormEvent, useEffect, useState } from "react";
import { api, Category } from "@/src/lib/api";

import AdminCategoriesHeader from "../../components/Admin/Categories/AdminCategoriesHeader";
import AdminCategoriesError from "../../components/Admin/Categories/AdminCategoriesError";
import AdminCategoriesSuccess from "../../components/Admin/Categories/AdminCategoriesSuccess";
import AdminCategoriesCreateForm from "../../components/Admin/Categories/AdminCategoriesCreateForm";
import AdminCategoriesList from "../../components/Admin/Categories/AdminCategoriesList";
import AdminCategoryDeleteModal from "../../components/Admin/Categories/AdminCategoryDeleteModal";

export default function AdminCategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [newCategoryName, setNewCategoryName] = useState("");

  const [editingCategoryId, setEditingCategoryId] = useState<string | null>(null);
  const [editingName, setEditingName] = useState("");

  const [categoryToDelete, setCategoryToDelete] = useState<Category | null>(null);

  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [savingId, setSavingId] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  // -----------------------------------------------------
  // LOAD CATEGORIES
  // -----------------------------------------------------
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
      setError(err instanceof Error ? err.message : "Failed to load categories.");
    } finally {
      setLoading(false);
    }
  }

  function clearMessages() {
    setError("");
    setSuccess("");
  }

  // -----------------------------------------------------
  // CREATE CATEGORY
  // -----------------------------------------------------
  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    clearMessages();

    const name = newCategoryName.trim();
    if (!name) {
      setError("Category name is required.");
      return;
    }

    if (categories.some((c) => c.name.trim().toLowerCase() === name.toLowerCase())) {
      setError("A category with this name already exists.");
      return;
    }

    try {
      setCreating(true);

      const created = await api.createCategory({ name });
      setCategories((prev) => [...prev, created]);

      setNewCategoryName("");
      setSuccess("Category created successfully.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create category.");
    } finally {
      setCreating(false);
    }
  }

  // -----------------------------------------------------
  // EDIT CATEGORY
  // -----------------------------------------------------
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
      (c) => c.id !== categoryId && c.name.trim().toLowerCase() === name.toLowerCase()
    );

    if (duplicate) {
      setError("A category with this name already exists.");
      return;
    }

    const currentCategory = categories.find((c) => c.id === categoryId);
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

      await api.updateCategory(categoryId, { name });

      setCategories((prev) =>
        prev.map((c) => (c.id === categoryId ? { ...c, name } : c))
      );

      cancelEditing();
      setSuccess("Category updated successfully.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update category.");
    } finally {
      setSavingId(null);
    }
  }

  // -----------------------------------------------------
  // DELETE CATEGORY
  // -----------------------------------------------------
  function openDeleteConfirmation(category: Category) {
    clearMessages();
    setCategoryToDelete(category);
  }

  function closeDeleteConfirmation() {
    if (deletingId !== null) return;
    setCategoryToDelete(null);
  }

  async function confirmDelete() {
    if (!categoryToDelete) return;

    const category = categoryToDelete;

    try {
      setDeletingId(category.id);

      await api.deleteCategory(category.id);

      setCategories((prev) => prev.filter((c) => c.id !== category.id));

      if (editingCategoryId === category.id) cancelEditing();

      setCategoryToDelete(null);
      setSuccess("Category deleted successfully.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete category.");
    } finally {
      setDeletingId(null);
    }
  }

  // -----------------------------------------------------
  // RENDER
  // -----------------------------------------------------
  return (
    <>
      <main className="mx-auto max-w-5xl px-4 py-8">
        <AdminCategoriesHeader />

        {error && <AdminCategoriesError message={error} />}
        {success && <AdminCategoriesSuccess message={success} />}

        <AdminCategoriesCreateForm
          creating={creating}
          newCategoryName={newCategoryName}
          onChange={setNewCategoryName}
          onSubmit={handleCreate}
        />

        <section className="rounded-xl border border-gray-200 bg-white shadow-sm">
          <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4">
            <div>
              <h2 className="font-semibold text-gray-900">Categories</h2>

              {!loading && (
                <p className="mt-1 text-xs text-gray-500">
                  {categories.length} {categories.length === 1 ? "category" : "categories"}
                </p>
              )}
            </div>
          </div>

          <AdminCategoriesList
            categories={categories}
            loading={loading}
            editingCategoryId={editingCategoryId}
            savingId={savingId}
            deletingId={deletingId}
            editingName={editingName}
            onStartEdit={startEditing}
            onChangeName={setEditingName}
            onSave={handleUpdate}
            onCancel={cancelEditing}
            onDelete={openDeleteConfirmation}
          />
        </section>
      </main>

      {categoryToDelete && (
        <AdminCategoryDeleteModal
          category={categoryToDelete}
          deletingId={deletingId}
          onCancel={closeDeleteConfirmation}
          onConfirm={confirmDelete}
        />
      )}
    </>
  );
}

"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import {
  api,
  getClientUserId,
  type Category,
  type Product,
} from "@/src/lib/api";
import { useAuthStore } from "@/src/store/authStore";

interface ProductForm {
  name: string;
  description: string;
  price: string;
  stockQuantity: string;
  categoryId: string;
}

const emptyForm: ProductForm = {
  name: "",
  description: "",
  price: "",
  stockQuantity: "0",
  categoryId: "",
};

export default function AdminProductsPage() {
  const router = useRouter();

  const isLoggedIn = useAuthStore(
    (state) => state.isLoggedIn,
  );

  const [authorized, setAuthorized] = useState(false);
  const [loading, setLoading] = useState(true);

  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>(
    [],
  );

  const [form, setForm] =
    useState<ProductForm>(emptyForm);

  const [error, setError] = useState<string | null>(
    null,
  );

  const [success, setSuccess] = useState<
    string | null
  >(null);

  const [creating, setCreating] = useState(false);

  const [deletingProductId, setDeletingProductId] =
    useState<string | null>(null);

  /*
   * Custom category dropdown state.
   */
  const [categoryDropdownOpen, setCategoryDropdownOpen] =
    useState(false);

  const categoryDropdownRef =
    useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadAdminProducts() {
      try {
        setLoading(true);
        setError(null);

        const currentUserId = getClientUserId();

        if (!currentUserId) {
          router.replace("/auth/login");
          return;
        }

        /*
         * Verify the current user's actual backend role.
         */
        const currentUser =
          await api.getUserById(currentUserId);

        const currentUserIsAdmin =
          currentUser.roles?.some(
            (role) =>
              role.trim().toLowerCase() === "admin",
          ) ?? false;

        if (!currentUserIsAdmin) {
          if (!cancelled) {
            setAuthorized(false);
          }

          router.replace("/");
          return;
        }

        if (!cancelled) {
          setAuthorized(true);
        }

        /*
         * Load products and categories together.
         */
        const [productsResult, categoriesResult] =
          await Promise.all([
            api.getProducts(),
            api.getCategories(),
          ]);

        if (!cancelled) {
          setProducts(productsResult);
          setCategories(categoriesResult);
        }
      } catch (err) {
        console.error(
          "Failed to load admin products:",
          err,
        );

        if (!cancelled) {
          setError(
            err instanceof Error
              ? err.message
              : "Failed to load products.",
          );
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    if (!isLoggedIn) {
      router.replace("/auth/login");
      setLoading(false);
      return;
    }

    loadAdminProducts();

    return () => {
      cancelled = true;
    };
  }, [isLoggedIn, router]);

  /*
   * Close the category dropdown when clicking outside.
   */
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        categoryDropdownRef.current &&
        !categoryDropdownRef.current.contains(
          event.target as Node,
        )
      ) {
        setCategoryDropdownOpen(false);
      }
    }

    if (categoryDropdownOpen) {
      document.addEventListener(
        "mousedown",
        handleClickOutside,
      );
    }

    return () => {
      document.removeEventListener(
        "mousedown",
        handleClickOutside,
      );
    };
  }, [categoryDropdownOpen]);

  /*
   * Close the category dropdown with Escape.
   */
  useEffect(() => {
    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setCategoryDropdownOpen(false);
      }
    }

    if (categoryDropdownOpen) {
      document.addEventListener(
        "keydown",
        handleEscape,
      );
    }

    return () => {
      document.removeEventListener(
        "keydown",
        handleEscape,
      );
    };
  }, [categoryDropdownOpen]);

  function updateField(
    field: keyof ProductForm,
    value: string,
  ) {
    setForm((current) => ({
      ...current,
      [field]: value,
    }));
  }

  function handleCategorySelect(categoryId: string) {
    updateField("categoryId", categoryId);
    setCategoryDropdownOpen(false);
  }

  function getSelectedCategoryName(): string {
    if (!form.categoryId) {
      return "Select a category";
    }

    const selectedCategory = categories.find(
      (category) =>
        category.id === form.categoryId,
    );

    return (
      selectedCategory?.name ??
      "Select a category"
    );
  }

  async function handleCreateProduct(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    setError(null);
    setSuccess(null);

    const name = form.name.trim();
    const description = form.description.trim();
    const categoryId = form.categoryId.trim();

    const price = Number(form.price);
    const stockQuantity = Number(
      form.stockQuantity,
    );

    if (!name) {
      setError("Product name is required.");
      return;
    }

    if (!Number.isFinite(price) || price <= 0) {
      setError(
        "Price must be greater than zero.",
      );
      return;
    }

    if (
      !Number.isInteger(stockQuantity) ||
      stockQuantity < 0
    ) {
      setError(
        "Stock quantity must be a whole number greater than or equal to zero.",
      );
      return;
    }

    /*
     * If a category was selected, make sure it exists
     * in the categories currently loaded by the API.
     */
    if (
      categoryId &&
      !categories.some(
        (category) => category.id === categoryId,
      )
    ) {
      setError("Please select a valid category.");
      return;
    }

    setCreating(true);

    try {
      const createdProduct =
        await api.createProduct({
          name,
          description: description || null,
          price,
          stockQuantity,
          categoryId: categoryId || null,
        });

      setProducts((current) => [
        createdProduct,
        ...current,
      ]);

      setForm(emptyForm);
      setCategoryDropdownOpen(false);

      setSuccess(
        `Product "${createdProduct.name}" created successfully.`,
      );
    } catch (err) {
      console.error(
        "Failed to create product:",
        err,
      );

      setError(
        err instanceof Error
          ? err.message
          : "Failed to create product.",
      );
    } finally {
      setCreating(false);
    }
  }

  async function handleDeleteProduct(
    product: Product,
  ) {
    const confirmed = window.confirm(
      `Are you sure you want to delete "${product.name}"?`,
    );

    if (!confirmed) {
      return;
    }

    setDeletingProductId(product.id);
    setError(null);
    setSuccess(null);

    try {
      await api.deleteProduct(product.id);

      setProducts((current) =>
        current.filter(
          (item) => item.id !== product.id,
        ),
      );

      setSuccess(
        `Product "${product.name}" deleted successfully.`,
      );
    } catch (err) {
      console.error(
        "Failed to delete product:",
        err,
      );

      setError(
        err instanceof Error
          ? err.message
          : "Failed to delete product.",
      );
    } finally {
      setDeletingProductId(null);
    }
  }

  function getCategoryName(
    categoryId?: string | null,
  ): string {
    if (!categoryId) {
      return "-";
    }

    const category = categories.find(
      (item) => item.id === categoryId,
    );

    return category?.name ?? "Unknown category";
  }

  if (loading) {
    return (
      <main className="max-w-6xl mx-auto px-4 py-10">
        <p className="text-gray-600">
          Loading admin products...
        </p>
      </main>
    );
  }

  if (!isLoggedIn || !authorized) {
    return null;
  }

  return (
    <main className="max-w-6xl mx-auto px-4 py-10">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold">
            Product Management
          </h1>

          <p className="text-gray-600 mt-2">
            Create and manage products.
          </p>
        </div>

        <button
          type="button"
          onClick={() =>
            router.push("/admin/users")
          }
          className="px-4 py-2 border rounded hover:bg-gray-50"
        >
          Users
        </button>
      </div>

      {/* Messages */}
      {error && (
        <div className="mb-6 rounded border border-red-300 bg-red-50 px-4 py-3 text-red-700">
          {error}
        </div>
      )}

      {success && (
        <div className="mb-6 rounded border border-green-300 bg-green-50 px-4 py-3 text-green-700">
          {success}
        </div>
      )}

      {/* Create Product */}
      <section className="mb-10 rounded-lg border bg-white p-6">
        <h2 className="text-xl font-semibold mb-6">
          Create Product
        </h2>

        <form
          onSubmit={handleCreateProduct}
          className="space-y-5"
        >
          {/* Name */}
          <div>
            <label
              htmlFor="name"
              className="block text-sm font-medium mb-1"
            >
              Name
            </label>

            <input
              id="name"
              type="text"
              value={form.name}
              onChange={(event) =>
                updateField(
                  "name",
                  event.target.value,
                )
              }
              placeholder="Product name"
              className="w-full rounded border px-3 py-2 focus:outline-none focus:ring-2 focus:ring-black"
              disabled={creating}
            />
          </div>

          {/* Description */}
          <div>
            <label
              htmlFor="description"
              className="block text-sm font-medium mb-1"
            >
              Description
            </label>

            <textarea
              id="description"
              value={form.description}
              onChange={(event) =>
                updateField(
                  "description",
                  event.target.value,
                )
              }
              placeholder="Product description"
              rows={4}
              className="w-full rounded border px-3 py-2 focus:outline-none focus:ring-2 focus:ring-black"
              disabled={creating}
            />
          </div>

          {/* Price + Stock */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
            <div>
              <label
                htmlFor="price"
                className="block text-sm font-medium mb-1"
              >
                Price
              </label>

              <input
                id="price"
                type="number"
                min="0.01"
                step="0.01"
                value={form.price}
                onChange={(event) =>
                  updateField(
                    "price",
                    event.target.value,
                  )
                }
                placeholder="0.00"
                className="w-full rounded border px-3 py-2 focus:outline-none focus:ring-2 focus:ring-black"
                disabled={creating}
              />
            </div>

            <div>
              <label
                htmlFor="stockQuantity"
                className="block text-sm font-medium mb-1"
              >
                Stock Quantity
              </label>

              <input
                id="stockQuantity"
                type="number"
                min="0"
                step="1"
                value={form.stockQuantity}
                onChange={(event) =>
                  updateField(
                    "stockQuantity",
                    event.target.value,
                  )
                }
                placeholder="0"
                className="w-full rounded border px-3 py-2 focus:outline-none focus:ring-2 focus:ring-black"
                disabled={creating}
              />
            </div>
          </div>

          {/* Category Dropdown */}
          <div>
            <label
              htmlFor="categoryId"
              className="block text-sm font-medium mb-1"
            >
              Category
            </label>

            <div
              ref={categoryDropdownRef}
              className="relative"
            >
              <button
                id="categoryId"
                type="button"
                disabled={
                  creating ||
                  categories.length === 0
                }
                onClick={() =>
                  setCategoryDropdownOpen(
                    (current) => !current,
                  )
                }
                aria-haspopup="listbox"
                aria-expanded={
                  categoryDropdownOpen
                }
                className={`w-full rounded border bg-white px-3 py-2 text-left flex items-center justify-between focus:outline-none focus:ring-2 focus:ring-black ${
                  creating ||
                  categories.length === 0
                    ? "cursor-not-allowed opacity-50"
                    : "cursor-pointer hover:bg-gray-50"
                }`}
              >
                <span
                  className={
                    form.categoryId
                      ? "text-gray-900"
                      : "text-gray-500"
                  }
                >
                  {getSelectedCategoryName()}
                </span>

                <svg
                  className={`h-5 w-5 transition-transform ${
                    categoryDropdownOpen
                      ? "rotate-180"
                      : ""
                  }`}
                  viewBox="0 0 20 20"
                  fill="currentColor"
                  aria-hidden="true"
                >
                  <path
                    fillRule="evenodd"
                    d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.51a.75.75 0 01-1.08 0l-4.25-4.51a.75.75 0 01.02-1.06z"
                    clipRule="evenodd"
                  />
                </svg>
              </button>

              {categoryDropdownOpen &&
                categories.length > 0 && (
                  <div
                    className="absolute z-50 mt-1 w-full rounded-md border bg-white shadow-lg overflow-hidden"
                    role="listbox"
                    aria-labelledby="categoryId"
                  >
                    <button
                      type="button"
                      onClick={() =>
                        handleCategorySelect("")
                      }
                      className={`w-full px-3 py-2 text-left hover:bg-gray-100 ${
                        !form.categoryId
                          ? "bg-gray-100 font-medium"
                          : ""
                      }`}
                      role="option"
                      aria-selected={
                        !form.categoryId
                      }
                    >
                      Select a category
                    </button>

                    {categories.map(
                      (category) => {
                        const isSelected =
                          form.categoryId ===
                          category.id;

                        return (
                          <button
                            key={category.id}
                            type="button"
                            onClick={() =>
                              handleCategorySelect(
                                category.id,
                              )
                            }
                            className={`w-full px-3 py-2 text-left hover:bg-gray-100 flex items-center justify-between ${
                              isSelected
                                ? "bg-gray-100 font-medium"
                                : ""
                            }`}
                            role="option"
                            aria-selected={
                              isSelected
                            }
                          >
                            <span>
                              {category.name}
                            </span>

                            {isSelected && (
                              <svg
                                className="h-5 w-5"
                                viewBox="0 0 20 20"
                                fill="currentColor"
                                aria-hidden="true"
                              >
                                <path
                                  fillRule="evenodd"
                                  d="M16.704 5.29a.75.75 0 010 1.06l-7.25 7.25a.75.75 0 01-1.06 0l-3.25-3.25a.75.75 0 111.06-1.06l2.72 2.72 6.72-6.72a.75.75 0 011.06 0z"
                                  clipRule="evenodd"
                                />
                              </svg>
                            )}
                          </button>
                        );
                      },
                    )}
                  </div>
                )}
            </div>

            {categories.length === 0 && (
              <p className="mt-1 text-xs text-gray-500">
                No categories are currently
                available.
              </p>
            )}

            {categories.length > 0 && (
              <p className="mt-1 text-xs text-gray-500">
                Select a category for this product.
              </p>
            )}
          </div>

          {/* Submit */}
          <div className="flex justify-end">
            <button
              type="submit"
              disabled={creating}
              className="px-5 py-2.5 rounded bg-black text-white hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {creating
                ? "Creating..."
                : "Create Product"}
            </button>
          </div>
        </form>
      </section>

      {/* Products */}
      <section>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-semibold">
            Products
          </h2>

          <span className="text-sm text-gray-500">
            Total: {products.length}
          </span>
        </div>

        <div className="border rounded-lg overflow-hidden bg-white">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-100 border-b">
                <tr>
                  <th className="text-left px-4 py-3">
                    Name
                  </th>

                  <th className="text-left px-4 py-3">
                    Description
                  </th>

                  <th className="text-left px-4 py-3">
                    Price
                  </th>

                  <th className="text-left px-4 py-3">
                    Stock
                  </th>

                  <th className="text-left px-4 py-3">
                    Category
                  </th>

                  <th className="text-right px-4 py-3">
                    Actions
                  </th>
                </tr>
              </thead>

              <tbody>
                {products.length === 0 ? (
                  <tr>
                    <td
                      colSpan={6}
                      className="px-4 py-8 text-center text-gray-500"
                    >
                      No products found.
                    </td>
                  </tr>
                ) : (
                  products.map((product) => (
                    <tr
                      key={product.id}
                      className="border-b last:border-b-0"
                    >
                      <td className="px-4 py-4 font-medium">
                        {product.name}
                      </td>

                      <td className="px-4 py-4 max-w-xs">
                        <span className="line-clamp-2 text-gray-600">
                          {product.description ||
                            "-"}
                        </span>
                      </td>

                      <td className="px-4 py-4">
                        {product.price.toFixed(2)}
                      </td>

                      <td className="px-4 py-4">
                        {product.stockQuantity ?? 0}
                      </td>

                      <td className="px-4 py-4">
                        {getCategoryName(
                          product.categoryId,
                        )}
                      </td>

                      <td className="px-4 py-4">
                        <div className="flex justify-end">
                          <button
                            type="button"
                            disabled={
                              deletingProductId ===
                              product.id
                            }
                            onClick={() =>
                              handleDeleteProduct(
                                product,
                              )
                            }
                            className="px-3 py-2 rounded bg-red-600 text-white hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            {deletingProductId ===
                            product.id
                              ? "Deleting..."
                              : "Delete"}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </section>
    </main>
  );
}
"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

import { api, getClientUserId, Category, Product } from "@/src/lib/api";

import { useAuthStore } from "@/src/store/authStore";

import AdminProductsHeader from "../../components/Admin/Products/AdminProductsHeader";
import AdminProductsError from "../../components/Admin/Products/AdminProductsError";
import AdminProductsSuccess from "../../components/Admin/Products/AdminProductsSuccess";
import AdminProductsCreateForm from "../../components/Admin/Products/AdminProductsCreateForm";
import AdminProductsTable from "../../components/Admin/Products/AdminProductsTable";
import LoadingAdminProducts from "../../components/Admin/Products/LoadingAdminProducts";
import DeleteProductModal from "@/app/components/Admin/Products/DeleteProductModal";

const emptyForm = {
  name: "",
  description: "",
  price: "",
  stockQuantity: "0",
  categoryId: "",
};

export default function AdminProductsPage() {
  const router = useRouter();
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);

  const [authorized, setAuthorized] = useState(false);
  const [loading, setLoading] = useState(true);

  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);

  const [form, setForm] = useState(emptyForm);
  const [editingForm, setEditingForm] = useState(emptyForm);

  const [editingProductId, setEditingProductId] = useState<string | null>(null);
  const [updatingProductId, setUpdatingProductId] = useState<string | null>(
    null,
  );
  const [deletingProductId, setDeletingProductId] = useState<string | null>(
    null,
  );

  const [creating, setCreating] = useState(false);

  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);

  // -----------------------------------------------------
  // LOAD ADMIN PAGE
  // -----------------------------------------------------
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

        const currentUser = await api.getUserById(currentUserId);
        const isAdmin =
          currentUser.roles?.some((r) => r.trim().toLowerCase() === "admin") ??
          false;

        if (!isAdmin) {
          if (!cancelled) setAuthorized(false);
          router.replace("/");
          return;
        }

        if (!cancelled) setAuthorized(true);

        const [productsResult, categoriesResult] = await Promise.all([
          api.getProducts(),
          api.getCategories(),
        ]);

        if (!cancelled) {
          setProducts(productsResult);
          setCategories(categoriesResult);
        }
      } catch (err) {
        if (!cancelled) {
          setError(
            err instanceof Error ? err.message : "Failed to load products.",
          );
        }
      } finally {
        if (!cancelled) setLoading(false);
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

  // -----------------------------------------------------
  // FORM HELPERS
  // -----------------------------------------------------
  function updateField(field: string, value: string) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  function updateEditingField(field: string, value: string) {
    setEditingForm((prev) => ({ ...prev, [field]: value }));
  }

  function validateProductForm(productForm: typeof emptyForm): string | null {
    const name = productForm.name.trim();
    const categoryId = productForm.categoryId.trim();

    const price = Number(productForm.price);
    const stockQuantity = Number(productForm.stockQuantity);

    if (!name) return "Product name is required.";
    if (!Number.isFinite(price) || price <= 0)
      return "Price must be greater than zero.";
    if (!Number.isInteger(stockQuantity) || stockQuantity < 0)
      return "Stock quantity must be a whole number ≥ 0.";

    if (categoryId && !categories.some((c) => c.id === categoryId))
      return "Please select a valid category.";

    return null;
  }

  // -----------------------------------------------------
  // CREATE PRODUCT
  // -----------------------------------------------------
  async function handleCreateProduct(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    setSuccess(null);

    const validationError = validateProductForm(form);
    if (validationError) {
      setError(validationError);
      return;
    }

    const name = form.name.trim();
    const description = form.description.trim();
    const categoryId = form.categoryId.trim();
    const price = Number(form.price);
    const stockQuantity = Number(form.stockQuantity);

    setCreating(true);

    try {
      const created = await api.createProduct({
        name,
        description: description || null,
        price,
        stockQuantity,
        categoryId: categoryId || null,
      });

      setProducts((prev) => [created, ...prev]);
      setForm(emptyForm);

      setSuccess(`Product "${created.name}" created successfully.`);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to create product.",
      );
    } finally {
      setCreating(false);
    }
  }

  // -----------------------------------------------------
  // EDIT PRODUCT
  // -----------------------------------------------------
  function startEditingProduct(product: Product) {
    setError(null);
    setSuccess(null);

    setEditingProductId(product.id);
    setEditingForm({
      name: product.name ?? "",
      description: product.description ?? "",
      price: String(product.price ?? ""),
      stockQuantity: String(product.stockQuantity ?? 0),
      categoryId: product.categoryId ?? "",
    });
  }

  function cancelEditingProduct() {
    if (updatingProductId) return;
    setEditingProductId(null);
    setEditingForm(emptyForm);
    setError(null);
  }

  async function handleUpdateProduct(product: Product) {
    setError(null);
    setSuccess(null);

    const validationError = validateProductForm(editingForm);
    if (validationError) {
      setError(validationError);
      return;
    }

    const name = editingForm.name.trim();
    const description = editingForm.description.trim();
    const categoryId = editingForm.categoryId.trim();
    const price = Number(editingForm.price);
    const stockQuantity = Number(editingForm.stockQuantity);

    setUpdatingProductId(product.id);

    try {
      const updated = await api.updateProduct(product.id, {
        name,
        description: description || null,
        price,
        stockQuantity,
        categoryId: categoryId || null,
      });

      setProducts((prev) =>
        prev.map((p) => (p.id === product.id ? updated : p)),
      );

      setEditingProductId(null);
      setEditingForm(emptyForm);

      setSuccess(`Product "${updated.name}" updated successfully.`);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to update product.",
      );
    } finally {
      setUpdatingProductId(null);
    }
  }

  // -----------------------------------------------------
  // DELETE PRODUCT
  // -----------------------------------------------------
  async function handleDeleteProduct(product: Product) {
    setDeletingProductId(product.id);
    setError(null);
    setSuccess(null);

    try {
      await api.deleteProduct(product.id);

      setProducts((prev) => prev.filter((p) => p.id !== product.id));

      if (editingProductId === product.id) {
        setEditingProductId(null);
        setEditingForm(emptyForm);
      }

      setSuccess(`Product "${product.name}" deleted successfully.`);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to delete product.",
      );
    } finally {
      setDeletingProductId(null);
    }
  }

  // -----------------------------------------------------
  // CATEGORY NAME
  // -----------------------------------------------------
  function getCategoryName(categoryId?: string | null): string {
    if (!categoryId) return "-";
    const category = categories.find((c) => c.id === categoryId);
    return category?.name ?? "Unknown category";
  }

  // -----------------------------------------------------
  // RENDER
  // -----------------------------------------------------
  if (loading) {
    return (
      <main className="max-w-6xl mx-auto px-4 py-10">
        <LoadingAdminProducts />
      </main>
    );
  }

  if (!isLoggedIn || !authorized) return null;

  return (
    <main className="max-w-6xl mx-auto px-4 py-10">
      <AdminProductsHeader />

      {error && <AdminProductsError message={error} />}
      {success && <AdminProductsSuccess message={success} />}

      <AdminProductsCreateForm
        form={form}
        categories={categories}
        creating={creating}
        onChange={updateField}
        onSubmit={handleCreateProduct}
      />

      <AdminProductsTable
        products={products}
        categories={categories}
        editingProductId={editingProductId}
        editingForm={editingForm}
        updatingProductId={updatingProductId}
        deletingProductId={deletingProductId}
        onEdit={startEditingProduct}
        onCancelEdit={cancelEditingProduct}
        onSaveEdit={handleUpdateProduct}
        onDeleteRequest={(product) => setSelectedProduct(product)}
        onChangeEditField={updateEditingField}
        getCategoryName={getCategoryName}
      />

      {selectedProduct && (
        <DeleteProductModal
          productName={selectedProduct.name}
          onConfirm={() => {
            handleDeleteProduct(selectedProduct);
            setSelectedProduct(null);
          }}
          onCancel={() => setSelectedProduct(null)}
        />
      )}
    </main>
  );
}
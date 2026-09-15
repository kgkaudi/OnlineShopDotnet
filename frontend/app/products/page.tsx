"use client";

import { useEffect, useState } from "react";
import Container from "../components/Container";
import { api } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

export default function ProductsPage() {
  const [products, setProducts] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [addingProductId, setAddingProductId] = useState<string | null>(null);
  const [error, setError] = useState("");
  const { showSnackbar } = useSnackbar();

  async function addToCart(productId: string) {
    try {
      setAddingProductId(productId);

      const token = localStorage.getItem("token");

      if (!token) {
        showSnackbar("You must be logged in.", "error");
        return;
      }

      await api.addToCart(productId, 1);
      showSnackbar("Added to cart 🛒", "success");
    } catch (err) {
      console.error(err);
      showSnackbar(
        err instanceof Error
          ? err.message
          : "Something went wrong while adding to cart.",
        "error"
      );
    } finally {
      setAddingProductId(null);
    }
  }

  async function addToWishlist(productId: string) {
    try {
      const token = localStorage.getItem("token");

      if (!token) {
        showSnackbar("You must be logged in.", "error");
        return;
      }

      const res = await fetch("http://localhost:5000/api/wishlist/add", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ productId }),
      });

      if (!res.ok) {
        const msg = await res.text();
        showSnackbar(msg, "error");
        return;
      }

      showSnackbar("Added to wishlist ❤️", "success");
    } catch (err) {
      console.error(err);
      showSnackbar("Something went wrong while adding to wishlist.", "error");
    }
  }

  useEffect(() => {
    api
      .getProducts()
      .then((items) => setProducts(items))
      .catch((err) => setError(err.message || "Failed to load products"))
      .finally(() => setLoading(false));
  }, []);

  return (
    <Container>
      <h1 className="text-2xl font-bold mb-6">Products</h1>

      {loading && <p className="text-gray-600">Loading products...</p>}

      {error && (
        <p className="text-red-600 bg-red-100 px-3 py-2 rounded">{error}</p>
      )}

      {!loading && !error && products.length === 0 && (
        <p className="text-gray-600">No products found.</p>
      )}

      {!loading && !error && products.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {products.map((product) => (
            <div
              key={product.id}
              className="border rounded p-4 bg-white shadow-sm hover:shadow-md transition"
            >
              <h2 className="font-semibold text-lg">{product.name}</h2>

              <p className="text-gray-600 mt-1">
                {product.description || "No description available."}
              </p>

              <p className="text-black font-bold mt-3">
                €{Number(product.price).toFixed(2)}
              </p>

              {typeof product.stockQuantity === "number" && (
                <p className="text-sm text-gray-500 mt-1">
                  {product.stockQuantity > 0
                    ? `${product.stockQuantity} in stock`
                    : "Out of stock"}
                </p>
              )}

              <button
                onClick={() => addToCart(product.id)}
                disabled={
                  addingProductId === product.id ||
                  product.stockQuantity === 0
                }
                className="mt-4 w-full bg-black text-white py-3 rounded text-sm sm:text-base disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {addingProductId === product.id
                  ? "Adding..."
                  : "Add to Cart"}
              </button>

              <button
                onClick={() => addToWishlist(product.id)}
                className="mt-2 w-full border border-gray-300 py-3 rounded text-sm sm:text-base hover:bg-gray-100"
              >
                ❤️ Add to Wishlist
              </button>
            </div>
          ))}
        </div>
      )}
    </Container>
  );
}

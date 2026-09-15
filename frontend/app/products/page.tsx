"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import Container from "../components/Container";
import { api, type Product } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [quantities, setQuantities] = useState<Record<string, number>>({});
  const [loading, setLoading] = useState(true);
  const [addingProductId, setAddingProductId] = useState<string | null>(null);
  const [wishlistProductId, setWishlistProductId] = useState<string | null>(null);
  const [error, setError] = useState("");
  const { showSnackbar } = useSnackbar();

  function getQuantity(productId: string) {
    return quantities[productId] ?? 1;
  }

  function changeQuantity(productId: string, value: number, stockQuantity?: number) {
    const safeValue = Number.isFinite(value) ? Math.floor(value) : 1;
    let nextQuantity = Math.max(1, safeValue);

    if (typeof stockQuantity === "number") {
      nextQuantity = Math.min(nextQuantity, stockQuantity);
    }

    setQuantities((current) => ({ ...current, [productId]: nextQuantity }));
  }

  async function addToCart(productId: string) {
    try {
      const product = products.find((item) => item.id === productId);
      const quantity = getQuantity(productId);

      if (product?.stockQuantity === 0) {
        showSnackbar("This product is out of stock.", "error");
        return;
      }

      if (typeof product?.stockQuantity === "number" && quantity > product.stockQuantity) {
        showSnackbar(`Only ${product.stockQuantity} item(s) are available.`, "error");
        return;
      }

      const token = localStorage.getItem("token");
      if (!token) {
        showSnackbar("You must be logged in.", "error");
        return;
      }

      setAddingProductId(productId);
      await api.addToCart(productId, quantity);
      showSnackbar(
        `${quantity} ${quantity === 1 ? "item" : "items"} added to cart 🛒`,
        "success"
      );
    } catch (err) {
      console.error(err);
      showSnackbar(
        err instanceof Error ? err.message : "Something went wrong while adding to cart.",
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

      setWishlistProductId(productId);
      await api.addToWishlist(productId);
      showSnackbar("Added to wishlist ❤️", "success");
    } catch (err) {
      console.error(err);
      showSnackbar(
        err instanceof Error ? err.message : "Something went wrong while adding to wishlist.",
        "error"
      );
    } finally {
      setWishlistProductId(null);
    }
  }

  useEffect(() => {
    api
      .getProducts()
      .then((items) => {
        setProducts(items);
        const initialQuantities: Record<string, number> = {};
        items.forEach((product) => {
          initialQuantities[product.id] = 1;
        });
        setQuantities(initialQuantities);
      })
      .catch((err) => setError(err.message || "Failed to load products"))
      .finally(() => setLoading(false));
  }, []);

  return (
    <Container>
      <div className="flex items-end justify-between gap-4 mb-6">
        <div>
          <h1 className="text-3xl font-bold">Products</h1>
          <p className="text-gray-600 mt-1">Browse our products and choose what you want to buy.</p>
        </div>
        <Link href="/wishlist" className="hidden sm:inline-flex text-sm font-medium hover:underline">
          View Wishlist ❤️
        </Link>
      </div>

      {loading && <p className="text-gray-600">Loading products...</p>}

      {error && <p className="text-red-600 bg-red-100 px-3 py-2 rounded">{error}</p>}

      {!loading && !error && products.length === 0 && (
        <p className="text-gray-600">No products found.</p>
      )}

      {!loading && !error && products.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {products.map((product) => {
            const quantity = getQuantity(product.id);
            const outOfStock = product.stockQuantity === 0;
            const maxStock = product.stockQuantity;

            return (
              <article
                key={product.id}
                className="border rounded-2xl p-5 bg-white shadow-sm hover:shadow-lg transition flex flex-col"
              >
                <Link href={`/products/${product.id}`} className="group">
                  <div className="h-44 rounded-xl bg-gray-100 flex items-center justify-center mb-4">
                    <span className="text-6xl group-hover:scale-110 transition">🛍️</span>
                  </div>
                  <h2 className="font-semibold text-xl group-hover:underline">{product.name}</h2>
                </Link>

                <p className="text-gray-600 mt-2 line-clamp-3 flex-1">
                  {product.description || "No description available."}
                </p>

                <div className="mt-4 flex items-center justify-between gap-3">
                  <p className="text-xl font-bold">€{Number(product.price).toFixed(2)}</p>
                  {typeof product.stockQuantity === "number" && (
                    <p className={`text-sm ${outOfStock ? "text-red-600" : "text-gray-500"}`}>
                      {outOfStock ? "Out of stock" : `${product.stockQuantity} in stock`}
                    </p>
                  )}
                </div>

                {!outOfStock && (
                  <div className="mt-4">
                    <label htmlFor={`quantity-${product.id}`} className="block text-sm font-medium mb-2">
                      Quantity
                    </label>
                    <div className="flex items-center gap-2">
                      <button
                        type="button"
                        onClick={() => changeQuantity(product.id, quantity - 1, maxStock)}
                        disabled={quantity <= 1}
                        className="w-10 h-10 border rounded-lg hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
                      >
                        −
                      </button>
                      <input
                        id={`quantity-${product.id}`}
                        type="number"
                        min={1}
                        max={maxStock}
                        value={quantity}
                        onChange={(event) => changeQuantity(product.id, Number(event.target.value), maxStock)}
                        className="w-20 h-10 text-center border rounded-lg"
                      />
                      <button
                        type="button"
                        onClick={() => changeQuantity(product.id, quantity + 1, maxStock)}
                        disabled={typeof maxStock === "number" && quantity >= maxStock}
                        className="w-10 h-10 border rounded-lg hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
                      >
                        +
                      </button>
                    </div>
                  </div>
                )}

                <button
                  type="button"
                  onClick={() => addToCart(product.id)}
                  disabled={addingProductId === product.id || outOfStock}
                  className="mt-4 w-full bg-black text-white py-3 rounded-lg font-semibold hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {addingProductId === product.id ? "Adding..." : `Add ${quantity} to Cart`}
                </button>

                <button
                  type="button"
                  onClick={() => addToWishlist(product.id)}
                  disabled={wishlistProductId === product.id}
                  className="mt-2 w-full border border-gray-300 py-3 rounded-lg font-semibold hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {wishlistProductId === product.id ? "Adding..." : "❤️ Add to Wishlist"}
                </button>

                <Link
                  href={`/products/${product.id}`}
                  className="mt-3 text-center text-sm font-medium hover:underline"
                >
                  View product details →
                </Link>
              </article>
            );
          })}
        </div>
      )}
    </Container>
  );
}

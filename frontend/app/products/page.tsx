"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import Container from "../components/Container";
import { api } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

export default function ProductsPage() {
  const [products, setProducts] = useState<any[]>([]);
  const [quantities, setQuantities] = useState<Record<string, number>>({});
  const [loading, setLoading] = useState(true);
  const [addingProductId, setAddingProductId] = useState<string | null>(null);
  const [wishlistProductId, setWishlistProductId] = useState<string | null>(
    null,
  );
  const [wishlistIds, setWishlistIds] = useState<Set<string>>(new Set());
  const [error, setError] = useState("");
  const { showSnackbar } = useSnackbar();

  function getQuantity(productId: string) {
    return quantities[productId] ?? 1;
  }

  function changeQuantity(
    productId: string,
    quantity: number,
    stockQuantity?: number,
  ) {
    let nextQuantity = Math.max(1, quantity);

    if (typeof stockQuantity === "number") {
      nextQuantity = Math.min(nextQuantity, stockQuantity);
    }

    setQuantities((current) => ({
      ...current,
      [productId]: nextQuantity,
    }));
  }

  async function addToCart(productId: string) {
    try {
      const product = products.find((item) => item.id === productId);
      const quantity = getQuantity(productId);

      if (product?.stockQuantity === 0) {
        showSnackbar("This product is out of stock.", "error");
        return;
      }

      if (
        typeof product?.stockQuantity === "number" &&
        quantity > product.stockQuantity
      ) {
        showSnackbar(
          `Only ${product.stockQuantity} item(s) are available.`,
          "error",
        );
        return;
      }

      setAddingProductId(productId);

      const token = localStorage.getItem("token");

      if (!token) {
        showSnackbar("You must be logged in.", "error");
        return;
      }

      await api.addToCart(productId, quantity);

      showSnackbar(
        `${quantity} ${quantity === 1 ? "item" : "items"} added to cart 🛒`,
        "success",
      );
    } catch (err) {
      console.error(err);
      showSnackbar(
        err instanceof Error
          ? err.message
          : "Something went wrong while adding to cart.",
        "error",
      );
    } finally {
      setAddingProductId(null);
    }
  }

  async function toggleWishlist(productId: string) {
    try {
      const token = localStorage.getItem("token");
      if (!token) {
        showSnackbar("You must be logged in.", "error");
        return;
      }

      const isWishlisted = wishlistIds.has(productId);
      setWishlistProductId(productId);

      if (isWishlisted) {
        await api.removeFromWishlist(productId);
        setWishlistIds((current) => {
          const next = new Set(current);
          next.delete(productId);
          return next;
        });
        showSnackbar("Removed from wishlist ❤️", "success");
      } else {
        await api.addToWishlist(productId);
        setWishlistIds((current) => new Set(current).add(productId));
        showSnackbar("Added to wishlist ❤️", "success");
      }
    } catch (err) {
      console.error(err);
      showSnackbar(
        err instanceof Error
          ? err.message
          : "Something went wrong while updating your wishlist.",
        "error",
      );
    } finally {
      setWishlistProductId(null);
    }
  }

  useEffect(() => {
    api
      .getProducts()
      .then(async (items) => {
        // Fetch average rating for each product
        const enriched = await Promise.all(
          items.map(async (product) => {
            try {
              const reviews = await api.getReviews(product.id);
              const avg =
                reviews.length > 0
                  ? (
                      reviews.reduce((sum, r) => sum + r.rating, 0) /
                      reviews.length
                    ).toFixed(1)
                  : null;
              return { ...product, averageRating: avg };
            } catch {
              return { ...product, averageRating: null };
            }
          }),
        );

        setProducts(enriched);

        const initialQuantities: Record<string, number> = {};
        enriched.forEach((product) => {
          initialQuantities[product.id] = 1;
        });
        setQuantities(initialQuantities);
      })
      .catch((err) => setError(err.message || "Failed to load products"))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    if (!localStorage.getItem("token")) return;

    api
      .getWishlist()
      .then((wishlist) => {
        setWishlistIds(new Set(wishlist.map((item) => item.productId)));
      })
      .catch((err) => {
        console.error("Failed to load wishlist:", err);
      });
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
          {products.map((product) => {
            const quantity = getQuantity(product.id);
            const outOfStock = product.stockQuantity === 0;
            const maxStock =
              typeof product.stockQuantity === "number"
                ? product.stockQuantity
                : undefined;

            return (
              <div
                key={product.id}
                className="border rounded p-4 bg-white shadow-sm hover:shadow-md transition"
              >
                <h2 className="font-semibold text-lg">{product.name}</h2>

                <p className="text-gray-600 mt-1">
                  {product.description || "No description available."}
                </p>

                {product.averageRating && (
                  <p className="text-yellow-500 font-semibold mt-1">
                    ⭐ {product.averageRating}/5
                  </p>
                )}

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

                {!outOfStock && (
                  <div className="mt-4">
                    <label
                      htmlFor={`quantity-${product.id}`}
                      className="block text-sm font-medium text-gray-700 mb-2"
                    >
                      Quantity
                    </label>

                    <div className="flex items-center gap-2">
                      <button
                        type="button"
                        onClick={() =>
                          changeQuantity(product.id, quantity - 1, maxStock)
                        }
                        disabled={quantity <= 1}
                        className="w-10 h-10 border border-gray-300 rounded hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
                        aria-label={`Decrease quantity of ${product.name}`}
                      >
                        −
                      </button>

                      <input
                        id={`quantity-${product.id}`}
                        type="number"
                        min={1}
                        max={maxStock}
                        value={quantity}
                        onChange={(e) =>
                          changeQuantity(
                            product.id,
                            Number(e.target.value),
                            maxStock,
                          )
                        }
                        className="w-20 h-10 text-center border border-gray-300 rounded"
                      />

                      <button
                        type="button"
                        onClick={() =>
                          changeQuantity(product.id, quantity + 1, maxStock)
                        }
                        disabled={
                          typeof maxStock === "number" && quantity >= maxStock
                        }
                        className="w-10 h-10 border border-gray-300 rounded hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
                        aria-label={`Increase quantity of ${product.name}`}
                      >
                        +
                      </button>
                    </div>
                  </div>
                )}

                <button
                  onClick={() => addToCart(product.id)}
                  disabled={
                    addingProductId === product.id || outOfStock || quantity < 1
                  }
                  className="mt-4 w-full bg-black text-white py-3 rounded text-sm sm:text-base disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {addingProductId === product.id
                    ? "Adding..."
                    : `Add ${quantity} to Cart`}
                </button>

                <button
                  type="button"
                  onClick={() => toggleWishlist(product.id)}
                  disabled={wishlistProductId === product.id}
                  className="mt-2 w-full border border-gray-300 py-3 rounded text-sm sm:text-base hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {wishlistProductId === product.id
                    ? "Updating..."
                    : wishlistIds.has(product.id)
                      ? "❤️ Remove from Wishlist"
                      : "♡ Add to Wishlist"}
                </button>

                <Link
                  href={`/products/${product.id}`}
                  className="mt-2 block w-full border border-black py-3 rounded text-center text-sm sm:text-base font-medium hover:bg-black hover:text-white transition"
                >
                  View Product Details →
                </Link>
              </div>
            );
          })}
        </div>
      )}
    </Container>
  );
}

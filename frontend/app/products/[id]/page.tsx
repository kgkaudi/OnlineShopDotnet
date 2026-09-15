"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import Container from "../../components/Container";
import { api, type Product } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

export default function ProductDetailsPage() {
  const params = useParams<{ id: string }>();
  const productId = params?.id;
  const { showSnackbar } = useSnackbar();

  const [product, setProduct] = useState<Product | null>(null);
  const [quantity, setQuantity] = useState(1);
  const [loading, setLoading] = useState(true);
  const [addingToCart, setAddingToCart] = useState(false);
  const [addingToWishlist, setAddingToWishlist] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!productId) return;

    setLoading(true);
    setError("");

    api
      .getProduct(productId)
      .then((item) => {
        setProduct(item);
        setQuantity(1);
      })
      .catch((err) => {
        setError(err instanceof Error ? err.message : "Failed to load product.");
      })
      .finally(() => setLoading(false));
  }, [productId]);

  function updateQuantity(value: number) {
    if (!product) return;

    const max = product.stockQuantity;
    const safeValue = Number.isFinite(value) ? Math.floor(value) : 1;
    const minimum = Math.max(1, safeValue);

    setQuantity(
      typeof max === "number" ? Math.min(minimum, Math.max(0, max)) : minimum
    );
  }

  async function handleAddToCart() {
    if (!product) return;

    const token = localStorage.getItem("token");
    if (!token) {
      showSnackbar("You must be logged in.", "error");
      return;
    }

    if (product.stockQuantity === 0) {
      showSnackbar("This product is out of stock.", "error");
      return;
    }

    try {
      setAddingToCart(true);
      await api.addToCart(product.id, quantity);
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
      setAddingToCart(false);
    }
  }

  async function handleAddToWishlist() {
    if (!product) return;

    const token = localStorage.getItem("token");
    if (!token) {
      showSnackbar("You must be logged in.", "error");
      return;
    }

    try {
      setAddingToWishlist(true);
      await api.addToWishlist(product.id);
      showSnackbar("Added to wishlist ❤️", "success");
    } catch (err) {
      console.error(err);
      showSnackbar(
        err instanceof Error ? err.message : "Something went wrong while adding to wishlist.",
        "error"
      );
    } finally {
      setAddingToWishlist(false);
    }
  }

  if (loading) {
    return (
      <Container>
        <div className="py-12 text-center text-gray-600">Loading product...</div>
      </Container>
    );
  }

  if (error || !product) {
    return (
      <Container>
        <div className="max-w-2xl mx-auto py-12 text-center">
          <div className="text-6xl mb-4">😕</div>
          <h1 className="text-2xl font-bold">Product not found</h1>
          <p className="text-gray-600 mt-2">
            {error || "We couldn't find the product you're looking for."}
          </p>
          <Link
            href="/products"
            className="inline-block mt-6 bg-black text-white px-6 py-3 rounded-lg hover:bg-gray-800"
          >
            Back to Products
          </Link>
        </div>
      </Container>
    );
  }

  const outOfStock = product.stockQuantity === 0;
  const hasStockLimit = typeof product.stockQuantity === "number";

  return (
    <Container>
      <div className="mb-6">
        <Link href="/products" className="text-sm text-gray-600 hover:text-black">
          ← Back to Products
        </Link>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-10 items-start">
        <div className="bg-white border rounded-2xl overflow-hidden shadow-sm">
          <div className="aspect-square bg-gray-100 flex items-center justify-center">
            <div className="text-center px-8">
              <div className="text-8xl mb-5">🛍️</div>
              <p className="text-sm text-gray-500">Product image</p>
            </div>
          </div>
        </div>

        <div className="bg-white border rounded-2xl p-6 sm:p-8 shadow-sm">
          <div className="flex items-start justify-between gap-4">
            <div>
              <p className="text-sm font-medium text-gray-500 uppercase tracking-wide">
                Product details
              </p>
              <h1 className="text-3xl sm:text-4xl font-bold mt-2">{product.name}</h1>
            </div>

            {outOfStock ? (
              <span className="shrink-0 bg-red-100 text-red-700 text-sm font-semibold px-3 py-1.5 rounded-full">
                Out of stock
              </span>
            ) : (
              <span className="shrink-0 bg-green-100 text-green-700 text-sm font-semibold px-3 py-1.5 rounded-full">
                In stock
              </span>
            )}
          </div>

          <div className="mt-6 text-3xl font-bold">
            €{Number(product.price).toFixed(2)}
          </div>

          <div className="mt-6 border-t pt-6">
            <h2 className="font-semibold text-lg">Description</h2>
            <p className="text-gray-600 leading-7 mt-2">
              {product.description || "No description is available for this product."}
            </p>
          </div>

          {hasStockLimit && (
            <div className="mt-6 rounded-lg bg-gray-50 border p-4">
              <p className="font-medium">
                {outOfStock ? "Currently unavailable" : `${product.stockQuantity} item(s) available`}
              </p>
              {!outOfStock && (
                <p className="text-sm text-gray-500 mt-1">
                  Choose the quantity you want to add to your cart.
                </p>
              )}
            </div>
          )}

          {!outOfStock && (
            <div className="mt-7">
              <label htmlFor="product-quantity" className="block text-sm font-semibold mb-2">
                Quantity
              </label>

              <div className="flex items-center gap-3">
                <button
                  type="button"
                  onClick={() => updateQuantity(quantity - 1)}
                  disabled={quantity <= 1}
                  className="w-12 h-12 border rounded-lg text-xl hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
                  aria-label="Decrease quantity"
                >
                  −
                </button>

                <input
                  id="product-quantity"
                  type="number"
                  min={1}
                  max={product.stockQuantity}
                  value={quantity}
                  onChange={(event) => updateQuantity(Number(event.target.value))}
                  className="w-24 h-12 text-center border rounded-lg text-lg font-semibold"
                />

                <button
                  type="button"
                  onClick={() => updateQuantity(quantity + 1)}
                  disabled={hasStockLimit && quantity >= product.stockQuantity!}
                  className="w-12 h-12 border rounded-lg text-xl hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
                  aria-label="Increase quantity"
                >
                  +
                </button>
              </div>
            </div>
          )}

          <div className="mt-8 space-y-3">
            <button
              type="button"
              onClick={handleAddToCart}
              disabled={outOfStock || addingToCart}
              className="w-full bg-black text-white py-4 rounded-lg font-semibold hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed transition"
            >
              {addingToCart ? "Adding to Cart..." : `Add ${quantity} to Cart 🛒`}
            </button>

            <button
              type="button"
              onClick={handleAddToWishlist}
              disabled={addingToWishlist}
              className="w-full border border-gray-300 py-4 rounded-lg font-semibold hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
            >
              {addingToWishlist ? "Adding to Wishlist..." : "❤️ Add to Wishlist"}
            </button>
          </div>

          <div className="mt-7 pt-6 border-t text-sm text-gray-500 space-y-2">
            <p>✓ Secure shopping experience</p>
            <p>✓ Add multiple quantities directly from this page</p>
            <p>✓ Save products to your wishlist for later</p>
          </div>
        </div>
      </div>
    </Container>
  );
}

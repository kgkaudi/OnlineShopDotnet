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
  const [categoryName, setCategoryName] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [addingToCart, setAddingToCart] = useState(false);
  const [updatingWishlist, setUpdatingWishlist] = useState(false);
  const [isWishlisted, setIsWishlisted] = useState(false);
  const [error, setError] = useState("");

  // -----------------------------
  // Reviews state
  // -----------------------------
  const [reviews, setReviews] = useState<any[]>([]);
  const [loadingReviews, setLoadingReviews] = useState(true);
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewComment, setReviewComment] = useState("");
  const [submittingReview, setSubmittingReview] = useState(false);

  const averageRating =
    reviews.length > 0
      ? (
          reviews.reduce((sum, r) => sum + r.rating, 0) / reviews.length
        ).toFixed(1)
      : null;

  // -----------------------------
  // Load product
  // -----------------------------
  useEffect(() => {
    if (!productId) return;

    setLoading(true);
    setError("");

    api
      .getProduct(productId)
      .then(async (item) => {
        setProduct(item);
        setQuantity(1);

        // Fetch category name
        if (item.categoryId) {
          try {
            const category = await api.getCategory(item.categoryId);
            setCategoryName(category.name);
          } catch {
            setCategoryName("Uncategorized");
          }
        } else {
          setCategoryName("Uncategorized");
        }
      })
      .catch((err) => {
        setError(
          err instanceof Error ? err.message : "Failed to load product.",
        );
      })
      .finally(() => setLoading(false));
  }, [productId]);

  // -----------------------------
  // Load wishlist status
  // -----------------------------
  useEffect(() => {
    if (!productId) return;

    const token = localStorage.getItem("token");
    if (!token) {
      setIsWishlisted(false);
      return;
    }

    api
      .getWishlist()
      .then((wishlist) => {
        setIsWishlisted(wishlist.some((item) => item.productId === productId));
      })
      .catch((err) => {
        console.error("Failed to load wishlist:", err);
      });
  }, [productId]);

  // -----------------------------
  // Load reviews
  // -----------------------------
  useEffect(() => {
    if (!productId) return;

    setLoadingReviews(true);

    api
      .getReviews(productId)
      .then(async (data) => {
        // Fetch usernames for each review
        const enriched = await Promise.all(
          data.map(async (review) => {
            try {
              const user = await api.getUserProfile(review.userId);
              return {
                ...review,
                userName: user.fullName || "Unknown user",
              };
            } catch {
              return {
                ...review,
                userName: "Unknown user",
              };
            }
          }),
        );

        setReviews(enriched);
      })
      .catch((err) => {
        console.error("Failed to load reviews:", err);
      })
      .finally(() => setLoadingReviews(false));
  }, [productId]);

  // -----------------------------
  // Quantity update
  // -----------------------------
  function updateQuantity(value: number) {
    if (!product) return;

    const max = product.stockQuantity;
    const safeValue = Number.isFinite(value) ? Math.floor(value) : 1;
    const minimum = Math.max(1, safeValue);

    setQuantity(
      typeof max === "number" ? Math.min(minimum, Math.max(0, max)) : minimum,
    );
  }

  // -----------------------------
  // Add to cart
  // -----------------------------
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
      setAddingToCart(false);
    }
  }

  // -----------------------------
  // Wishlist toggle
  // -----------------------------
  async function toggleWishlist() {
    if (!product) return;

    const token = localStorage.getItem("token");
    if (!token) {
      showSnackbar("You must be logged in.", "error");
      return;
    }

    try {
      setUpdatingWishlist(true);

      if (isWishlisted) {
        await api.removeFromWishlist(product.id);
        setIsWishlisted(false);
        showSnackbar("Removed from wishlist ❤️", "success");
      } else {
        await api.addToWishlist(product.id);
        setIsWishlisted(true);
        showSnackbar("Added to wishlist ❤️", "success");
      }
    } catch (err) {
      console.error(err);
      showSnackbar(
        err instanceof Error
          ? err.message
          : "Something went wrong while updating wishlist.",
        "error",
      );
    } finally {
      setUpdatingWishlist(false);
    }
  }

  // -----------------------------
  // Submit review (with username)
  // -----------------------------
  async function submitReview() {
    const token = localStorage.getItem("token");
    if (!token) {
      showSnackbar("You must be logged in to write a review.", "error");
      return;
    }

    if (!reviewComment.trim()) {
      showSnackbar("Review comment cannot be empty.", "error");
      return;
    }

    try {
      setSubmittingReview(true);

      // Submit review
      await api.addReview(productId!, reviewRating, reviewComment.trim());
      showSnackbar("Review submitted!", "success");

      // Fetch updated reviews
      const updated = await api.getReviews(productId!);

      // Enrich reviews with usernames
      const enriched = await Promise.all(
        updated.map(async (review) => {
          try {
            const user = await api.getUserProfile(review.userId);
            return {
              ...review,
              userName: user.fullName || "Unknown user",
            };
          } catch {
            return {
              ...review,
              userName: "Unknown user",
            };
          }
        }),
      );

      setReviews(enriched);

      // Reset form
      setReviewComment("");
      setReviewRating(5);
    } catch (err) {
      console.error(err);
      showSnackbar("Failed to submit review.", "error");
    } finally {
      setSubmittingReview(false);
    }
  }

  // -----------------------------
  // Loading / error states
  // -----------------------------
  if (loading) {
    return (
      <Container>
        <div className="py-12 text-center text-gray-600">
          Loading product...
        </div>
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

  // -----------------------------
  // MAIN PAGE
  // -----------------------------
  return (
    <Container>
      <div className="mb-6">
        <Link
          href="/products"
          className="text-sm text-gray-600 hover:text-black"
        >
          ← Back to Products
        </Link>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-10 items-start">
        {/* IMAGE */}
        <div className="bg-white border rounded-2xl overflow-hidden shadow-sm">
          <div className="aspect-square bg-gray-100 flex items-center justify-center">
            <div className="text-center px-8">
              <div className="text-8xl mb-5">🛍️</div>
              <p className="text-sm text-gray-500">Product image</p>
            </div>
          </div>
        </div>

        {/* DETAILS */}
        <div className="bg-white border rounded-2xl p-6 sm:p-8 shadow-sm">
          <div className="flex items-start justify-between gap-4">
            <div>
              <p className="text-sm font-medium text-gray-500 uppercase tracking-wide">
                Product details
              </p>
              <h1 className="text-3xl sm:text-4xl font-bold mt-2">
                {product.name}
              </h1>
              {categoryName && (
                <p className="text-sm text-gray-500 mt-1">
                  Category: {categoryName}
                </p>
              )}
              {averageRating && (
                <p className="mt-1 text-lg text-yellow-500 font-semibold">
                  ⭐ {averageRating}/5
                  <span className="text-gray-600 text-sm ml-2">
                    ({reviews.length}{" "}
                    {reviews.length === 1 ? "review" : "reviews"})
                  </span>
                </p>
              )}
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
              {product.description ||
                "No description is available for this product."}
            </p>
          </div>

          {hasStockLimit && (
            <div className="mt-6 rounded-lg bg-gray-50 border p-4">
              <p className="font-medium">
                {outOfStock
                  ? "Currently unavailable"
                  : `${product.stockQuantity} item(s) available`}
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
              <label
                htmlFor="product-quantity"
                className="block text-sm font-semibold mb-2"
              >
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
                  onChange={(event) =>
                    updateQuantity(Number(event.target.value))
                  }
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
              {addingToCart
                ? "Adding to Cart..."
                : `Add ${quantity} to Cart 🛒`}
            </button>

            <button
              type="button"
              onClick={toggleWishlist}
              disabled={updatingWishlist}
              className="w-full border border-gray-300 py-4 rounded-lg font-semibold hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
            >
              {updatingWishlist
                ? "Updating Wishlist..."
                : isWishlisted
                  ? "❤️ Remove from Wishlist"
                  : "♡ Add to Wishlist"}
            </button>
          </div>

          <div className="mt-7 pt-6 border-t text-sm text-gray-500 space-y-2">
            <p>✓ Secure shopping experience</p>
            <p>✓ Add multiple quantities directly from this page</p>
            <p>✓ Save products to your wishlist for later</p>
          </div>

          {/* ----------------------------- */}
          {/* REVIEWS SECTION */}
          {/* ----------------------------- */}
          <div className="mt-14 border-t pt-10">
            <h2 className="text-2xl font-bold mb-6">Reviews</h2>

            {loadingReviews && (
              <p className="text-gray-600">Loading reviews...</p>
            )}

            {!loadingReviews && reviews.length === 0 && (
              <p className="text-gray-600">
                No reviews yet. Be the first to write one!
              </p>
            )}

            <div className="space-y-6">
              {reviews.map((r) => (
                <div key={r.id} className="border rounded-xl p-5 bg-gray-50">
                  <div className="flex items-center gap-3">
                    <span className="text-lg font-semibold">{r.rating}⭐</span>
                    <span className="text-sm text-gray-500">
                      <span className="text-sm text-gray-500">
                        {r.userName} •{" "}
                        {new Date(r.createdAt).toLocaleDateString("en-GB", {
                          day: "2-digit",
                          month: "short",
                          year: "numeric",
                        })}
                      </span>
                    </span>
                  </div>
                  <p className="mt-2 text-gray-700">{r.comment}</p>
                </div>
              ))}
            </div>

            {/* Write Review */}
            <div className="mt-10 border-t pt-10">
              <h3 className="text-xl font-bold mb-4">Write a Review</h3>

              <label className="block mb-2 font-medium">Rating</label>
              <select
                value={reviewRating}
                onChange={(e) => setReviewRating(Number(e.target.value))}
                className="border rounded-lg p-2"
              >
                {[1, 2, 3, 4, 5].map((n) => (
                  <option key={n} value={n}>
                    {n} ⭐
                  </option>
                ))}
              </select>

              <label className="block mt-4 mb-2 font-medium">Comment</label>
              <textarea
                value={reviewComment}
                onChange={(e) => setReviewComment(e.target.value)}
                className="border rounded-lg p-3 w-full"
                rows={4}
                placeholder="Share your experience..."
              />

              <button
                onClick={submitReview}
                disabled={submittingReview}
                className="mt-4 bg-black text-white px-6 py-3 rounded-lg hover:bg-gray-800 disabled:opacity-50"
              >
                {submittingReview ? "Submitting..." : "Submit Review"}
              </button>
            </div>
          </div>
        </div>
      </div>
    </Container>
  );
}

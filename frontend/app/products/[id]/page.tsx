"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import Container from "../../components/Container";
import { api, type Product } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

import ProductLoading from "@/app/components/ProductDetails/Loading/ProductLoading";
import ProductNotFound from "@/app/components/ProductDetails/Loading/ProductNotFound";
import ProductCarousel from "@/app/components/ProductDetails/Carousel/ProductCarousel";
import ProductHeader from "@/app/components/ProductDetails/Info/ProductHeader";
import ProductPrice from "@/app/components/ProductDetails/Info/ProductPrice";
import ProductDescription from "@/app/components/ProductDetails/Info/ProductDescription";
import ProductStockInfo from "@/app/components/ProductDetails/Info/ProductStockInfo";
import ProductQuantitySelector from "@/app/components/ProductDetails/Actions/ProductQuantitySelector";
import ProductAddToCartButton from "@/app/components/ProductDetails/Actions/ProductAddToCartButton";
import ProductWishlistButton from "@/app/components/ProductDetails/Actions/ProductWishlistButton";
import ProductReviewsList from "@/app/components/ProductDetails/Reviews/ProductReviewsList";
import ProductReviewForm from "@/app/components/ProductDetails/Reviews/ProductReviewForm";
import RecommendedProducts from "@/app/components/ProductDetails/Recommended/RecommendedProducts";

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

  useEffect(() => {
    if (!productId) return;

    setLoading(true);
    setError("");

    api
      .getProduct(productId)
      .then(async (item) => {
        setProduct(item);
        setQuantity(1);

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

  useEffect(() => {
    if (!productId) return;

    setLoadingReviews(true);

    api
      .getReviews(productId)
      .then(async (data) => {
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

  function updateQuantity(value: number) {
    if (!product) return;

    const max = product.stockQuantity;
    const safeValue = Number.isFinite(value) ? Math.floor(value) : 1;
    const minimum = Math.max(1, safeValue);

    setQuantity(
      typeof max === "number" ? Math.min(minimum, Math.max(0, max)) : minimum,
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

      await api.addReview(productId!, reviewRating, reviewComment.trim());
      showSnackbar("Review submitted!", "success");

      const updated = await api.getReviews(productId!);

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

      setReviewComment("");
      setReviewRating(5);
    } catch (err) {
      console.error(err);
      showSnackbar("Failed to submit review.", "error");
    } finally {
      setSubmittingReview(false);
    }
  }

  if (loading) {
    return (
      <Container>
        <ProductLoading />
      </Container>
    );
  }

  if (error || !product) {
    return (
      <Container>
        <ProductNotFound error={error || null} />
      </Container>
    );
  }

  const outOfStock = product.stockQuantity === 0;
  const hasStockLimit = typeof product.stockQuantity === "number";

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
        <div className="bg-white border rounded-2xl overflow-hidden shadow-sm">
          <ProductCarousel productId={product.id} productName={product.name} />
        </div>

        <div className="bg-white border rounded-2xl p-6 sm:p-8 shadow-sm">
          <ProductHeader
            product={product}
            categoryName={categoryName}
            averageRating={averageRating}
            reviewCount={reviews.length}
            outOfStock={outOfStock}
          />

          <ProductPrice price={product.price} />

          <ProductDescription product={product} />

          <ProductStockInfo
            product={product}
            outOfStock={outOfStock}
            hasStockLimit={hasStockLimit}
          />

          <ProductQuantitySelector
            product={product}
            quantity={quantity}
            hasStockLimit={hasStockLimit}
            outOfStock={outOfStock}
            updateQuantity={updateQuantity}
          />

          <div className="mt-8 space-y-3">
            <ProductAddToCartButton
              outOfStock={outOfStock}
              addingToCart={addingToCart}
              quantity={quantity}
              onAddToCart={handleAddToCart}
            />

            <ProductWishlistButton
              updatingWishlist={updatingWishlist}
              isWishlisted={isWishlisted}
              onToggleWishlist={toggleWishlist}
            />
          </div>

          <div className="mt-7 pt-6 border-t text-sm text-gray-500 space-y-2">
            <p>✓ Secure shopping experience</p>
            <p>✓ Add multiple quantities directly from this page</p>
            <p>✓ Save products to your wishlist for later</p>
          </div>

          <div className="mt-14 border-t pt-10">
            <h2 className="text-2xl font-bold mb-6">Reviews</h2>

            <ProductReviewsList
              reviews={reviews}
              loadingReviews={loadingReviews}
            />

            <ProductReviewForm
              reviewRating={reviewRating}
              reviewComment={reviewComment}
              submittingReview={submittingReview}
              setReviewRating={setReviewRating}
              setReviewComment={setReviewComment}
              onSubmit={submitReview}
            />
          </div>
        </div>
      </div>

      <div className="mt-20">
        <h2 className="text-2xl font-bold mb-6">You may also like</h2>
        <RecommendedProducts currentProductId={product.id} />
      </div>
    </Container>
  );
}

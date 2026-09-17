"use client";

import { useEffect, useState } from "react";
import Container from "../components/Container";
import ProductsGrid from "../components/Products/ProductsGrid";
import { api } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

export default function ProductsPage() {
  const [products, setProducts] = useState<any[]>([]);
  const [quantities, setQuantities] = useState<Record<string, number>>({});
  const [loading, setLoading] = useState(true);
  const [addingProductId, setAddingProductId] = useState<string | null>(null);
  const [wishlistProductId, setWishlistProductId] = useState<string | null>(null);
  const [wishlistIds, setWishlistIds] = useState<Set<string>>(new Set());
  const [error, setError] = useState("");
  const { showSnackbar } = useSnackbar();

  function getQuantity(productId: string) {
    return quantities[productId] ?? 1;
  }

  function changeQuantity(productId: string, quantity: number, stockQuantity?: number) {
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

      if (typeof product?.stockQuantity === "number" && quantity > product.stockQuantity) {
        showSnackbar(`Only ${product.stockQuantity} item(s) are available.`, "error");
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
        err instanceof Error ? err.message : "Something went wrong while updating your wishlist.",
        "error"
      );
    } finally {
      setWishlistProductId(null);
    }
  }

  useEffect(() => {
    api
      .getProducts()
      .then(async (items) => {
        const enriched = await Promise.all(
          items.map(async (product) => {
            try {
              const [reviews, category] = await Promise.all([
                api.getReviews(product.id),
                product.categoryId ? api.getCategory(product.categoryId) : null,
              ]);

              const validReviews = reviews.filter(
                (r) => typeof r.rating === "number" && !Number.isNaN(r.rating)
              );

              const avg =
                validReviews.length > 0
                  ? (
                      validReviews.reduce((sum, r) => sum + r.rating, 0) /
                      validReviews.length
                    ).toFixed(1)
                  : null;

              return {
                ...product,
                averageRating: avg,
                reviewCount: validReviews.length,
                categoryName: category ? category.name : "Uncategorized",
              };
            } catch {
              return {
                ...product,
                averageRating: null,
                reviewCount: 0,
                categoryName: "Uncategorized",
              };
            }
          })
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
        <ProductsGrid
          products={products}
          quantities={quantities}
          wishlistIds={wishlistIds}
          addingProductId={addingProductId}
          wishlistProductId={wishlistProductId}
          onChangeQuantity={changeQuantity}
          onAddToCart={addToCart}
          onToggleWishlist={toggleWishlist}
        />
      )}
    </Container>
  );
}

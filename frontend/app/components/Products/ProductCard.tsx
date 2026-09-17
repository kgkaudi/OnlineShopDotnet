"use client";

import Link from "next/link";
import AddToCartButton from "./AddToCartButton";
import WishlistButton from "./WishlistButton";
import QuantitySelector from "./QuantitySelector";

interface ProductCardProps {
  product: any;
  quantity: number;
  outOfStock: boolean;
  maxStock?: number;

  addingProductId: string | null;
  wishlistProductId: string | null;
  isWishlisted: boolean;

  onChangeQuantity: (
    productId: string,
    quantity: number,
    maxStock?: number
  ) => void;

  onAddToCart: (productId: string) => void;
  onToggleWishlist: (productId: string) => void;
}

export default function ProductCard({
  product,
  quantity,
  outOfStock,
  maxStock,
  addingProductId,
  wishlistProductId,
  isWishlisted,
  onChangeQuantity,
  onAddToCart,
  onToggleWishlist,
}: ProductCardProps) {
  return (
    <div className="border rounded p-4 bg-white shadow-sm hover:shadow-md transition">
      {/* Title */}
      <h2 className="font-semibold text-lg">{product.name}</h2>

      {/* Category */}
      <p className="text-sm text-gray-500">
        Category: {product.categoryName}
      </p>

      {/* Description */}
      <p className="text-gray-600 mt-1">
        {product.description || "No description available."}
      </p>

      {/* Rating */}
      {product.averageRating && (
        <p className="mt-1 text-lg text-yellow-500 font-semibold">
          ⭐ {product.averageRating}/5
          <span className="text-gray-600 text-sm ml-2">
            ({product.reviewCount}{" "}
            {product.reviewCount === 1 ? "review" : "reviews"})
          </span>
        </p>
      )}

      {/* Price */}
      <p className="text-black font-bold mt-3">
        €{Number(product.price).toFixed(2)}
      </p>

      {/* Stock */}
      {typeof product.stockQuantity === "number" && (
        <p className="text-sm text-gray-500 mt-1">
          {product.stockQuantity > 0
            ? `${product.stockQuantity} in stock`
            : "Out of stock"}
        </p>
      )}

      {/* Quantity Selector */}
      {!outOfStock && (
        <QuantitySelector
          productId={product.id}
          quantity={quantity}
          maxStock={maxStock}
          onChangeQuantity={onChangeQuantity}
        />
      )}

      {/* Add to Cart */}
      <AddToCartButton
        productId={product.id}
        quantity={quantity}
        outOfStock={outOfStock}
        addingProductId={addingProductId}
        onAddToCart={onAddToCart}
      />

      {/* Wishlist */}
      <WishlistButton
        productId={product.id}
        wishlistProductId={wishlistProductId}
        isWishlisted={isWishlisted}
        onToggleWishlist={onToggleWishlist}
      />

      {/* View Product */}
      <Link
        href={`/products/${product.id}`}
        className="mt-2 block w-full border border-black py-3 rounded text-center text-sm sm:text-base font-medium hover:bg-black hover:text-white transition"
      >
        View Product Details →
      </Link>
    </div>
  );
}

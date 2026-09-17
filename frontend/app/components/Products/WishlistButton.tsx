"use client";

interface WishlistButtonProps {
  productId: string;
  wishlistProductId: string | null;
  isWishlisted: boolean;
  onToggleWishlist: (productId: string) => void;
}

export default function WishlistButton({
  productId,
  wishlistProductId,
  isWishlisted,
  onToggleWishlist,
}: WishlistButtonProps) {
  const isUpdating = wishlistProductId === productId;

  return (
    <button
      type="button"
      onClick={() => onToggleWishlist(productId)}
      disabled={isUpdating}
      className="mt-2 w-full border border-gray-300 py-3 rounded text-sm sm:text-base hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
    >
      {isUpdating
        ? "Updating..."
        : isWishlisted
        ? "❤️ Remove from Wishlist"
        : "♡ Add to Wishlist"}
    </button>
  );
}

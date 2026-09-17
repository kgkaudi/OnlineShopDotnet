"use client";

interface Props {
  updatingWishlist: boolean;
  isWishlisted: boolean;
  onToggleWishlist: () => void;
}

export default function ProductWishlistButton({
  updatingWishlist,
  isWishlisted,
  onToggleWishlist,
}: Props) {
  return (
    <button
      type="button"
      onClick={onToggleWishlist}
      disabled={updatingWishlist}
      className="w-full border border-gray-300 py-4 rounded-lg font-semibold hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
    >
      {updatingWishlist
        ? "Updating Wishlist..."
        : isWishlisted
        ? "❤️ Remove from Wishlist"
        : "♡ Add to Wishlist"}
    </button>
  );
}

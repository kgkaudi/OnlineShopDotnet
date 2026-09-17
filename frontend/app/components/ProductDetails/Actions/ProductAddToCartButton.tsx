// frontend/app/components/ProductDetails/Actions/ProductAddToCartButton.tsx
"use client";

interface Props {
  outOfStock: boolean;
  addingToCart: boolean;
  quantity: number;
  onAddToCart: () => void;
}

export default function ProductAddToCartButton({
  outOfStock,
  addingToCart,
  quantity,
  onAddToCart,
}: Props) {
  return (
    <button
      type="button"
      onClick={onAddToCart}
      disabled={outOfStock || addingToCart}
      className="w-full bg-black text-white py-4 rounded-lg font-semibold hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed transition"
    >
      {addingToCart ? "Adding to Cart..." : `Add ${quantity} to Cart 🛒`}
    </button>
  );
}

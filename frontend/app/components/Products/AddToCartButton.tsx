"use client";

interface AddToCartButtonProps {
  productId: string;
  quantity: number;
  outOfStock: boolean;
  addingProductId: string | null;
  onAddToCart: (productId: string) => void;
}

export default function AddToCartButton({
  productId,
  quantity,
  outOfStock,
  addingProductId,
  onAddToCart,
}: AddToCartButtonProps) {
  const isAdding = addingProductId === productId;

  return (
    <button
      onClick={() => onAddToCart(productId)}
      disabled={isAdding || outOfStock || quantity < 1}
      className="mt-4 w-full bg-black text-white py-3 rounded text-sm sm:text-base disabled:opacity-50 disabled:cursor-not-allowed"
    >
      {isAdding ? "Adding..." : `Add ${quantity} to Cart`}
    </button>
  );
}

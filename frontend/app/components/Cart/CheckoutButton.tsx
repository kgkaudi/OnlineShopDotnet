"use client";

interface CheckoutButtonProps {
  onCheckout: () => void;
}

export default function CheckoutButton({ onCheckout }: CheckoutButtonProps) {
  return (
    <button
      onClick={onCheckout}
      className="mt-5 w-full bg-black text-white py-3 rounded hover:bg-gray-800"
    >
      Place Order
    </button>
  );
}

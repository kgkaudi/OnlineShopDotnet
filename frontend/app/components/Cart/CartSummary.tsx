"use client";

interface CartSummaryProps {
  totalItems: number;
  subtotal: number;
  discount: number;
  total: number;
}

export default function CartSummary({
  totalItems,
  subtotal,
  discount,
  total,
}: CartSummaryProps) {
  return (
    <>
      <div className="flex justify-between text-sm mb-2">
        <span>Items</span>
        <span>{totalItems}</span>
      </div>

      <div className="border-t pt-3 mt-3 flex justify-between font-bold text-lg">
        <span>Subtotal</span>
        <span>€{subtotal.toFixed(2)}</span>
      </div>

      {discount > 0 && (
        <div className="flex justify-between text-sm text-green-700">
          <span>Discount</span>
          <span>−€{discount.toFixed(2)}</span>
        </div>
      )}

      <div className="border-t pt-4 mt-4 flex justify-between font-bold text-xl">
        <span>Total</span>
        <span>€{total.toFixed(2)}</span>
      </div>
    </>
  );
}

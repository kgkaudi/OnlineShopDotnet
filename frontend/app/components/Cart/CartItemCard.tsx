"use client";

interface CartItemCardProps {
  item: any;
  isUpdating: boolean;
  onDecrease: () => void;
  onIncrease: () => void;
  onRemove: () => void;
}

export default function CartItemCard({
  item,
  isUpdating,
  onDecrease,
  onIncrease,
  onRemove,
}: CartItemCardProps) {
  return (
    <div className="border rounded-lg bg-white p-4 shadow-sm">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div className="min-w-0">
          <h2 className="font-semibold text-lg">
            {item.product?.name || "Product unavailable"}
          </h2>

          <p className="text-sm text-gray-500 mt-1">
            {item.product
              ? `€${Number(item.product.price).toFixed(2)} each`
              : "Product details unavailable"}
          </p>          
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={onDecrease}
            disabled={isUpdating || item.quantity <= 1}
            className="w-9 h-9 border rounded hover:bg-gray-100 disabled:opacity-40"
          >
            −
          </button>

          <span className="w-8 text-center font-semibold">
            {item.quantity}
          </span>

          <button
            onClick={onIncrease}
            disabled={isUpdating}
            className="w-9 h-9 border rounded hover:bg-gray-100 disabled:opacity-40"
          >
            +
          </button>

          <button
            onClick={onRemove}
            disabled={isUpdating}
            className="ml-2 text-red-600 hover:underline disabled:opacity-40"
          >
            Remove
          </button>
        </div>
      </div>

      {item.product && (
        <div className="border-t mt-4 pt-3 text-right font-semibold">
          €{(Number(item.product.price) * item.quantity).toFixed(2)}
        </div>
      )}
    </div>
  );
}

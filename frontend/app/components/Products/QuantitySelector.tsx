"use client";

interface QuantitySelectorProps {
  productId: string;
  quantity: number;
  maxStock?: number;
  onChangeQuantity: (
    productId: string,
    quantity: number,
    maxStock?: number
  ) => void;
}

export default function QuantitySelector({
  productId,
  quantity,
  maxStock,
  onChangeQuantity,
}: QuantitySelectorProps) {
  return (
    <div className="mt-4">
      <label
        htmlFor={`quantity-${productId}`}
        className="block text-sm font-medium text-gray-700 mb-2"
      >
        Quantity
      </label>

      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={() => onChangeQuantity(productId, quantity - 1, maxStock)}
          disabled={quantity <= 1}
          className="w-10 h-10 border border-gray-300 rounded hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
        >
          −
        </button>

        <input
          id={`quantity-${productId}`}
          type="number"
          min={1}
          max={maxStock}
          value={quantity}
          onChange={(e) =>
            onChangeQuantity(productId, Number(e.target.value), maxStock)
          }
          className="w-20 h-10 text-center border border-gray-300 rounded"
        />

        <button
          type="button"
          onClick={() => onChangeQuantity(productId, quantity + 1, maxStock)}
          disabled={typeof maxStock === "number" && quantity >= maxStock}
          className="w-10 h-10 border border-gray-300 rounded hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
        >
          +
        </button>
      </div>
    </div>
  );
}

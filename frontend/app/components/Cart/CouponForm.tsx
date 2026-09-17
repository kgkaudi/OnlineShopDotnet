"use client";

interface CouponFormProps {
  couponCode: string;
  appliedCoupon: any;
  applyingCoupon: boolean;
  onChangeCode: (value: string) => void;
  onApply: () => void;
  onRemove: () => void;
}

export default function CouponForm({
  couponCode,
  appliedCoupon,
  applyingCoupon,
  onChangeCode,
  onApply,
  onRemove,
}: CouponFormProps) {
  return (
    <div className="border-t pt-4 mt-4">
      <label
        htmlFor="coupon-code"
        className="block text-sm font-semibold mb-2"
      >
        Coupon code
      </label>

      {appliedCoupon ? (
        <div className="flex items-center justify-between gap-3 rounded-lg border border-green-200 bg-green-50 p-3">
          <div className="min-w-0">
            <p className="font-semibold text-green-800">
              {appliedCoupon.code}
            </p>
            <p className="text-xs text-green-700 mt-1">
              {appliedCoupon.type.toLowerCase() === "percentage"
                ? `${appliedCoupon.value}% discount`
                : `€${appliedCoupon.value.toFixed(2)} discount`}
            </p>
          </div>

          <button
            type="button"
            onClick={onRemove}
            className="text-sm font-medium text-red-600 hover:underline"
          >
            Remove
          </button>
        </div>
      ) : (
        <div className="flex gap-2">
          <input
            id="coupon-code"
            type="text"
            value={couponCode}
            onChange={(e) => onChangeCode(e.target.value)}
            placeholder="Enter coupon"
            disabled={applyingCoupon}
            className="min-w-0 flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm"
          />

          <button
            type="button"
            onClick={onApply}
            disabled={applyingCoupon || !couponCode.trim()}
            className="rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-700 disabled:opacity-50"
          >
            {applyingCoupon ? "Checking..." : "Apply"}
          </button>
        </div>
      )}
    </div>
  );
}

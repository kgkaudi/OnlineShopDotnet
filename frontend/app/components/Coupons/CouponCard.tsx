"use client";

import { Coupon } from "@/src/lib/api";

interface CouponCardProps {
  coupon: Coupon;
}

export default function CouponCard({ coupon }: CouponCardProps) {
  return (
    <div className="border rounded p-4 flex items-center justify-between bg-white shadow-sm hover:shadow-md transition">
      <div>
        <p className="font-semibold">{coupon.code}</p>

        <p className="text-sm text-gray-600">
          {coupon.type === "percentage"
            ? `${coupon.value}% off`
            : `€${coupon.value} off`}
        </p>

        <p className="text-xs text-gray-500 mt-1">
          Expires:{" "}
          {coupon.expiration
            ? new Date(coupon.expiration).toLocaleDateString()
            : "Unknown"}
        </p>

        <p className="text-xs text-gray-500">
          Usage: {coupon.usedCount}/{coupon.maxUsage}
        </p>

        {!coupon.active && (
          <p className="text-xs text-red-600 mt-1">This coupon is inactive.</p>
        )}
      </div>
    </div>
  );
}

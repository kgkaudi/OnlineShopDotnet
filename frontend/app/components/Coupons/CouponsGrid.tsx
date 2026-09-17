"use client";

import { Coupon } from "@/src/lib/api";
import CouponCard from "./CouponCard";

interface CouponsGridProps {
  coupons: Coupon[];
}

export default function CouponsGrid({ coupons }: CouponsGridProps) {
  return (
    <div className="space-y-4">
      {coupons.map((coupon) => (
        <CouponCard key={coupon.id} coupon={coupon} />
      ))}
    </div>
  );
}

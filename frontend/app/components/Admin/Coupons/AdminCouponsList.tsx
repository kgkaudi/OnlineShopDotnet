"use client";

import { Coupon, UserProfile } from "@/src/lib/api";
import AdminCouponRow from "./AdminCouponRow";

interface Props {
  coupons: Coupon[];
  users: UserProfile[];
  deletingId: string | null;

  // NEW: modal-based delete
  onDeleteRequest: (coupon: Coupon) => void;
}

export default function AdminCouponsList({
  coupons,
  users,
  deletingId,
  onDeleteRequest,
}: Props) {
  if (coupons.length === 0) {
    return <p className="text-gray-600">No coupons created yet.</p>;
  }

  return (
    <div className="space-y-4">
      {coupons.map((coupon) => (
        <AdminCouponRow
          key={coupon.id}
          coupon={coupon}
          users={users}
          deletingId={deletingId}
          onDeleteRequest={onDeleteRequest}
        />
      ))}
    </div>
  );
}

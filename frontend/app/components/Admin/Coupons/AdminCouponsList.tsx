"use client";

import { Coupon, UserProfile } from "@/src/lib/api";
import AdminCouponRow from "./AdminCouponRow";

interface Props {
  coupons: Coupon[];
  users: UserProfile[];
  onDelete: (id: string) => void;
}

export default function AdminCouponsList({ coupons, users, onDelete }: Props) {
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
          onDelete={onDelete}
        />
      ))}
    </div>
  );
}

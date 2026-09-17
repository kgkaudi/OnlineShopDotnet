"use client";

import { Coupon, UserProfile } from "@/src/lib/api";

interface Props {
  coupon: Coupon;
  users: UserProfile[];
  onDelete: (id: string) => void;
}

export default function AdminCouponRow({ coupon, users, onDelete }: Props) {
  const assignedUser =
    coupon.userId &&
    (users.find((u) => u.id === coupon.userId)?.email ?? coupon.userId);

  return (
    <div className="border rounded p-4 flex items-center justify-between">
      <div>
        <p className="font-semibold">{coupon.code}</p>
        <p className="text-sm text-gray-600">
          {coupon.type === "percentage"
            ? `${coupon.value}%`
            : `€${coupon.value}`}
        </p>
        <p className="text-xs text-gray-500 mt-1">
          Expires: {new Date(coupon.expiration).toLocaleDateString()}
        </p>
        <p className="text-xs text-gray-500">
          Usage: {coupon.usedCount}/{coupon.maxUsage}
        </p>
        <p className="text-xs text-gray-500 mt-1">
          Assigned to: {assignedUser || "Global"}
        </p>
      </div>

      <button
        onClick={() => onDelete(coupon.id)}
        className="text-red-600 hover:underline"
      >
        Delete
      </button>
    </div>
  );
}

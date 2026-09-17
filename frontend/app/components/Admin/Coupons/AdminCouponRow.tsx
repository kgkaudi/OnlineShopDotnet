"use client";

import { Coupon, UserProfile } from "@/src/lib/api";

interface Props {
  coupon: Coupon;
  users: UserProfile[];
  deletingId: string | null;

  onDeleteRequest: (coupon: Coupon) => void;
}

export default function AdminCouponRow({
  coupon,
  users,
  deletingId,
  onDeleteRequest,
}: Props) {
  const assignedUser =
    coupon.userId &&
    (users.find((u) => u.id === coupon.userId)?.email ?? coupon.userId);

  const isDeleting = deletingId === coupon.id;

  const expiration = coupon.expiration
    ? new Date(coupon.expiration).toISOString().split("T")[0]
    : "Unknown";

  return (
    <div className="border rounded p-4 flex items-center justify-between">
      <div>
        <p className="font-semibold">{coupon.code}</p>

        <p className="text-sm text-gray-600">
          {coupon.type === "percentage"
            ? `${coupon.value}%`
            : `€${coupon.value}`}
        </p>

        <p className="text-xs text-gray-500 mt-1">Expires: {expiration}</p>

        <p className="text-xs text-gray-500">
          Usage: {coupon.usedCount}/{coupon.maxUsage}
        </p>

        <p className="text-xs text-gray-500 mt-1">
          Assigned to: {assignedUser || "Global"}
        </p>
      </div>

      <button
        disabled={isDeleting}
        onClick={() => onDeleteRequest(coupon)}
        className="text-red-600 hover:underline disabled:opacity-50"
      >
        {isDeleting ? "Deleting..." : "Delete"}
      </button>
    </div>
  );
}

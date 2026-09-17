"use client";

interface OrderStatusBadgeProps {
  status: string;
  classes: string;
}

export default function OrderStatusBadge({ status, classes }: OrderStatusBadgeProps) {
  return (
    <span className={`rounded-full px-3 py-1 text-xs font-medium ${classes}`}>
      {status}
    </span>
  );
}

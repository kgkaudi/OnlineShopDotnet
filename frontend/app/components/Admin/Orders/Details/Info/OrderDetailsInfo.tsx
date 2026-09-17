"use client";

interface Props {
  order: any;
}

export default function OrderDetailsInfo({ order }: Props) {
  return (
    <div className="mb-6 space-y-1">
      <p>
        <strong>User:</strong> {order.userFullName ?? order.userId}
      </p>
      <p>
        <strong>Status:</strong> {order.status}
      </p>
      <p>
        <strong>Total:</strong> ${order.total.toFixed(2)}
      </p>
      <p>
        <strong>Created:</strong> {new Date(order.createdAt).toLocaleString()}
      </p>
    </div>
  );
}

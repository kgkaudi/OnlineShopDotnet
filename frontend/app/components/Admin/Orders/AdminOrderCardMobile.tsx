"use client";

import Link from "next/link";

interface Props {
  order: any;
  onDeleteRequest: () => void;
  deletingId?: string | null;
}

export default function AdminOrderCardMobile({
  order,
  onDeleteRequest,
  deletingId,
}: Props) {
  const isDeleting = deletingId === order.id;

  return (
    <div className="border rounded-lg bg-white p-4 shadow-sm">
      <p className="font-semibold">Order ID: {order.id}</p>
      <p className="text-gray-600">User: {order.userFullName}</p>
      <p className="text-gray-600">Total: ${order.total.toFixed(2)}</p>
      <p className="text-gray-600">Status: {order.status}</p>
      <p className="text-gray-600">
        Created: {new Date(order.createdAt).toLocaleString()}
      </p>

      <div className="mt-4 flex flex-col gap-2">
        <Link
          href={`/admin/orders/${order.id}`}
          className="px-3 py-2 rounded bg-blue-600 text-white text-center hover:bg-blue-700"
        >
          View
        </Link>

        <button
          type="button"
          disabled={isDeleting}
          className="px-3 py-2 rounded bg-red-600 text-white hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
          onClick={onDeleteRequest}
        >
          {isDeleting ? "Deleting..." : "Delete"}
        </button>
      </div>
    </div>
  );
}
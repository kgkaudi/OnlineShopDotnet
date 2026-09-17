"use client";

import Link from "next/link";
import { api } from "@/src/lib/api";

interface Props {
  order: any;
  onDelete: (id: string) => void;
}

export default function AdminOrderCardMobile({ order, onDelete }: Props) {
  function onDeleteRequest(order: any): void {
    throw new Error("Function not implemented.");
  }

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
          className="px-3 py-2 rounded bg-red-600 text-white hover:bg-red-700"
          onClick={() => onDeleteRequest(order)}
        >
          Delete
        </button>
      </div>
    </div>
  );
}

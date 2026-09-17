"use client";

import Link from "next/link";

interface Props {
  order: any;
  onDelete: (id: string) => void;
}

export default function AdminOrderRow({ order, onDelete }: Props) {
  return (
    <tr className="border-b last:border-b-0">
      <td className="px-4 py-4 font-medium">{order.id}</td>

      <td className="px-4 py-4">{order.userFullName}</td>

      <td className="px-4 py-4">${order.total.toFixed(2)}</td>

      <td className="px-4 py-4">{order.status}</td>

      <td className="px-4 py-4">
        {new Date(order.createdAt).toLocaleString()}
      </td>

      <td className="px-4 py-4">
        <div className="flex justify-end gap-2">
          <Link
            href={`/admin/orders/${order.id}`}
            className="px-3 py-2 rounded bg-blue-600 text-white hover:bg-blue-700"
          >
            View
          </Link>

          <button
            className="px-3 py-2 rounded bg-red-600 text-white hover:bg-red-700"
            onClick={() => onDelete(order.id)}
          >
            Delete
          </button>
        </div>
      </td>
    </tr>
  );
}

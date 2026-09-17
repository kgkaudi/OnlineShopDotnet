"use client";

import { Order } from "@/src/lib/api";

interface OrderFilterBarProps {
  orders: Order[];
  statusFilter: string;
  onChangeFilter: (value: string) => void;
}

export default function OrderFilterBar({
  orders,
  statusFilter,
  onChangeFilter,
}: OrderFilterBarProps) {
  const statuses = Array.from(new Set(orders.map((o) => o.status)));

  return (
    <div className="mb-6 flex flex-wrap gap-2">
      <button
        type="button"
        onClick={() => onChangeFilter("all")}
        className={`rounded-full px-4 py-1.5 text-sm font-medium transition ${
          statusFilter === "all"
            ? "bg-gray-900 text-white"
            : "bg-gray-100 text-gray-700 hover:bg-gray-200"
        }`}
      >
        All ({orders.length})
      </button>

      {statuses.map((status) => {
        const count = orders.filter((o) => o.status === status).length;
        const active = statusFilter.toLowerCase() === status.toLowerCase();

        return (
          <button
            key={status}
            type="button"
            onClick={() => onChangeFilter(status)}
            className={`rounded-full px-4 py-1.5 text-sm font-medium transition ${
              active
                ? "bg-gray-900 text-white"
                : "bg-gray-100 text-gray-700 hover:bg-gray-200"
            }`}
          >
            {status} ({count})
          </button>
        );
      })}
    </div>
  );
}

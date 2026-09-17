"use client";

import { api } from "@/src/lib/api";
import { useRouter } from "next/navigation";

interface Props {
  order: any;
  setOrder: (order: any) => void;
}

export default function OrderDetailsActions({ order, setOrder }: Props) {
  const router = useRouter();

  async function cancelOrder() {
    const updated = await api.cancelOrder(order.id);
    setOrder(updated);
  }

  async function deleteOrder() {
    if (!confirm("Delete this order?")) return;

    await api.deleteOrder(order.id);
    router.push("/admin/orders");
  }

  return (
    <div className="flex gap-4">
      <button
        className="px-4 py-2 bg-yellow-500 text-white rounded"
        onClick={cancelOrder}
      >
        Cancel Order
      </button>

      <button
        className="px-4 py-2 bg-red-600 text-white rounded"
        onClick={deleteOrder}
      >
        Delete Order
      </button>
    </div>
  );
}

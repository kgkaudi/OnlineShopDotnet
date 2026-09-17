"use client";

import { useEffect, useState } from "react";
import { api } from "@/src/lib/api";
import { useRouter } from "next/navigation";
import React from "react";

export default function OrderDetails({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const router = useRouter();

  const { id } = React.use(params);

  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      try {
        const orderData = await api.getOrderById(id);

        // Fetch user profile
        const profile = await api.getUserProfile(orderData.userId);

        setOrder({
          ...orderData,
          userFullName: profile.fullName ?? profile.email,
        });
      } catch {
        setOrder(null);
      } finally {
        setLoading(false);
      }
    }

    load();
  }, [id]);

  if (loading) return <p className="p-6">Loading...</p>;
  if (!order) return <p className="p-6">Order not found.</p>;

  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-2xl font-bold mb-4">Order {order.id}</h1>

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

      <h2 className="text-xl font-semibold mb-2">Items</h2>
      <ul className="space-y-3 mb-6">
        {order.items.map((item) => (
          <li key={item.productId} className="border p-3 rounded">
            <p>
              <strong>Product:</strong> {item.productName ?? item.productId}
            </p>
            <p>
              <strong>Quantity:</strong> {item.quantity}
            </p>
            <p>
              <strong>Unit Price:</strong> ${item.unitPrice.toFixed(2)}
            </p>
          </li>
        ))}
      </ul>

      <div className="flex gap-4">
        <button
          className="px-4 py-2 bg-yellow-500 text-white rounded"
          onClick={() => {
            api.cancelOrder(order.id).then(setOrder);
          }}
        >
          Cancel Order
        </button>

        <button
          className="px-4 py-2 bg-red-600 text-white rounded"
          onClick={() => {
            if (confirm("Delete this order?")) {
              api.deleteOrder(order.id).then(() => {
                router.push("/admin/orders");
              });
            }
          }}
        >
          Delete Order
        </button>
      </div>
    </div>
  );
}

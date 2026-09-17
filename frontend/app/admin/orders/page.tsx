"use client";

import { useEffect, useState } from "react";
import { api } from "@/src/lib/api";
import Link from "next/link";

export default function OrdersAdminPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.getAllOrders()
      .then(setOrders)
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <main className="max-w-6xl mx-auto px-4 py-10">
        <h1 className="text-3xl font-bold mb-4">Orders Admin</h1>
        <p className="text-gray-600">Loading orders...</p>
      </main>
    );
  }

  if (orders.length === 0) {
    return (
      <main className="max-w-6xl mx-auto px-4 py-10">
        <h1 className="text-3xl font-bold mb-4">Orders Admin</h1>
        <p className="text-gray-600">No orders found.</p>
      </main>
    );
  }

  return (
    <main className="max-w-6xl mx-auto px-4 py-10">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold">Orders Admin</h1>
          <p className="text-gray-600 mt-2">View and manage all orders.</p>
        </div>

        <Link
          href="/admin/users"
          className="px-4 py-2 border rounded hover:bg-gray-50"
        >
          Users
        </Link>
      </div>

      {/* Mobile Cards */}
      <section className="grid gap-4 md:hidden">
        {orders.map((order) => (
          <div
            key={order.id}
            className="border rounded-lg bg-white p-4 shadow-sm"
          >
            <p className="font-semibold">Order ID: {order.id}</p>
            <p className="text-gray-600">
              User: {order.userFullName ?? order.userId}
            </p>
            <p className="text-gray-600">
              Total: ${order.total.toFixed(2)}
            </p>
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
                onClick={() => {
                  if (confirm("Delete this order?")) {
                    api.deleteOrder(order.id).then(() => {
                      setOrders((prev) =>
                        prev.filter((o) => o.id !== order.id)
                      );
                    });
                  }
                }}
              >
                Delete
              </button>
            </div>
          </div>
        ))}
      </section>

      {/* Desktop Table */}
      <section className="hidden md:block border rounded-lg overflow-hidden bg-white">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-100 border-b">
              <tr>
                <th className="text-left px-4 py-3">Order ID</th>
                <th className="text-left px-4 py-3">User</th>
                <th className="text-left px-4 py-3">Total</th>
                <th className="text-left px-4 py-3">Status</th>
                <th className="text-left px-4 py-3">Created</th>
                <th className="text-right px-4 py-3">Actions</th>
              </tr>
            </thead>

            <tbody>
              {orders.map((order) => (
                <tr key={order.id} className="border-b last:border-b-0">
                  <td className="px-4 py-4 font-medium">{order.id}</td>

                  <td className="px-4 py-4">
                    {order.userFullName ?? order.userId}
                  </td>

                  <td className="px-4 py-4">
                    ${order.total.toFixed(2)}
                  </td>

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
                        onClick={() => {
                          if (confirm("Delete this order?")) {
                            api.deleteOrder(order.id).then(() => {
                              setOrders((prev) =>
                                prev.filter((o) => o.id !== order.id)
                              );
                            });
                          }
                        }}
                      >
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </main>
  );
}

"use client";

import { useEffect, useState } from "react";
import { api } from "@/src/lib/api";

import AdminOrdersHeader from "../../components/Admin/Orders/AdminOrdersHeader";
import AdminOrdersLoading from "../../components/Admin/Orders/AdminOrdersLoading";
import AdminOrdersEmpty from "../../components/Admin/Orders/AdminOrdersEmpty";
import AdminOrderCardMobile from "../../components/Admin/Orders/AdminOrderCardMobile";
import AdminOrdersTable from "../../components/Admin/Orders/AdminOrdersTable";

export default function OrdersAdminPage() {
  const [orders, setOrders] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .getAllOrders()
      .then(async (orders) => {
        const enriched = await Promise.all(
          orders.map(async (order) => {
            try {
              const profile = await api.getUserProfile(order.userId);
              return {
                ...order,
                userFullName: profile.fullName ?? profile.email,
              };
            } catch {
              return {
                ...order,
                userFullName: order.userId,
              };
            }
          })
        );

        setOrders(enriched);
      })
      .finally(() => setLoading(false));
  }, []);

  function deleteOrder(id: string) {
    if (!confirm("Delete this order?")) return;

    api.deleteOrder(id).then(() => {
      setOrders((prev) => prev.filter((o) => o.id !== id));
    });
  }

  if (loading) {
    return (
      <main className="max-w-6xl mx-auto px-4 py-10">
        <AdminOrdersHeader />
        <AdminOrdersLoading />
      </main>
    );
  }

  if (orders.length === 0) {
    return (
      <main className="max-w-6xl mx-auto px-4 py-10">
        <AdminOrdersHeader />
        <AdminOrdersEmpty />
      </main>
    );
  }

  return (
    <main className="max-w-6xl mx-auto px-4 py-10">
      <AdminOrdersHeader />

      {/* Mobile */}
      <section className="grid gap-4 md:hidden">
        {orders.map((order) => (
          <AdminOrderCardMobile
            key={order.id}
            order={order}
            onDelete={deleteOrder}
          />
        ))}
      </section>

      {/* Desktop */}
      <AdminOrdersTable orders={orders} onDelete={deleteOrder} />
    </main>
  );
}

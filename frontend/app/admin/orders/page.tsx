"use client";

import { useEffect, useState } from "react";
import { api } from "@/src/lib/api";

import AdminOrdersHeader from "../../components/Admin/Orders/AdminOrdersHeader";
import AdminOrdersLoading from "../../components/Admin/Orders/AdminOrdersLoading";
import AdminOrdersEmpty from "../../components/Admin/Orders/AdminOrdersEmpty";
import AdminOrderCardMobile from "../../components/Admin/Orders/AdminOrderCardMobile";
import AdminOrdersTable from "../../components/Admin/Orders/AdminOrdersTable";
import DeleteOrderModal from "@/app/components/Admin/Orders/DeleteOrderModal";

export default function OrdersAdminPage() {
  const [orders, setOrders] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  // NEW: modal state
  const [selectedOrder, setSelectedOrder] = useState<any | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

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

  // NEW: modal-based delete
  async function handleDeleteOrder(order: any) {
    setDeletingId(order.id);

    try {
      await api.deleteOrder(order.id);
      setOrders((prev) => prev.filter((o) => o.id !== order.id));
    } finally {
      setDeletingId(null);
      setSelectedOrder(null);
    }
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

            // FIX: trigger modal instead of deleting directly
            onDeleteRequest={() => setSelectedOrder(order)}

            deletingId={deletingId}
          />
        ))}
      </section>

      {/* Desktop */}
      <AdminOrdersTable
        orders={orders}

        // FIX: trigger modal instead of deleting directly
        onDeleteRequest={(order) => setSelectedOrder(order)}

        deletingId={deletingId}
      />

      {/* MODAL OUTSIDE TABLE */}
      {selectedOrder && (
        <DeleteOrderModal
          orderId={selectedOrder.id}
          userName={selectedOrder.userFullName}
          total={selectedOrder.total}
          onConfirm={() => handleDeleteOrder(selectedOrder)}
          onCancel={() => setSelectedOrder(null)}
        />
      )}
    </main>
  );
}

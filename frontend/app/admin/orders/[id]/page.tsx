"use client";

import { useEffect, useState } from "react";
import { api } from "@/src/lib/api";
import { useRouter } from "next/navigation";
import React from "react";

import OrderDetailsLoading from "@/app/components/Admin/Orders/Details/Loading/OrderDetailsLoading";
import OrderDetailsNotFound from "@/app/components/Admin/Orders/Details/Loading/OrderDetailsNotFound";
import OrderDetailsHeader from "@/app/components/Admin/Orders/Details/Header/OrderDetailsHeader";
import OrderDetailsInfo from "@/app/components/Admin/Orders/Details/Info/OrderDetailsInfo";
import OrderDetailsItems from "@/app/components/Admin/Orders/Details/Items/OrderDetailsItems";
import OrderDetailsActions from "@/app/components/Admin/Orders/Details/Actions/OrderDetailsActions";

export default function OrderDetails({ params }: { params: Promise<{ id: string }> }) {
  const router = useRouter();
  const { id } = React.use(params);

  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      try {
        const orderData = await api.getOrderById(id);
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

  if (loading) return <OrderDetailsLoading />;
  if (!order) return <OrderDetailsNotFound />;

  return (
    <div className="max-w-4xl mx-auto p-6">
      <OrderDetailsHeader orderId={order.id} />

      <OrderDetailsInfo order={order} />

      <OrderDetailsItems items={order.items} />

      <OrderDetailsActions order={order} setOrder={setOrder} />
    </div>
  );
}

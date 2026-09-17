"use client";

import { useEffect, useMemo, useState } from "react";
import { api, Order, OrderItem, Product } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

import OrdersGrid from "../components/Orders/OrdersGrid";
import OrderFilterBar from "../components/Orders/OrderFilterBar";
import EmptyOrders from "../components/Orders/EmptyOrders";

function formatDate(date: string) {
  const parsed = new Date(date);
  if (Number.isNaN(parsed.getTime())) return "Unknown date";

  return parsed.toLocaleString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function formatPrice(value: number) {
  return new Intl.NumberFormat(undefined, {
    style: "currency",
    currency: "EUR",
  }).format(value);
}

function getStatusClasses(status: string) {
  switch (status.toLowerCase()) {
    case "completed":
    case "delivered":
      return "bg-green-100 text-green-700";
    case "cancelled":
    case "canceled":
      return "bg-red-100 text-red-700";
    case "processing":
    case "shipped":
      return "bg-blue-100 text-blue-700";
    case "pending":
      return "bg-yellow-100 text-yellow-700";
    default:
      return "bg-gray-100 text-gray-700";
  }
}

function getProductName(productId: string, products: Product[]) {
  return products.find((p) => p.id === productId)?.name ?? `Product ${productId}`;
}

function calculateItemTotal(item: OrderItem) {
  return item.quantity * item.unitPrice;
}

export default function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");

  const [reorderingId, setReorderingId] = useState<string | null>(null);
  const [cancelingId, setCancelingId] = useState<string | null>(null);

  const { showSnackbar } = useSnackbar();

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        setLoading(true);
        setError("");

        const [ordersRes, productsRes] = await Promise.all([
          api.getOrders(),
          api.getProducts(),
        ]);

        if (cancelled) return;

        setOrders(ordersRes ?? []);
        setProducts(productsRes ?? []);
      } catch (err) {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : "Failed to load orders.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, []);

  const sortedOrders = useMemo(
    () =>
      [...orders].sort(
        (a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      ),
    [orders]
  );

  const filteredOrders = useMemo(() => {
    if (statusFilter === "all") return sortedOrders;
    return sortedOrders.filter(
      (o) => o.status.toLowerCase() === statusFilter.toLowerCase()
    );
  }, [sortedOrders, statusFilter]);

  async function handleReorder(order: Order) {
    if (!order.items?.length) {
      showSnackbar("This order has no items to reorder.", "error");
      return;
    }

    setReorderingId(order.id);

    try {
      for (const item of order.items) {
        await api.addToCart(item.productId, item.quantity);
      }

      showSnackbar("Items added to your cart 🛒", "success");
    } catch (err) {
      showSnackbar(
        err instanceof Error ? err.message : "Failed to reorder items.",
        "error"
      );
    } finally {
      setReorderingId(null);
    }
  }

  async function handleCancel(order: Order) {
    const confirmed = window.confirm(
      `Cancel order #${order.id}? This cannot be undone.`
    );

    if (!confirmed) return;

    setCancelingId(order.id);

    try {
      const updated = await api.cancelOrder(order.id);

      setOrders((current) =>
        current.map((o) => (o.id === order.id ? updated : o))
      );

      showSnackbar("Order cancelled.", "success");
    } catch (err) {
      showSnackbar(
        err instanceof Error ? err.message : "Failed to cancel order.",
        "error"
      );
    } finally {
      setCancelingId(null);
    }
  }

  if (loading) {
    return (
      <main className="mx-auto max-w-5xl px-4 py-10">
        <div className="h-8 w-40 bg-gray-200 animate-pulse rounded" />
        <div className="mt-2 h-4 w-64 bg-gray-200 animate-pulse rounded" />
      </main>
    );
  }

  return (
    <main className="mx-auto max-w-5xl px-4 py-10">
      <h1 className="text-3xl font-bold text-gray-900 mb-2">My Orders</h1>
      <p className="text-sm text-gray-600 mb-6">
        View your previous orders and their current status.
      </p>

      {error && (
        <div className="mb-6 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {!error && orders.length > 0 && (
        <OrderFilterBar
          orders={orders}
          statusFilter={statusFilter}
          onChangeFilter={setStatusFilter}
        />
      )}

      {!error && orders.length === 0 && <EmptyOrders />}

      {!error && orders.length > 0 && filteredOrders.length === 0 && (
        <div className="rounded-xl border border-gray-200 bg-white px-6 py-12 text-center text-gray-600">
          No orders match this filter.
        </div>
      )}

      {filteredOrders.length > 0 && (
        <OrdersGrid
          orders={filteredOrders}
          products={products}
          reorderingId={reorderingId}
          cancelingId={cancelingId}
          onReorder={handleReorder}
          onCancel={handleCancel}
          formatDate={formatDate}
          formatPrice={formatPrice}
          getStatusClasses={getStatusClasses}
          getProductName={getProductName}
          calculateItemTotal={calculateItemTotal}
        />
      )}
    </main>
  );
}

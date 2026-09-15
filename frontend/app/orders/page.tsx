"use client";

import { useEffect, useMemo, useState } from "react";
import { api, Order, OrderItem, Product } from "@/src/lib/api";

function formatDate(date: string) {
  const parsedDate = new Date(date);
  if (Number.isNaN(parsedDate.getTime())) {
    return "Unknown date";
  }
  return parsedDate.toLocaleString(undefined, {
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
  const product = products.find((item) => item.id === productId);
  return product?.name ?? `Product ${productId}`;
}
function calculateItemTotal(item: OrderItem) {
  return item.quantity * item.unitPrice;
}
export default function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  useEffect(() => {
    let cancelled = false;
    async function loadOrders() {
      try {
        setLoading(true);
        setError("");
        const [ordersResult, productsResult] = await Promise.all([
          api.getOrders(),
          api.getProducts(),
        ]);
        if (cancelled) {
          return;
        }
        setOrders(ordersResult ?? []);
        setProducts(productsResult ?? []);
      } catch (err) {
        if (cancelled) {
          return;
        }
        setError(err instanceof Error ? err.message : "Failed to load orders.");
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }
    loadOrders();
    return () => {
      cancelled = true;
    };
  }, []);
  const sortedOrders = useMemo(() => {
    return [...orders].sort(
      (a, b) =>
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    );
  }, [orders]);
  if (loading) {
    return (
      <main className="mx-auto max-w-5xl px-4 py-10">
        <div className="mb-8">
          <div className="h-8 w-40 animate-pulse rounded bg-gray-200" />
          <div className="mt-2 h-4 w-64 animate-pulse rounded bg-gray-200" />
        </div>
        <div className="space-y-5">
          {[1, 2, 3].map((item) => (
            <div
              key={item}
              className="animate-pulse rounded-xl border border-gray-200 bg-
white p-6"
            >
              <div className="h-5 w-32 rounded bg-gray-200" />
              <div className="mt-3 h-4 w-48 rounded bg-gray-200" />3
              <div className="mt-6 space-y-3">
                <div className="h-4 w-full rounded bg-gray-200" />
                <div className="h-4 w-3/4 rounded bg-gray-200" />
              </div>
            </div>
          ))}
        </div>
      </main>
    );
  }
  return (
    <main className="mx-auto max-w-5xl px-4 py-10">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">My Orders</h1>
        <p className="mt-2 text-sm text-gray-600">
          View your previous orders and their current status.
        </p>
      </div>
      {error && (
        <div
          className="mb-6 rounded-lg border border-red-200 bg-red-50 px-4
py-3 text-sm text-red-700"
        >
          {error}
        </div>
      )}
      {!error && sortedOrders.length === 0 && (
        <div
          className="rounded-xl border border-gray-200 bg-white px-6 py-12
text-center"
        >
          <div className="text-4xl">📦</div>
          <h2 className="mt-4 text-xl font-semibold text-gray-900">
            No orders yet
          </h2>
          <p className="mt-2 text-sm text-gray-600">
            Your completed purchases will appear here.
          </p>
        </div>
      )}
      {sortedOrders.length > 0 && (
        <div className="space-y-6">
          4
          {sortedOrders.map((order) => (
            <section
              key={order.id}
              className="overflow-hidden rounded-xl border border-gray-200 bg-
white shadow-sm"
            >
              {/* Order header */}
              <div
                className="flex flex-col gap-4 border-b border-gray-200 px-6
py-5 sm:flex-row sm:items-center sm:justify-between"
              >
                <div>
                  <div className="flex flex-wrap items-center gap-3">
                    <h2 className="font-semibold text-gray-900">
                      Order #{order.id}
                    </h2>
                    <span
                      className={`rounded-full px-3 py-1 text-xs font-medium $
{getStatusClasses(
order.status,
)}`}
                    >
                      {order.status}
                    </span>
                  </div>
                  <p className="mt-1 text-sm text-gray-500">
                    {formatDate(order.createdAt)}
                  </p>
                </div>
                <div className="text-left sm:text-right">
                  <p className="text-xs uppercase tracking-wide text-gray-500">
                    Total
                  </p>
                  <p className="mt-1 text-xl font-bold text-gray-900">
                    {formatPrice(order.total)}
                  </p>
                </div>
              </div>
              {/* Order items */}
              <div className="divide-y divide-gray-100">
                {order.items?.map((item, index) => (
                  <div
                    key={`${order.id}-${item.productId}-${index}`}
                    className="flex flex-col gap-3 px-6 py-4 sm:flex-row
sm:items-center sm:justify-between"
                  >
                    <div className="min-w-0">
                      <p className="font-medium text-gray-900">
                        {getProductName(item.productId, products)}
                      </p>
                      <p className="mt-1 text-sm text-gray-500">
                        Product ID: {item.productId}
                      </p>
                    </div>
                    <div
                      className="flex items-center justify-between gap-8
sm:justify-end"
                    >
                      <div className="text-sm text-gray-600">
                        <span>
                          {item.quantity} × {formatPrice(item.unitPrice)}
                        </span>
                      </div>
                      <div
                        className="min-w-24 text-right font-semibold text-
gray-900"
                      >
                        {formatPrice(calculateItemTotal(item))}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
              {/* Order footer */}
              <div className="border-t border-gray-200 bg-gray-50 px-6 py-4">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-600">
                    {order.items?.reduce(
                      (total, item) => total + item.quantity,
                      0,
                    ) ?? 0}{" "}
                    item
                    {(order.items?.reduce(
                      (total, item) => total + item.quantity,
                      0,
                    ) ?? 0) !== 1
                      ? "s"
                      : ""}
                  </span>
                  <span className="font-semibold text-gray-900">
                    {formatPrice(order.total)}
                  </span>
                </div>
              </div>
            </section>
          ))}
        </div>
      )}
    </main>
  );
}

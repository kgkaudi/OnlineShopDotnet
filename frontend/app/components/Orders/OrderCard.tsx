"use client";

import OrderStatusBadge from "./OrderStatusBadge";
import OrderItemsList from "./OrderItemsList";
import { Order, Product, OrderItem } from "@/src/lib/api";

interface OrderCardProps {
  order: Order;
  products: Product[];
  formatDate: (date: string) => string;
  formatPrice: (value: number) => string;
  getStatusClasses: (status: string) => string;
  getProductName: (productId: string, products: Product[]) => string;
  calculateItemTotal: (item: OrderItem) => number;
  canCancel: boolean;
  isReordering: boolean;
  isCanceling: boolean;
  onReorder: (order: Order) => void;
  onCancel: (order: Order) => void;
}

export default function OrderCard({
  order,
  products,
  formatDate,
  formatPrice,
  getStatusClasses,
  getProductName,
  calculateItemTotal,
  canCancel,
  isReordering,
  isCanceling,
  onReorder,
  onCancel,
}: OrderCardProps) {
  return (
    <section className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm">
      {/* Header */}
      <div className="flex flex-col gap-4 border-b border-gray-200 px-6 py-5 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <div className="flex flex-wrap items-center gap-3">
            <h2 className="font-semibold text-gray-900">Order #{order.id}</h2>

            <OrderStatusBadge
              status={order.status}
              classes={getStatusClasses(order.status)}
            />
          </div>

          <p className="mt-1 text-sm text-gray-500">{formatDate(order.createdAt)}</p>
        </div>

        <div className="text-left sm:text-right">
          <p className="text-xs uppercase tracking-wide text-gray-500">Total</p>
          <p className="mt-1 text-xl font-bold text-gray-900">
            {formatPrice(order.total)}
          </p>
        </div>
      </div>

      {/* Items */}
      <OrderItemsList
        items={order.items ?? []}
        products={products}
        formatPrice={formatPrice}
        getProductName={getProductName}
        calculateItemTotal={calculateItemTotal}
      />

      {/* Footer */}
      <div className="border-t border-gray-200 bg-gray-50 px-6 py-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-3 text-sm">
            <span className="text-gray-600">
              {order.items?.reduce((t, i) => t + i.quantity, 0) ?? 0} items
            </span>

            <span className="font-semibold text-gray-900">
              {formatPrice(order.total)}
            </span>
          </div>

          <div className="flex gap-2">
            {canCancel && (
              <button
                type="button"
                disabled={isCanceling}
                onClick={() => onCancel(order)}
                className="rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-50 disabled:opacity-50"
              >
                {isCanceling ? "Cancelling..." : "Cancel Order"}
              </button>
            )}

            <button
              type="button"
              disabled={isReordering}
              onClick={() => onReorder(order)}
              className="rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:opacity-50"
            >
              {isReordering ? "Adding..." : "Reorder"}
            </button>
          </div>
        </div>
      </div>
    </section>
  );
}

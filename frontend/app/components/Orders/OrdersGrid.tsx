"use client";

import OrderCard from "./OrderCard";
import { Order, Product, OrderItem } from "@/src/lib/api";

interface OrdersGridProps {
  orders: Order[];
  products: Product[];
  reorderingId: string | null;
  cancelingId: string | null;

  onReorder: (order: Order) => void;
  onCancel: (order: Order) => void;

  formatDate: (date: string) => string;
  formatPrice: (value: number) => string;
  getStatusClasses: (status: string) => string;
  getProductName: (productId: string, products: Product[]) => string;
  calculateItemTotal: (item: OrderItem) => number;
}

export default function OrdersGrid({
  orders,
  products,
  reorderingId,
  cancelingId,
  onReorder,
  onCancel,
  formatDate,
  formatPrice,
  getStatusClasses,
  getProductName,
  calculateItemTotal,
}: OrdersGridProps) {
  return (
    <div className="space-y-6">
      {orders.map((order) => {
        const canCancel = ["pending", "processing"].includes(
          order.status.toLowerCase()
        );

        return (
          <OrderCard
            key={order.id}
            order={order}
            products={products}
            formatDate={formatDate}
            formatPrice={formatPrice}
            getStatusClasses={getStatusClasses}
            getProductName={getProductName}
            calculateItemTotal={calculateItemTotal}
            canCancel={canCancel}
            isReordering={reorderingId === order.id}
            isCanceling={cancelingId === order.id}
            onReorder={() => onReorder(order)}
            onCancel={() => onCancel(order)}
          />
        );
      })}
    </div>
  );
}

"use client";

import { OrderItem, Product } from "@/src/lib/api";

interface OrderItemsListProps {
  items: OrderItem[];
  products: Product[];
  formatPrice: (value: number) => string;
  getProductName: (productId: string, products: Product[]) => string;
  calculateItemTotal: (item: OrderItem) => number;
}

export default function OrderItemsList({
  items,
  products,
  formatPrice,
  getProductName,
  calculateItemTotal,
}: OrderItemsListProps) {
  return (
    <div className="divide-y divide-gray-100">
      {items.map((item, index) => (
        <div
          key={`${item.productId}-${index}`}
          className="flex flex-col gap-3 px-6 py-4 sm:flex-row sm:items-center sm:justify-between"
        >
          <div className="min-w-0">
            <p className="font-medium text-gray-900">
              {getProductName(item.productId, products)}
            </p>

            <p className="mt-1 text-sm text-gray-500">
              Product ID: {item.productId}
            </p>
          </div>

          <div className="flex items-center justify-between gap-8 sm:justify-end">
            <div className="text-sm text-gray-600">
              {item.quantity} × {formatPrice(item.unitPrice)}
            </div>

            <div className="min-w-24 text-right font-semibold text-gray-900">
              {formatPrice(calculateItemTotal(item))}
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}

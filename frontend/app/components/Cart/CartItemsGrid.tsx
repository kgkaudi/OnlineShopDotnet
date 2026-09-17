"use client";

import CartItemCard from "./CartItemCard";

interface CartItemsGridProps {
  items: any[];
  updatingProductId: string | null;
  onUpdateQuantity: (productId: string, quantity: number) => void;
  onRemoveItem: (productId: string) => void;
}

export default function CartItemsGrid({
  items,
  updatingProductId,
  onUpdateQuantity,
  onRemoveItem,
}: CartItemsGridProps) {
  return (
    <div className="lg:col-span-2 space-y-4">
      {items.map((item) => {
        const isUpdating = updatingProductId === item.productId;

        return (
          <CartItemCard
            key={item.id || item.productId}
            item={item}
            isUpdating={isUpdating}
            onDecrease={() => onUpdateQuantity(item.productId, item.quantity - 1)}
            onIncrease={() => onUpdateQuantity(item.productId, item.quantity + 1)}
            onRemove={() => onRemoveItem(item.productId)}
          />
        );
      })}
    </div>
  );
}

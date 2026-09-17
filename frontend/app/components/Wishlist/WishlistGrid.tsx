"use client";

import WishlistItemCard from "./WishlistItemCard";

interface WishlistGridProps {
  items: any[];
  onRemove: (productId: string) => void;
}

export default function WishlistGrid({ items, onRemove }: WishlistGridProps) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      {items.map((item) => (
        <WishlistItemCard key={item.productId} item={item} onRemove={onRemove} />
      ))}
    </div>
  );
}

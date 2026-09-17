"use client";

import ProductCard from "./ProductCard";

interface ProductsGridProps {
  products: any[];
  quantities: Record<string, number>;
  wishlistIds: Set<string>;
  addingProductId: string | null;
  wishlistProductId: string | null;

  onChangeQuantity: (
    productId: string,
    quantity: number,
    maxStock?: number
  ) => void;

  onAddToCart: (productId: string) => void;
  onToggleWishlist: (productId: string) => void;
}

export default function ProductsGrid({
  products,
  quantities,
  wishlistIds,
  addingProductId,
  wishlistProductId,
  onChangeQuantity,
  onAddToCart,
  onToggleWishlist,
}: ProductsGridProps) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      {products.map((product) => {
        const quantity = quantities[product.id] ?? 1;
        const outOfStock = product.stockQuantity === 0;

        const maxStock =
          typeof product.stockQuantity === "number"
            ? product.stockQuantity
            : undefined;

        return (
          <ProductCard
            key={product.id}
            product={product}
            quantity={quantity}
            outOfStock={outOfStock}
            maxStock={maxStock}
            addingProductId={addingProductId}
            wishlistProductId={wishlistProductId}
            isWishlisted={wishlistIds.has(product.id)}
            onChangeQuantity={onChangeQuantity}
            onAddToCart={onAddToCart}
            onToggleWishlist={onToggleWishlist}
          />
        );
      })}
    </div>
  );
}

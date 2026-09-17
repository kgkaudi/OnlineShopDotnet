// frontend/app/components/ProductDetails/Recommended/RecommendedProducts.tsx
"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api, Product } from "@/src/lib/api";

interface Props {
  currentProductId: string;
}

export default function RecommendedProducts({ currentProductId }: Props) {
  const [items, setItems] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .getProducts()
      .then((all) => {
        const filtered = all.filter((p) => p.id !== currentProductId).slice(0, 4);
        setItems(filtered);
      })
      .finally(() => setLoading(false));
  }, [currentProductId]);

  if (loading) {
    return <p className="text-gray-600">Loading recommendations...</p>;
  }

  if (items.length === 0) {
    return <p className="text-gray-600">No recommendations available.</p>;
  }

  return (
    <div className="grid gap-6 grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
      {items.map((item) => (
        <Link
          key={item.id}
          href={`/products/${item.id}`}
          className="border rounded-xl p-4 bg-white shadow-sm hover:shadow-md transition block"
        >
          <h3 className="font-semibold text-lg">{item.name}</h3>
          <p className="text-gray-600 mt-1 line-clamp-2">
            {item.description ?? "No description available."}
          </p>
          <p className="text-black font-bold mt-3">
            €{item.price.toFixed(2)}
          </p>
          <button className="mt-4 w-full bg-black text-white py-2 rounded">
            View Product
          </button>
        </Link>
      ))}
    </div>
  );
}

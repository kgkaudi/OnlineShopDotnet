"use client";

import { useEffect, useState } from "react";
import Container from "../components/Container";
import { api, getClientToken } from "@/src/lib/api";

export default function CartPage() {
  const [cart, setCart] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const token = getClientToken();

    if (!token) {
      setError("You must be logged in to view your cart.");
      setLoading(false);
      return;
    }

    api
      .getCart(token)
      .then((items) => setCart(items))
      .catch((err) => setError(err.message || "Failed to load cart"))
      .finally(() => setLoading(false));
  }, []);

  return (
    <Container>
      <h1 className="text-2xl font-bold mb-6">Cart</h1>

      {loading && <p className="text-gray-600">Loading your cart...</p>}

      {error && (
        <p className="text-red-600 bg-red-100 px-3 py-2 rounded">{error}</p>
      )}

      {!loading && !error && cart.length === 0 && (
        <p className="text-gray-600">Your cart is empty.</p>
      )}

      {!loading && !error && cart.length > 0 && (
        <div className="space-y-4">
          {cart.map((item) => (
            <div
              key={item.id}
              className="border rounded p-4 flex items-center justify-between"
            >
              <div>
                <h2 className="font-semibold">{item.productName}</h2>
                <p className="text-gray-600">{item.price} €</p>
              </div>

              <p className="text-sm text-gray-500">
                Qty: {item.quantity ?? 1}
              </p>
            </div>
          ))}
        </div>
      )}
    </Container>
  );
}

"use client";

import { useEffect, useState } from "react";
import Container from "../components/Container";

export default function WishlistPage() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  async function fetchWishlist() {
    try {
      const token = localStorage.getItem("token");
      if (!token) {
        setError("You must be logged in.");
        setLoading(false);
        return;
      }

      const res = await fetch("http://localhost:5000/api/wishlist", {
        headers: { Authorization: `Bearer ${token}` },
      });

      if (!res.ok) {
        setError(await res.text());
        setLoading(false);
        return;
      }

      const data = await res.json();
      setItems(data);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  async function removeItem(productId: string) {
    try {
      const token = localStorage.getItem("token");
      const res = await fetch(
        `http://localhost:5000/api/wishlist/${productId}`,
        {
          method: "DELETE",
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      if (!res.ok) {
        alert(await res.text());
        return;
      }

      // Remove from UI
      setItems((prev) => prev.filter((i) => i.productId !== productId));
    } catch (err) {
      console.error(err);
    }
  }

  useEffect(() => {
    fetchWishlist();
  }, []);

  return (
    <Container>
      <h1 className="text-2xl font-bold mb-6">Wishlist</h1>

      {loading && <p className="text-gray-600">Loading...</p>}
      {error && (
        <p className="text-red-600 bg-red-100 p-3 rounded mb-4">{error}</p>
      )}

      {!loading && items.length === 0 && (
        <p className="text-gray-600">Your wishlist is empty.</p>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {items.map((item) => (
          <div key={item.productId} className="border p-4 rounded shadow-sm">
            <h2 className="font-semibold text-lg">{item.productName}</h2>
            <p className="text-gray-600 mb-2">{item.productPrice} €</p>

            <button
              onClick={() => removeItem(item.productId)}
              className="text-red-600 hover:underline"
            >
              Remove
            </button>
          </div>
        ))}
      </div>
    </Container>
  );
}

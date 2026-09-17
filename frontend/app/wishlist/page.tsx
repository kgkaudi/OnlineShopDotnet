"use client";

import { useEffect, useState } from "react";
import Container from "../components/Container";
import WishlistGrid from "../components/Wishlist/WishlistGrid";
import EmptyWishlist from "../components/Wishlist/EmptyWishlist";
import { useSnackbar } from "@/src/context/SnackbarContext";

export default function WishlistPage() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const { showSnackbar } = useSnackbar();

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
      if (!token) {
        showSnackbar("You must be logged in.", "error");
        return;
      }

      const res = await fetch("http://localhost:5000/api/wishlist/remove", {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ productId }),
      });

      if (!res.ok) {
        const msg = await res.text();
        showSnackbar(msg, "error");
        return;
      }

      setItems((prev) => prev.filter((i) => i.productId !== productId));
      showSnackbar("Removed from wishlist ❤️", "success");
    } catch (err) {
      console.error(err);
      showSnackbar("Something went wrong while removing item.", "error");
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

      {!loading && items.length === 0 && <EmptyWishlist />}

      {!loading && items.length > 0 && (
        <WishlistGrid items={items} onRemove={removeItem} />
      )}
    </Container>
  );
}

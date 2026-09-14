"use client";

import { useEffect, useState } from "react";
import Container from "../components/Container";
import { api } from "@/src/lib/api";

export default function ProductsPage() {
  const [products, setProducts] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  async function addToWishlist(productId: string) {
    try {
      const token = localStorage.getItem("token");
      if (!token) {
        alert("You must be logged in.");
        return;
      }

      const res = await fetch(
        `http://localhost:5000/api/wishlist/${productId}`,
        {
          method: "POST",
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      if (!res.ok) {
        alert(await res.text());
        return;
      }

      alert("Added to wishlist ❤️");
    } catch (err) {
      console.error(err);
    }
  }

  useEffect(() => {
    api
      .getProducts()
      .then((items) => setProducts(items))
      .catch((err) => setError(err.message || "Failed to load products"))
      .finally(() => setLoading(false));
  }, []);

  return (
    <Container>
      <h1 className="text-2xl font-bold mb-6">Products</h1>

      {loading && <p className="text-gray-600">Loading products...</p>}

      {error && (
        <p className="text-red-600 bg-red-100 px-3 py-2 rounded">{error}</p>
      )}

      {!loading && !error && products.length === 0 && (
        <p className="text-gray-600">No products found.</p>
      )}

      {!loading && !error && products.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
          {products.map((product) => (
            <div
              key={product.id}
              className="border rounded p-4 bg-white shadow-sm hover:shadow-md transition"
            >
              <h2 className="font-semibold text-lg">{product.name}</h2>

              <p className="text-gray-600 mt-1">{product.description}</p>

              <p className="text-black font-bold mt-3">{product.price} €</p>

              <button className="mt-4 w-full bg-black text-white py-2 rounded">
                Add to Cart
              </button>

              <button
                onClick={() => addToWishlist(product.id)}
                className="mt-2 w-full border border-gray-300 py-2 rounded hover:bg-gray-100"
              >
                ❤️ Add to Wishlist
              </button>
            </div>
          ))}
        </div>
      )}
    </Container>
  );
}

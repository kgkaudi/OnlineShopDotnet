"use client";

import { useEffect, useState } from "react";
import Container from "@/app/components/Container";
import { api } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";
import Link from "next/link";

export default function ReviewsAdminPage() {
  const [reviews, setReviews] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [confirmId, setConfirmId] = useState<string | null>(null);
  const { showSnackbar } = useSnackbar();

  useEffect(() => {
    api
      .getAllReviews()
      .then(async (data) => {
        const enriched = await Promise.all(
          data.map(async (r) => {
            let userName = "Unknown user";
            let productName = "Unknown product";

            try {
              const user = await api.getUserProfile(r.userId);
              userName = user.fullName || "Unknown user";
            } catch {}

            try {
              const product = await api.getProduct(r.productId);
              productName = product.name || "Unknown product";
            } catch {}

            return { ...r, userName, productName };
          })
        );

        setReviews(enriched);
      })
      .catch((err) => {
        console.error(err);
        showSnackbar("Failed to load reviews.", "error");
      })
      .finally(() => setLoading(false));
  }, []);

  async function deleteReview(id: string) {
    try {
      setDeletingId(id);
      await api.deleteReview(id);
      setReviews((current) => current.filter((r) => r.id !== id));
      showSnackbar("Review deleted successfully.", "success");
    } catch (err) {
      console.error(err);
      showSnackbar("Failed to delete review.", "error");
    } finally {
      setDeletingId(null);
      setConfirmId(null);
    }
  }

  return (
    <Container>
      <h1 className="text-2xl font-bold mb-6">Reviews Admin</h1>

      {loading && <p className="text-gray-600">Loading reviews...</p>}

      {!loading && reviews.length === 0 && (
        <p className="text-gray-600">No reviews found.</p>
      )}

      {!loading && reviews.length > 0 && (
        <div className="space-y-4">
          {reviews.map((r) => (
            <div
              key={r.id}
              className="border rounded-xl p-5 bg-gray-50 flex justify-between items-start"
            >
              <div className="max-w-[75%]">
                <p className="font-semibold">
                  {r.rating}⭐ — {r.userName}
                </p>

                <p className="text-sm text-gray-500">
                  {new Date(r.createdAt).toLocaleDateString("en-GB")}
                </p>

                <p className="mt-2 text-gray-700">{r.comment}</p>

                <p className="mt-3 text-sm text-gray-600">
                  Product:{" "}
                  <Link
                    href={`/products/${r.productId}`}
                    className="underline hover:text-black"
                  >
                    {r.productName}
                  </Link>
                </p>

                {confirmId === r.id && (
                  <div className="mt-4 flex gap-3">
                    <button
                      onClick={() => deleteReview(r.id)}
                      className="bg-red-700 text-white px-3 py-1 rounded"
                    >
                      Yes, delete
                    </button>

                    <button
                      onClick={() => setConfirmId(null)}
                      className="bg-gray-300 text-black px-3 py-1 rounded"
                    >
                      Cancel
                    </button>
                  </div>
                )}
              </div>

              {confirmId !== r.id && (
                <button
                  onClick={() => setConfirmId(r.id)}
                  disabled={deletingId === r.id}
                  className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700 disabled:opacity-50"
                >
                  {deletingId === r.id ? "Deleting..." : "Delete"}
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </Container>
  );
}

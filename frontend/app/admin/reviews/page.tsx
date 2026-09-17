"use client";

import { useEffect, useState } from "react";
import Container from "@/app/components/Container";
import { api } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

import AdminReviewsHeader from "../../components/Admin/Reviews/AdminReviewsHeader";
import AdminReviewsLoading from "../../components/Admin/Reviews/AdminReviewsLoading";
import AdminReviewsEmpty from "../../components/Admin/Reviews/AdminReviewsEmpty";
import AdminReviewsList from "../../components/Admin/Reviews/AdminReviewsList";

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
      .catch(() => {
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
    } catch {
      showSnackbar("Failed to delete review.", "error");
    } finally {
      setDeletingId(null);
      setConfirmId(null);
    }
  }

  return (
    <Container>
      <AdminReviewsHeader />

      {loading && <AdminReviewsLoading />}
      {!loading && reviews.length === 0 && <AdminReviewsEmpty />}

      {!loading && reviews.length > 0 && (
        <AdminReviewsList
          reviews={reviews}
          deletingId={deletingId}
          confirmId={confirmId}
          onConfirm={setConfirmId}
          onCancel={() => setConfirmId(null)}
          onDelete={deleteReview}
        />
      )}
    </Container>
  );
}

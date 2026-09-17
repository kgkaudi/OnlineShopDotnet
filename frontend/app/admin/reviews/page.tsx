"use client";

import { useEffect, useState } from "react";
import Container from "@/app/components/Container";
import { api } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

import AdminReviewsHeader from "../../components/Admin/Reviews/AdminReviewsHeader";
import AdminReviewsLoading from "../../components/Admin/Reviews/AdminReviewsLoading";
import AdminReviewsEmpty from "../../components/Admin/Reviews/AdminReviewsEmpty";
import AdminReviewsList from "../../components/Admin/Reviews/AdminReviewsList";
import DeleteReviewModal from "@/app/components/Admin/Reviews/DeleteReviewModal";

export default function ReviewsAdminPage() {
  const [reviews, setReviews] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  // NEW: modal state
  const [selectedReview, setSelectedReview] = useState<any | null>(null);

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

  // NEW: modal-based delete
  async function handleDeleteReview(review: any) {
    setDeletingId(review.id);

    try {
      await api.deleteReview(review.id);
      setReviews((current) => current.filter((r) => r.id !== review.id));
      showSnackbar("Review deleted successfully.", "success");
    } catch {
      showSnackbar("Failed to delete review.", "error");
    } finally {
      setDeletingId(null);
      setSelectedReview(null);
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

          onDeleteRequest={(review) => setSelectedReview(review)}
        />
      )}

      {selectedReview && (
        <DeleteReviewModal
          reviewUserName={selectedReview.userName}
          reviewProductName={selectedReview.productName}
          onConfirm={() => handleDeleteReview(selectedReview)}
          onCancel={() => setSelectedReview(null)}
        />
      )}
    </Container>
  );
}

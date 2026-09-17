"use client";

import AdminReviewRow from "./AdminReviewRow";

interface Props {
  reviews: any[];
  deletingId: string | null;

  // NEW: modal-based delete
  onDeleteRequest: (review: any) => void;
}

export default function AdminReviewsList({
  reviews,
  deletingId,
  onDeleteRequest,
}: Props) {
  return (
    <div className="space-y-4">
      {reviews.map((r) => (
        <AdminReviewRow
          key={r.id}
          review={r}
          deletingId={deletingId}

          // pass full review object to trigger modal
          onDeleteRequest={() => onDeleteRequest(r)}
        />
      ))}
    </div>
  );
}

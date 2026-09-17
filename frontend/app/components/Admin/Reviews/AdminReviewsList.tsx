"use client";

import AdminReviewRow from "./AdminReviewRow";

interface Props {
  reviews: any[];
  deletingId: string | null;
  confirmId: string | null;
  onConfirm: (id: string) => void;
  onCancel: () => void;
  onDelete: (id: string) => void;
}

export default function AdminReviewsList({
  reviews,
  deletingId,
  confirmId,
  onConfirm,
  onCancel,
  onDelete,
}: Props) {
  return (
    <div className="space-y-4">
      {reviews.map((r) => (
        <AdminReviewRow
          key={r.id}
          review={r}
          deletingId={deletingId}
          confirmId={confirmId}
          onConfirm={onConfirm}
          onCancel={onCancel}
          onDelete={onDelete}
        />
      ))}
    </div>
  );
}

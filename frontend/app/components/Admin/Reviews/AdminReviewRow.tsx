"use client";

import Link from "next/link";

interface Props {
  review: any;
  deletingId: string | null;

  // NEW: modal-based delete
  onDeleteRequest: (review: any) => void;
}

export default function AdminReviewRow({
  review,
  deletingId,
  onDeleteRequest,
}: Props) {
  const isDeleting = deletingId === review.id;

  return (
    <div className="border rounded-xl p-5 bg-gray-50 flex justify-between items-start">
      <div className="max-w-[75%]">
        <p className="font-semibold">
          {review.rating}⭐ — {review.userName}
        </p>

        <p className="text-sm text-gray-500">
          {new Date(review.createdAt).toLocaleDateString("en-GB")}
        </p>

        <p className="mt-2 text-gray-700">{review.comment}</p>

        <p className="mt-3 text-sm text-gray-600">
          Product:{" "}
          <Link
            href={`/products/${review.productId}`}
            className="underline hover:text-black"
          >
            {review.productName}
          </Link>
        </p>
      </div>

      <button
        type="button"
        disabled={isDeleting}
        onClick={() => onDeleteRequest(review)}
        className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700 disabled:opacity-50"
      >
        {isDeleting ? "Deleting..." : "Delete"}
      </button>
    </div>
  );
}

"use client";

interface Props {
  reviews: any[];
  loadingReviews: boolean;
}

export default function ProductReviewsList({
  reviews,
  loadingReviews,
}: Props) {
  if (loadingReviews) {
    return <p className="text-gray-600">Loading reviews...</p>;
  }

  if (!loadingReviews && reviews.length === 0) {
    return (
      <p className="text-gray-600">
        No reviews yet. Be the first to write one!
      </p>
    );
  }

  return (
    <div className="space-y-6">
      {reviews.map((r) => (
        <div key={r.id} className="border rounded-xl p-5 bg-gray-50">
          <div className="flex items-center gap-3">
            <span className="text-lg font-semibold">{r.rating}⭐</span>
            <span className="text-sm text-gray-500">
              {r.userName} •{" "}
              {new Date(r.createdAt).toLocaleDateString("en-GB", {
                day: "2-digit",
                month: "short",
                year: "numeric",
              })}
            </span>
          </div>
          <p className="mt-2 text-gray-700">{r.comment}</p>
        </div>
      ))}
    </div>
  );
}

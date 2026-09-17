"use client";

interface Props {
  reviewRating: number;
  reviewComment: string;
  submittingReview: boolean;
  setReviewRating: (value: number) => void;
  setReviewComment: (value: string) => void;
  onSubmit: () => void;
}

export default function ProductReviewForm({
  reviewRating,
  reviewComment,
  submittingReview,
  setReviewRating,
  setReviewComment,
  onSubmit,
}: Props) {
  return (
    <div className="mt-10 border-t pt-10">
      <h3 className="text-xl font-bold mb-4">Write a Review</h3>

      <label className="block mb-2 font-medium">Rating</label>
      <select
        value={reviewRating}
        onChange={(e) => setReviewRating(Number(e.target.value))}
        className="border rounded-lg p-2"
      >
        {[1, 2, 3, 4, 5].map((n) => (
          <option key={n} value={n}>
            {n} ⭐
          </option>
        ))}
      </select>

      <label className="block mt-4 mb-2 font-medium">Comment</label>
      <textarea
        value={reviewComment}
        onChange={(e) => setReviewComment(e.target.value)}
        className="border rounded-lg p-3 w-full"
        rows={4}
        placeholder="Share your experience..."
      />

      <button
        onClick={onSubmit}
        disabled={submittingReview}
        className="mt-4 bg-black text-white px-6 py-3 rounded-lg hover:bg-gray-800 disabled:opacity-50"
      >
        {submittingReview ? "Submitting..." : "Submit Review"}
      </button>
    </div>
  );
}

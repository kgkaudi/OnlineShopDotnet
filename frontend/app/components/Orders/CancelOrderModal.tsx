"use client";

interface Props {
  orderId: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function CancelOrderModal({ orderId, onConfirm, onCancel }: Props) {
  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md shadow-lg">
        <h2 className="text-xl font-bold text-red-600 mb-3">Cancel Order</h2>

        <p className="text-gray-700 mb-6">
          Are you sure you want to cancel order <strong>#{orderId}</strong>?  
          This action cannot be undone.
        </p>

        <div className="flex justify-end gap-3">
          <button
            onClick={onCancel}
            className="px-4 py-2 rounded border border-gray-300 hover:bg-gray-100"
          >
            Keep Order
          </button>

          <button
            onClick={onConfirm}
            className="px-4 py-2 rounded bg-red-600 text-white hover:bg-red-700"
          >
            Cancel Order
          </button>
        </div>
      </div>
    </div>
  );
}

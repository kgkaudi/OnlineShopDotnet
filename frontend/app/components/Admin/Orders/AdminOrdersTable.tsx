"use client";

import AdminOrderRow from "./AdminOrderRow";

interface Props {
  orders: any[];
  deletingId: string | null;

  // NEW: modal-based delete
  onDeleteRequest: (order: any) => void;
}

export default function AdminOrdersTable({
  orders,
  deletingId,
  onDeleteRequest,
}: Props) {
  return (
    <section className="hidden md:block border rounded-lg overflow-hidden bg-white">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-100 border-b">
            <tr>
              <th className="text-left px-4 py-3">Order ID</th>
              <th className="text-left px-4 py-3">User</th>
              <th className="text-left px-4 py-3">Total</th>
              <th className="text-left px-4 py-3">Status</th>
              <th className="text-left px-4 py-3">Created</th>
              <th className="text-right px-4 py-3">Actions</th>
            </tr>
          </thead>

          <tbody>
            {orders.map((order) => (
              <AdminOrderRow
                key={order.id}
                order={order}
                deletingId={deletingId}
                onDeleteRequest={(o) => onDeleteRequest(o)}
              />
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}

"use client";

import { UserProfile } from "@/src/lib/api";

interface Props {
  code: string;
  type: "percentage" | "fixed";
  value: number;
  expiration: string;
  maxUsage: number;
  userId: string;
  users: UserProfile[];
  creating: boolean;
  onChange: (field: string, value: any) => void;
  onSubmit: () => void;
}

export default function AdminCouponsCreateForm({
  code,
  type,
  value,
  expiration,
  maxUsage,
  userId,
  users,
  creating,
  onChange,
  onSubmit,
}: Props) {
  return (
    <div className="border rounded-lg bg-white p-5 mb-8 shadow-sm">
      <h2 className="font-semibold text-lg mb-4">Create New Coupon</h2>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium mb-1">Code</label>
          <input
            type="text"
            value={code}
            onChange={(e) => onChange("code", e.target.value)}
            className="w-full border rounded px-3 py-2"
            placeholder="SUMMER20"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Type</label>
          <select
            value={type}
            onChange={(e) => onChange("type", e.target.value)}
            className="w-full border rounded px-3 py-2"
          >
            <option value="percentage">Percentage (%)</option>
            <option value="fixed">Fixed (€)</option>
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Value</label>
          <input
            type="number"
            value={value}
            onChange={(e) => onChange("value", Number(e.target.value))}
            className="w-full border rounded px-3 py-2"
            placeholder="10"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Expiration</label>
          <input
            type="date"
            value={expiration}
            onChange={(e) => onChange("expiration", e.target.value)}
            className="w-full border rounded px-3 py-2"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Max Usage</label>
          <input
            type="number"
            value={maxUsage}
            onChange={(e) => onChange("maxUsage", Number(e.target.value))}
            className="w-full border rounded px-3 py-2"
            placeholder="1"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Assign to User</label>
          <select
            value={userId}
            onChange={(e) => onChange("userId", e.target.value)}
            className="w-full border rounded px-3 py-2"
          >
            <option value="">Global (no user)</option>
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.fullName} ({u.email})
              </option>
            ))}
          </select>
        </div>
      </div>

      <button
        onClick={onSubmit}
        disabled={creating}
        className="mt-4 bg-black text-white px-5 py-2 rounded hover:bg-gray-800 disabled:opacity-50"
      >
        {creating ? "Creating..." : "Create Coupon"}
      </button>
    </div>
  );
}

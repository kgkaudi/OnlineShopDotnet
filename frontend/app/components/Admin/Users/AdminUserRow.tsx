"use client";

import { AdminUser } from "@/src/lib/api";
import { getClientUserId } from "@/src/lib/api";

interface Props {
  user: AdminUser;
  updatingUserId: string | null;
  deletingUserId: string | null;
  onToggleAdmin: (user: AdminUser) => void;
  onDelete: (user: AdminUser) => void;
}

export default function AdminUserRow({
  user,
  updatingUserId,
  deletingUserId,
  onToggleAdmin,
  onDelete,
}: Props) {
  const hasAdminRole =
    user.roles?.some((role) => role.trim().toLowerCase() === "admin") ?? false;

  const currentUserId = getClientUserId();
  const isCurrentUser = currentUserId === user.id;

  return (
    <tr className="border-b last:border-b-0">
      <td className="px-4 py-4">
        <div className="font-medium">{user.fullName || "Unnamed user"}</div>
        {isCurrentUser && (
          <span className="text-xs text-gray-500">You</span>
        )}
      </td>

      <td className="px-4 py-4">{user.email}</td>

      <td className="px-4 py-4">
        <div className="flex flex-wrap gap-2">
          {user.roles?.map((role) => (
            <span
              key={role}
              className="rounded-full bg-gray-100 px-2 py-1 text-xs"
            >
              {role}
            </span>
          ))}
        </div>
      </td>

      <td className="px-4 py-4">
        {user.isEmailVerified ? (
          <span className="text-green-600">Yes</span>
        ) : (
          <span className="text-gray-500">No</span>
        )}
      </td>

      <td className="px-4 py-4">
        {user.createdAt
          ? new Date(user.createdAt).toLocaleDateString()
          : "-"}
      </td>

      <td className="px-4 py-4">
        <div className="flex justify-end gap-2">
          <button
            type="button"
            disabled={updatingUserId === user.id || isCurrentUser}
            onClick={() => onToggleAdmin(user)}
            className="px-3 py-2 border rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {updatingUserId === user.id
              ? "Saving..."
              : hasAdminRole
              ? "Remove Admin"
              : "Make Admin"}
          </button>

          <button
            type="button"
            disabled={deletingUserId === user.id || isCurrentUser}
            onClick={() => onDelete(user)}
            className="px-3 py-2 rounded bg-red-600 text-white hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {deletingUserId === user.id ? "Deleting..." : "Delete"}
          </button>
        </div>
      </td>
    </tr>
  );
}

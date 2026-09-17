"use client";

import { AdminUser } from "@/src/lib/api";
import AdminUserRow from "./AdminUserRow";
import AdminUsersError from "./AdminUsersError";

interface Props {
  users: AdminUser[];
  error: string | null;
  updatingUserId: string | null;
  deletingUserId: string | null;
  onToggleAdmin: (user: AdminUser) => void;
  onDelete: (user: AdminUser) => void;
}

export default function AdminUsersTable({
  users,
  error,
  updatingUserId,
  deletingUserId,
  onToggleAdmin,
  onDelete,
}: Props) {
  return (
    <>
      {error && <AdminUsersError message={error} />}

      <div className="border rounded-lg overflow-hidden bg-white">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-100 border-b">
              <tr>
                <th className="text-left px-4 py-3">Name</th>
                <th className="text-left px-4 py-3">Email</th>
                <th className="text-left px-4 py-3">Roles</th>
                <th className="text-left px-4 py-3">Email Verified</th>
                <th className="text-left px-4 py-3">Created</th>
                <th className="text-right px-4 py-3">Actions</th>
              </tr>
            </thead>

            <tbody>
              {users.length === 0 ? (
                <tr>
                  <td
                    colSpan={6}
                    className="px-4 py-8 text-center text-gray-500"
                  >
                    No users found.
                  </td>
                </tr>
              ) : (
                users.map((user) => (
                  <AdminUserRow
                    key={user.id}
                    user={user}
                    updatingUserId={updatingUserId}
                    deletingUserId={deletingUserId}
                    onToggleAdmin={onToggleAdmin}
                    onDelete={onDelete}
                  />
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
}

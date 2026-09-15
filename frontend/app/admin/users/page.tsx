"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {
  api,
  getClientUserId,
  type AdminUser,
} from "@/src/lib/api";
import { useAuthStore } from "@/src/store/authStore";

export default function AdminUsersPage() {
  const router = useRouter();

  const isLoggedIn = useAuthStore(
    (state) => state.isLoggedIn,
  );

  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [authorized, setAuthorized] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [updatingUserId, setUpdatingUserId] =
    useState<string | null>(null);

  const [deletingUserId, setDeletingUserId] =
    useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadAdminPage() {
      try {
        setLoading(true);
        setError(null);

        /*
         * First make sure there is a logged-in user.
         */
        const currentUserId = getClientUserId();

        if (!currentUserId) {
          router.replace("/auth/login");
          return;
        }

        /*
         * IMPORTANT:
         *
         * Do not rely only on authStore.isAdmin.
         * The backend user record contains the authoritative roles.
         */
        const currentUser =
          await api.getUserById(currentUserId);

        const currentUserIsAdmin =
          currentUser.roles?.some(
            (role) =>
              role.trim().toLowerCase() === "admin",
          ) ?? false;

        if (!currentUserIsAdmin) {
          if (!cancelled) {
            setAuthorized(false);
          }

          router.replace("/");
          return;
        }

        if (!cancelled) {
          setAuthorized(true);
        }

        /*
         * Now that we know the current user is an admin,
         * load all users.
         */
        const result = await api.getUsers();

        if (!cancelled) {
          setUsers(result);
        }
      } catch (err) {
        console.error(
          "Failed to load admin users:",
          err,
        );

        if (!cancelled) {
          setError(
            err instanceof Error
              ? err.message
              : "Failed to load admin users.",
          );
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    /*
     * If Zustand says we are not logged in, redirect.
     *
     * The actual admin authorization is still checked
     * against the backend above.
     */
    if (!isLoggedIn) {
      router.replace("/auth/login");
      setLoading(false);
      return;
    }

    loadAdminPage();

    return () => {
      cancelled = true;
    };
  }, [isLoggedIn, router]);

  async function toggleAdminRole(user: AdminUser) {
    const currentRoles = user.roles ?? [];

    const hasAdminRole = currentRoles.some(
      (role) =>
        role.trim().toLowerCase() === "admin",
    );

    /*
     * The backend does not allow an admin to remove
     * their own Admin role, so don't even send the request.
     */
    const currentUserId = getClientUserId();

    if (currentUserId === user.id) {
      setError(
        "You cannot remove the Admin role from your own account.",
      );
      return;
    }

    const newRoles = hasAdminRole
      ? currentRoles.filter(
          (role) =>
            role.trim().toLowerCase() !== "admin",
        )
      : [...currentRoles, "Admin"];

    if (newRoles.length === 0) {
      setError(
        "A user must have at least one role.",
      );
      return;
    }

    setUpdatingUserId(user.id);
    setError(null);

    try {
      const updatedUser =
        await api.updateUserRoles(
          user.id,
          newRoles,
        );

      setUsers((currentUsers) =>
        currentUsers.map((currentUser) =>
          currentUser.id === user.id
            ? updatedUser
            : currentUser,
        ),
      );
    } catch (err) {
      console.error(
        "Failed to update roles:",
        err,
      );

      setError(
        err instanceof Error
          ? err.message
          : "Failed to update user roles.",
      );
    } finally {
      setUpdatingUserId(null);
    }
  }

  async function deleteUser(user: AdminUser) {
    const currentUserId = getClientUserId();

    /*
     * Never allow an admin to delete their own account.
     */
    if (currentUserId === user.id) {
      setError(
        "You cannot delete your own admin account.",
      );
      return;
    }

    const confirmed = window.confirm(
      `Are you sure you want to delete ${
        user.fullName || user.email
      }?`,
    );

    if (!confirmed) {
      return;
    }

    setDeletingUserId(user.id);
    setError(null);

    try {
      await api.deleteUser(user.id);

      setUsers((currentUsers) =>
        currentUsers.filter(
          (currentUser) =>
            currentUser.id !== user.id,
        ),
      );
    } catch (err) {
      console.error(
        "Failed to delete user:",
        err,
      );

      setError(
        err instanceof Error
          ? err.message
          : "Failed to delete user.",
      );
    } finally {
      setDeletingUserId(null);
    }
  }

  if (loading) {
    return (
      <main className="max-w-6xl mx-auto px-4 py-10">
        <p className="text-gray-600">
          Loading admin panel...
        </p>
      </main>
    );
  }

  if (!isLoggedIn || !authorized) {
    return null;
  }

  return (
    <main className="max-w-6xl mx-auto px-4 py-10">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold">
            User Management
          </h1>

          <p className="text-gray-600 mt-2">
            Manage registered users and roles.
          </p>
        </div>

        <button
          type="button"
          onClick={() => router.push("/")}
          className="px-4 py-2 border rounded hover:bg-gray-50"
        >
          Back
        </button>
      </div>

      {error && (
        <div className="mb-6 rounded border border-red-300 bg-red-50 px-4 py-3 text-red-700">
          {error}
        </div>
      )}

      <div className="border rounded-lg overflow-hidden bg-white">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-100 border-b">
              <tr>
                <th className="text-left px-4 py-3">
                  Name
                </th>

                <th className="text-left px-4 py-3">
                  Email
                </th>

                <th className="text-left px-4 py-3">
                  Roles
                </th>

                <th className="text-left px-4 py-3">
                  Email Verified
                </th>

                <th className="text-left px-4 py-3">
                  Created
                </th>

                <th className="text-right px-4 py-3">
                  Actions
                </th>
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
                users.map((user) => {
                  const hasAdminRole =
                    user.roles?.some(
                      (role) =>
                        role.trim().toLowerCase() ===
                        "admin",
                    ) ?? false;

                  const currentUserId =
                    getClientUserId();

                  const isCurrentUser =
                    currentUserId === user.id;

                  return (
                    <tr
                      key={user.id}
                      className="border-b last:border-b-0"
                    >
                      <td className="px-4 py-4">
                        <div className="font-medium">
                          {user.fullName ||
                            "Unnamed user"}
                        </div>

                        {isCurrentUser && (
                          <span className="text-xs text-gray-500">
                            You
                          </span>
                        )}
                      </td>

                      <td className="px-4 py-4">
                        {user.email}
                      </td>

                      <td className="px-4 py-4">
                        <div className="flex flex-wrap gap-2">
                          {user.roles?.map(
                            (role) => (
                              <span
                                key={role}
                                className="rounded-full bg-gray-100 px-2 py-1 text-xs"
                              >
                                {role}
                              </span>
                            ),
                          )}
                        </div>
                      </td>

                      <td className="px-4 py-4">
                        {user.isEmailVerified ? (
                          <span className="text-green-600">
                            Yes
                          </span>
                        ) : (
                          <span className="text-gray-500">
                            No
                          </span>
                        )}
                      </td>

                      <td className="px-4 py-4">
                        {user.createdAt
                          ? new Date(
                              user.createdAt,
                            ).toLocaleDateString()
                          : "-"}
                      </td>

                      <td className="px-4 py-4">
                        <div className="flex justify-end gap-2">
                          <button
                            type="button"
                            disabled={
                              updatingUserId ===
                                user.id ||
                              isCurrentUser
                            }
                            onClick={() =>
                              toggleAdminRole(user)
                            }
                            className="px-3 py-2 border rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            {updatingUserId ===
                            user.id
                              ? "Saving..."
                              : hasAdminRole
                                ? "Remove Admin"
                                : "Make Admin"}
                          </button>

                          <button
                            type="button"
                            disabled={
                              deletingUserId ===
                                user.id ||
                              isCurrentUser
                            }
                            onClick={() =>
                              deleteUser(user)
                            }
                            className="px-3 py-2 rounded bg-red-600 text-white hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            {deletingUserId ===
                            user.id
                              ? "Deleting..."
                              : "Delete"}
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>

      <p className="mt-4 text-sm text-gray-500">
        Total users: {users.length}
      </p>
    </main>
  );
}
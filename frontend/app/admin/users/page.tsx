"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { api, getClientUserId, AdminUser } from "@/src/lib/api";

import { useAuthStore } from "@/src/store/authStore";
import { useSnackbar } from "@/src/context/SnackbarContext";

import AdminUsersHeader from "@/app/components/Admin/Users/AdminUsersHeader";
import AdminUsersTable from "@/app/components/Admin/Users/AdminUsersTable";
import AdminUsersSummary from "@/app/components/Admin/Users/AdminUsersSummary";
import LoadingAdminUsers from "@/app/components/Admin/Users/LoadingAdminUsers";

export default function AdminUsersPage() {
  const router = useRouter();
  const { showSnackbar } = useSnackbar();
  const isLoggedIn = useAuthStore((state) => state.isLoggedIn);

  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [authorized, setAuthorized] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [updatingUserId, setUpdatingUserId] = useState<string | null>(null);
  const [deletingUserId, setDeletingUserId] = useState<string | null>(null);

  // ---------------------------------------------------------
  // LOAD USERS
  // ---------------------------------------------------------
  async function loadUsers() {
    try {
      const result = await api.getUsers();
      setUsers(result);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to load admin users."
      );
    }
  }

  // ---------------------------------------------------------
  // INITIAL ADMIN CHECK + LOAD
  // ---------------------------------------------------------
  useEffect(() => {
    let cancelled = false;

    async function loadAdminPage() {
      try {
        setLoading(true);
        setError(null);

        const currentUserId = getClientUserId();
        if (!currentUserId) {
          router.replace("/auth/login");
          return;
        }

        const currentUser = await api.getUserById(currentUserId);
        const isAdmin =
          currentUser.roles?.some(
            (role) => role.trim().toLowerCase() === "admin"
          ) ?? false;

        if (!isAdmin) {
          if (!cancelled) setAuthorized(false);
          router.replace("/");
          return;
        }

        if (!cancelled) setAuthorized(true);

        const result = await api.getUsers();
        if (!cancelled) setUsers(result);
      } catch (err) {
        if (!cancelled) {
          setError(
            err instanceof Error ? err.message : "Failed to load admin users."
          );
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

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

  // ---------------------------------------------------------
  // TOGGLE ADMIN ROLE
  // ---------------------------------------------------------
  async function handleToggleAdmin(user: AdminUser) {
    const currentRoles = user.roles ?? [];
    const hasAdminRole = currentRoles.some(
      (role) => role.trim().toLowerCase() === "admin"
    );

    const currentUserId = getClientUserId();
    if (currentUserId === user.id) {
      showSnackbar("You cannot remove Admin role from your own account.", "error");
      return;
    }

    const newRoles = hasAdminRole
      ? currentRoles.filter((role) => role.trim().toLowerCase() !== "admin")
      : [...currentRoles, "Admin"];

    if (newRoles.length === 0) {
      showSnackbar("A user must have at least one role.", "error");
      return;
    }

    setUpdatingUserId(user.id);

    try {
      const updatedUser = await api.updateUserRoles(user.id, newRoles);

      setUsers((current) =>
        current.map((u) => (u.id === user.id ? updatedUser : u))
      );

      showSnackbar("User roles updated.", "success");
    } catch (err) {
      showSnackbar(
        err instanceof Error ? err.message : "Failed to update user roles.",
        "error"
      );
    } finally {
      setUpdatingUserId(null);
    }
  }

  // ---------------------------------------------------------
  // DELETE USER
  // ---------------------------------------------------------
  async function handleDeleteUser(user: AdminUser) {
    setDeletingUserId(user.id);

    try {
      await api.deleteUser(user.id);

      showSnackbar(`User "${user.fullName ?? user.email}" deleted.`, "success");

      await loadUsers();
    } catch (err) {
      showSnackbar(
        err instanceof Error ? err.message : "Failed to delete user.",
        "error"
      );
    } finally {
      setDeletingUserId(null);
    }
  }

  // ---------------------------------------------------------
  // RENDER
  // ---------------------------------------------------------
  if (loading) {
    return (
      <main className="max-w-6xl mx-auto px-4 py-10">
        <LoadingAdminUsers />
      </main>
    );
  }

  if (!isLoggedIn || !authorized) {
    return null;
  }

  return (
    <main className="max-w-6xl mx-auto px-4 py-10">
      <AdminUsersHeader />

      <AdminUsersTable
        users={users}
        error={error}
        updatingUserId={updatingUserId}
        deletingUserId={deletingUserId}
        onToggleAdmin={handleToggleAdmin}
        onDelete={handleDeleteUser}
      />

      <AdminUsersSummary count={users.length} />
    </main>
  );
}
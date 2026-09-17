"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {
  api,
  getClientUserId,
  AdminUser,
} from "@/src/lib/api";

import { useAuthStore } from "@/src/store/authStore";

import AdminUsersHeader from "@/app/components/Admin/Users/AdminUsersHeader";
import AdminUsersTable from "@/app/components/Admin/Users/AdminUsersTable";
import AdminUsersSummary from "@/app/components/Admin/Users/AdminUsersSummary";
import LoadingAdminUsers from "@/app/components/Admin/Users/LoadingAdminUsers";

export default function AdminUsersPage() {
  const router = useRouter();
  const isLoggedIn = useAuthStore((state) => state.isLoggedIn);

  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [authorized, setAuthorized] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [updatingUserId, setUpdatingUserId] = useState<string | null>(null);
  const [deletingUserId, setDeletingUserId] = useState<string | null>(null);

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

  async function toggleAdminRole(user: AdminUser) {
    const currentRoles = user.roles ?? [];
    const hasAdminRole = currentRoles.some(
      (role) => role.trim().toLowerCase() === "admin"
    );

    const currentUserId = getClientUserId();
    if (currentUserId === user.id) {
      setError("You cannot remove the Admin role from your own account.");
      return;
    }

    const newRoles = hasAdminRole
      ? currentRoles.filter((role) => role.trim().toLowerCase() !== "admin")
      : [...currentRoles, "Admin"];

    if (newRoles.length === 0) {
      setError("A user must have at least one role.");
      return;
    }

    setUpdatingUserId(user.id);
    setError(null);

    try {
      const updatedUser = await api.updateUserRoles(user.id, newRoles);

      setUsers((current) =>
        current.map((u) => (u.id === user.id ? updatedUser : u))
      );
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to update user roles."
      );
    } finally {
      setUpdatingUserId(null);
    }
  }

  async function deleteUser(user: AdminUser) {
    const currentUserId = getClientUserId();
    if (currentUserId === user.id) {
      setError("You cannot delete your own admin account.");
      return;
    }

    const confirmed = window.confirm(
      `Are you sure you want to delete ${user.fullName || user.email}?`
    );

    if (!confirmed) return;

    setDeletingUserId(user.id);
    setError(null);

    try {
      await api.deleteUser(user.id);
      setUsers((current) => current.filter((u) => u.id !== user.id));
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to delete user."
      );
    } finally {
      setDeletingUserId(null);
    }
  }

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
        onToggleAdmin={toggleAdminRole}
        onDelete={deleteUser}
      />

      <AdminUsersSummary count={users.length} />
    </main>
  );
}
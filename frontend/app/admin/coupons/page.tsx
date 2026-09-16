"use client";

import { useEffect, useState } from "react";
import Container from "../../components/Container";
import { api, Coupon, clientIsAdmin, UserProfile } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";
import { useRouter } from "next/navigation";

export default function AdminCouponsPage() {
  const router = useRouter();
  const { showSnackbar } = useSnackbar();

  const [coupons, setCoupons] = useState<Coupon[]>([]);
  const [users, setUsers] = useState<UserProfile[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [creating, setCreating] = useState(false);

  const [code, setCode] = useState("");
  const [type, setType] = useState<"percentage" | "fixed">("percentage");
  const [value, setValue] = useState<number>(0);
  const [expiration, setExpiration] = useState("");
  const [maxUsage, setMaxUsage] = useState<number>(1);
  const [userId, setUserId] = useState<string>(""); // NEW

  const isAdmin = clientIsAdmin();

  // Redirect non-admins
  useEffect(() => {
    if (!isAdmin) {
      router.push("/coupons");
    }
  }, [isAdmin, router]);

  async function loadCoupons() {
    try {
      setError("");
      const list = await api.getCoupons();
      setCoupons(list);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load coupons.");
    } finally {
      setLoading(false);
    }
  }

  async function loadUsers() {
    try {
      const list = await api.getUsers();
      setUsers(list);
    } catch {
      showSnackbar("Failed to load users.", "error");
    }
  }

  useEffect(() => {
    loadCoupons();
    loadUsers(); // NEW
  }, []);

  async function handleCreateCoupon() {
    if (!code.trim()) {
      showSnackbar("Coupon code cannot be empty.", "error");
      return;
    }

    if (value <= 0) {
      showSnackbar("Value must be greater than zero.", "error");
      return;
    }

    try {
      setCreating(true);

      const newCoupon: Coupon = {
        id: "",
        code: code.trim(),
        type,
        value,
        expiration: new Date(expiration).toISOString(),
        maxUsage,
        usedCount: 0,
        active: true,
        userId: userId || null, // NEW
      };

      const created = await api.createCoupon(newCoupon);

      setCoupons((prev) => [...prev, created]);
      showSnackbar("Coupon created successfully!", "success");

      // Reset form
      setCode("");
      setValue(0);
      setExpiration("");
      setMaxUsage(1);
      setUserId("");
    } catch (err) {
      showSnackbar(
        err instanceof Error ? err.message : "Failed to create coupon.",
        "error"
      );
    } finally {
      setCreating(false);
    }
  }

  async function handleDeleteCoupon(id: string) {
    try {
      await api.deleteCoupon(id);
      setCoupons((prev) => prev.filter((c) => c.id !== id));
      showSnackbar("Coupon deleted.", "success");
    } catch (err) {
      showSnackbar(
        err instanceof Error ? err.message : "Failed to delete coupon.",
        "error"
      );
    }
  }

  if (!isAdmin) {
    return null;
  }

  if (loading) {
    return (
      <Container>
        <h1 className="text-2xl font-bold mb-6">Admin — Coupons</h1>
        <p className="text-gray-600">Loading coupons...</p>
      </Container>
    );
  }

  if (error) {
    return (
      <Container>
        <h1 className="text-2xl font-bold mb-6">Admin — Coupons</h1>
        <p className="text-red-600 bg-red-100 px-3 py-2 rounded">{error}</p>
      </Container>
    );
  }

  return (
    <Container>
      <h1 className="text-2xl font-bold mb-6">Admin — Manage Coupons</h1>

      {/* CREATE COUPON */}
      <div className="border rounded-lg bg-white p-5 mb-8 shadow-sm">
        <h2 className="font-semibold text-lg mb-4">Create New Coupon</h2>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium mb-1">Code</label>
            <input
              type="text"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              className="w-full border rounded px-3 py-2"
              placeholder="SUMMER20"
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Type</label>
            <select
              value={type}
              onChange={(e) =>
                setType(e.target.value as "percentage" | "fixed")
              }
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
              onChange={(e) => setValue(Number(e.target.value))}
              className="w-full border rounded px-3 py-2"
              placeholder="10"
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Expiration</label>
            <input
              type="date"
              value={expiration}
              onChange={(e) => setExpiration(e.target.value)}
              className="w-full border rounded px-3 py-2"
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Max Usage</label>
            <input
              type="number"
              value={maxUsage}
              onChange={(e) => setMaxUsage(Number(e.target.value))}
              className="w-full border rounded px-3 py-2"
              placeholder="1"
            />
          </div>

          {/* NEW: Assign to user */}
          <div>
            <label className="block text-sm font-medium mb-1">Assign to User</label>
            <select
              value={userId}
              onChange={(e) => setUserId(e.target.value)}
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
          onClick={handleCreateCoupon}
          disabled={creating}
          className="mt-4 bg-black text-white px-5 py-2 rounded hover:bg-gray-800 disabled:opacity-50"
        >
          {creating ? "Creating..." : "Create Coupon"}
        </button>
      </div>

      {/* LIST COUPONS */}
      <div className="border rounded-lg bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-lg mb-4">Existing Coupons</h2>

        {coupons.length === 0 ? (
          <p className="text-gray-600">No coupons created yet.</p>
        ) : (
          <div className="space-y-4">
            {coupons.map((coupon) => (
              <div
                key={coupon.id}
                className="border rounded p-4 flex items-center justify-between"
              >
                <div>
                  <p className="font-semibold">{coupon.code}</p>
                  <p className="text-sm text-gray-600">
                    {coupon.type === "percentage"
                      ? `${coupon.value}%`
                      : `€${coupon.value}`}
                  </p>
                  <p className="text-xs text-gray-500 mt-1">
                    Expires: {new Date(coupon.expiration).toLocaleDateString()}
                  </p>
                  <p className="text-xs text-gray-500">
                    Usage: {coupon.usedCount}/{coupon.maxUsage}
                  </p>

                  {/* NEW: Show assigned user */}
                  <p className="text-xs text-gray-500 mt-1">
                    Assigned to:{" "}
                    {coupon.userId
                      ? users.find((u) => u.id === coupon.userId)?.email ??
                        coupon.userId
                      : "Global"}
                  </p>
                </div>

                <button
                  onClick={() => handleDeleteCoupon(coupon.id)}
                  className="text-red-600 hover:underline"
                >
                  Delete
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </Container>
  );
}

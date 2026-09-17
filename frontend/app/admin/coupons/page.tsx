"use client";

import { useEffect, useState } from "react";
import Container from "../../components/Container";
import { api, Coupon, UserProfile, clientIsAdmin } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";
import { useRouter } from "next/navigation";

import AdminCouponsHeader from "../../components/Admin/Coupons/AdminCouponsHeader";
import AdminCouponsLoading from "../../components/Admin/Coupons/AdminCouponsLoading";
import AdminCouponsError from "../../components/Admin/Coupons/AdminCouponsError";
import AdminCouponsCreateForm from "../../components/Admin/Coupons/AdminCouponsCreateForm";
import AdminCouponsList from "../../components/Admin/Coupons/AdminCouponsList";

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
  const [userId, setUserId] = useState<string>("");

  const isAdmin = clientIsAdmin();

  useEffect(() => {
    if (!isAdmin) router.push("/coupons");
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
    loadUsers();
  }, []);

  function updateField(field: string, value: any) {
    if (field === "code") setCode(value);
    if (field === "type") setType(value);
    if (field === "value") setValue(value);
    if (field === "expiration") setExpiration(value);
    if (field === "maxUsage") setMaxUsage(value);
    if (field === "userId") setUserId(value);
  }

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
        userId: userId || null,
      };

      const created = await api.createCoupon(newCoupon);

      setCoupons((prev) => [...prev, created]);
      showSnackbar("Coupon created successfully!", "success");

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

  if (!isAdmin) return null;

  if (loading) {
    return (
      <Container>
        <AdminCouponsHeader />
        <AdminCouponsLoading />
      </Container>
    );
  }

  if (error) {
    return (
      <Container>
        <AdminCouponsHeader />
        <AdminCouponsError message={error} />
      </Container>
    );
  }

  return (
    <Container>
      <AdminCouponsHeader />

      <AdminCouponsCreateForm
        code={code}
        type={type}
        value={value}
        expiration={expiration}
        maxUsage={maxUsage}
        userId={userId}
        users={users}
        creating={creating}
        onChange={updateField}
        onSubmit={handleCreateCoupon}
      />

      <AdminCouponsList
        coupons={coupons}
        users={users}
        onDelete={handleDeleteCoupon}
      />
    </Container>
  );
}

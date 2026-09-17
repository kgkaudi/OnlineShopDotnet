"use client";

import { useEffect, useState } from "react";
import Container from "../components/Container";

import CouponsGrid from "../components/Coupons/CouponsGrid";
import EmptyCoupons from "../components/Coupons/EmptyCoupons";
import LoadingCoupons from "../components/Coupons/LoadingCoupons";
import CouponsError from "../components/Coupons/CouponsError";

import { api, Coupon } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

export default function CouponsPage() {
  const [coupons, setCoupons] = useState<Coupon[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const { showSnackbar } = useSnackbar();

  async function loadCoupons() {
    try {
      setError("");
      const list = await api.getMyCoupons();
      setCoupons(list);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load coupons.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadCoupons();
  }, []);

  return (
    <Container>
      <h1 className="text-2xl font-bold mb-6">Available Coupons</h1>

      {loading && <LoadingCoupons />}
      {error && <CouponsError message={error} />}

      {!loading && !error && coupons.length === 0 && <EmptyCoupons />}

      {!loading && !error && coupons.length > 0 && (
        <div className="border rounded-lg bg-white p-5 shadow-sm">
          <CouponsGrid coupons={coupons} />
        </div>
      )}
    </Container>
  );
}

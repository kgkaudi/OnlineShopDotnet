"use client";

import { useEffect, useState } from "react";
import Container from "../components/Container";
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
      const list = await api.getMyCoupons(); // ✔ correct endpoint
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

  if (loading) {
    return (
      <Container>
        <h1 className="text-2xl font-bold mb-6">Available Coupons</h1>
        <p className="text-gray-600">Loading coupons...</p>
      </Container>
    );
  }

  if (error) {
    return (
      <Container>
        <h1 className="text-2xl font-bold mb-6">Available Coupons</h1>
        <p className="text-red-600 bg-red-100 px-3 py-2 rounded">{error}</p>
      </Container>
    );
  }

  return (
    <Container>
      <h1 className="text-2xl font-bold mb-6">Available Coupons</h1>

      <div className="border rounded-lg bg-white p-5 shadow-sm">
        {coupons.length === 0 ? (
          <p className="text-gray-600">No coupons available at the moment.</p>
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
                      ? `${coupon.value}% off`
                      : `€${coupon.value} off`}
                  </p>

                  <p className="text-xs text-gray-500 mt-1">
                    Expires:{" "}
                    {coupon.expiration
                      ? new Date(coupon.expiration).toLocaleDateString()
                      : "Unknown"}
                  </p>

                  <p className="text-xs text-gray-500">
                    Usage: {coupon.usedCount}/{coupon.maxUsage}
                  </p>

                  {!coupon.active && (
                    <p className="text-xs text-red-600 mt-1">
                      This coupon is inactive.
                    </p>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </Container>
  );
}

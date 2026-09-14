"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/src/store/authStore";
import { logout as logoutApi } from "@/src/api/auth";
import { useEffect, useState } from "react";

export default function Header() {
  const router = useRouter();
  const isLoggedIn = useAuthStore((state) => state.isLoggedIn);
  const setLoggedIn = useAuthStore((state) => state.setLoggedIn);
  const [hydrated, setHydrated] = useState(false);

  useEffect(() => {
    // Mark hydration complete
    setHydrated(true);
  }, []);

  async function handleLogout() {
    try {
      await logoutApi();
    } catch (err) {
      console.error("Logout failed:", err);
    }

    setLoggedIn(false);
    router.push("/auth/login");
  }

  return (
    <header className="border-b bg-white">
      <div className="max-w-6xl mx-auto px-4 py-4 flex items-center justify-between">
        <Link href="/" className="text-xl font-bold">
          OnlineShop
        </Link>

        <nav className="flex items-center gap-6 text-sm">
          <Link href="/products" className="text-black hover:underline">
            Products
          </Link>
          <Link href="/cart" className="text-black hover:underline">
            Cart
          </Link>
          <Link href="/wishlist" className="text-black hover:underline">
            Wishlist
          </Link>

          {/* Render login/logout only after hydration */}
          {hydrated && (
            <>
              {!isLoggedIn && (
                <Link href="/auth/login" className="text-black hover:underline">
                  Login
                </Link>
              )}

              {isLoggedIn && (
                <button
                  onClick={handleLogout}
                  className="text-black hover:underline"
                >
                  Logout
                </button>
              )}
            </>
          )}
        </nav>
      </div>
    </header>
  );
}

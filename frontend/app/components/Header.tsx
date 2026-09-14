"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/src/store/authStore";
import { logout as logoutApi } from "@/src/api/auth";

export default function Header() {
  const router = useRouter();

  // Subscribe to global auth state
  const isLoggedIn = useAuthStore((state) => state.isLoggedIn);
  const setLoggedIn = useAuthStore((state) => state.setLoggedIn);

  async function handleLogout() {
    // Backend logout (token invalidation)
    await logoutApi();

    // Update global auth state
    setLoggedIn(false);

    // Redirect
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
        </nav>
      </div>
    </header>
  );
}

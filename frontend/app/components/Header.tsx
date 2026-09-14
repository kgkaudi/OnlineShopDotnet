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
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
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
    <header className="border-b bg-white sticky top-0 z-50">
      <div className="max-w-6xl mx-auto px-4 py-4 flex items-center justify-between">
        {/* Logo */}
        <Link href="/" className="text-xl font-bold">
          OnlineShop
        </Link>

        {/* Desktop Navigation */}
        <nav className="hidden md:flex items-center gap-6 text-sm">
          <Link href="/products" className="text-black hover:underline">
            Products
          </Link>
          <Link href="/cart" className="text-black hover:underline">
            Cart
          </Link>
          <Link href="/wishlist" className="text-black hover:underline">
            Wishlist
          </Link>

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

        {/* Mobile Hamburger */}
        <button
          className="md:hidden p-2 border rounded"
          onClick={() => setMenuOpen(true)}
        >
          <span className="text-xl">☰</span>
        </button>
      </div>

      {/* Mobile Menu Overlay */}
      {menuOpen && (
        <div
          className="fixed inset-0 bg-black/40 backdrop-blur-sm z-40"
          onClick={() => setMenuOpen(false)}
        />
      )}

      {/* Mobile Slide-in Menu */}
      <div
        className={`fixed top-0 right-0 w-64 h-full bg-white shadow-lg z-50 transform transition-transform duration-300 ease-out ${
          menuOpen ? "translate-x-0" : "translate-x-full"
        }`}
      >
        <div className="p-6 flex flex-col space-y-4 text-lg"> {/* ✅ vertical layout */}
          <Link href="/" onClick={() => setMenuOpen(false)} className="block">
            Home
          </Link>
          <Link href="/products" onClick={() => setMenuOpen(false)} className="block">
            Products
          </Link>
          <Link href="/cart" onClick={() => setMenuOpen(false)} className="block">
            Cart
          </Link>
          <Link href="/wishlist" onClick={() => setMenuOpen(false)} className="block">
            Wishlist
          </Link>

          {hydrated && (
            <>
              {!isLoggedIn && (
                <Link
                  href="/auth/login"
                  onClick={() => setMenuOpen(false)}
                  className="block"
                >
                  Login
                </Link>
              )}

              {isLoggedIn && (
                <button
                  onClick={() => {
                    handleLogout();
                    setMenuOpen(false);
                  }}
                  className="text-left w-full"
                >
                  Logout
                </button>
              )}
            </>
          )}
        </div>
      </div>
    </header>
  );
}

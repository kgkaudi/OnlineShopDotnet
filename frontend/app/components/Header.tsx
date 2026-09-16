"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";

import { useAuthStore } from "@/src/store/authStore";
import { logout as logoutApi } from "@/src/api/auth";
import { clientIsAdmin } from "@/src/lib/api";

export default function Header() {
  const router = useRouter();

  const isLoggedIn = useAuthStore((state) => state.isLoggedIn);
  const setLoggedIn = useAuthStore((state) => state.setLoggedIn);

  const [hydrated, setHydrated] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);

  useEffect(() => {
    setHydrated(true);

    if (isLoggedIn) {
      setIsAdmin(clientIsAdmin());
    } else {
      setIsAdmin(false);
    }
  }, [isLoggedIn]);

  function closeMenu() {
    setMenuOpen(false);
  }

  async function handleLogout() {
    try {
      await logoutApi();
    } catch (err) {
      console.error("Logout failed:", err);
    }

    setIsAdmin(false);
    setLoggedIn(false);
    setMenuOpen(false);

    router.push("/auth/login");
  }

  return (
    <header className="sticky top-0 z-50 border-b bg-white">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-4">
        {/* Logo */}
        <Link href="/" className="text-xl font-bold">
          OnlineShop
        </Link>

        {/* Desktop Navigation */}
        <nav className="hidden items-center gap-6 text-sm md:flex">
          <Link href="/products" className="text-black hover:underline">
            Products
          </Link>

          <Link href="/cart" className="text-black hover:underline">
            Cart
          </Link>

          <Link href="/wishlist" className="text-black hover:underline">
            Wishlist
          </Link>

          {hydrated && isLoggedIn && (
            <>
              <Link href="/orders" className="text-black hover:underline">
                Orders
              </Link>

              <Link href="/profile" className="text-black hover:underline">
                Profile
              </Link>

              {isAdmin && (
                <>
                  <Link
                    href="/admin/users"
                    className="font-semibold text-black hover:underline"
                  >
                    Users Admin
                  </Link>

                  <Link
                    href="/admin/products"
                    className="font-semibold text-black hover:underline"
                  >
                    Products Admin
                  </Link>

                  <Link
                    href="/admin/categories"
                    className="font-semibold text-black hover:underline"
                  >
                    Categories Admin
                  </Link>

                  <Link
                    href="/admin/reviews"
                    className="font-semibold text-black hover:underline"
                  >
                    Reviews Admin
                  </Link>
                </>
              )}
            </>
          )}

          {/* Authentication */}
          {hydrated && (
            <>
              {!isLoggedIn && (
                <Link href="/auth/login" className="text-black hover:underline">
                  Login
                </Link>
              )}

              {isLoggedIn && (
                <button
                  type="button"
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
          type="button"
          className="rounded border p-2 md:hidden"
          onClick={() => setMenuOpen(true)}
          aria-label="Open navigation menu"
          aria-expanded={menuOpen}
        >
          <span className="text-xl">☰</span>
        </button>
      </div>

      {/* Mobile Overlay */}
      {menuOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/40 backdrop-blur-sm"
          onClick={closeMenu}
          aria-hidden="true"
        />
      )}

      {/* Mobile Slide-in Menu */}
      <div
        className={`fixed right-0 top-0 z-50 h-full w-64 transform bg-white shadow-lg transition-transform duration-300 ease-out ${
          menuOpen ? "translate-x-0" : "translate-x-full"
        }`}
      >
        <div className="flex flex-col space-y-4 p-6 text-lg">
          {/* Close */}
          <button
            type="button"
            onClick={closeMenu}
            className="mb-2 self-end text-2xl leading-none"
            aria-label="Close navigation menu"
          >
            ×
          </button>

          <Link href="/" onClick={closeMenu} className="block">
            Home
          </Link>

          <Link href="/products" onClick={closeMenu} className="block">
            Products
          </Link>

          <Link href="/cart" onClick={closeMenu} className="block">
            Cart
          </Link>

          <Link href="/wishlist" onClick={closeMenu} className="block">
            Wishlist
          </Link>

          {hydrated && isLoggedIn && (
            <>
              <Link href="/orders" onClick={closeMenu} className="block">
                Orders
              </Link>

              <Link href="/profile" onClick={closeMenu} className="block">
                Profile
              </Link>

              {isAdmin && (
                <div className="mt-2 border-t pt-4">
                  <p className="mb-3 text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Admin
                  </p>

                  <div className="space-y-3">
                    <Link
                      href="/admin/users"
                      onClick={closeMenu}
                      className="block font-semibold"
                    >
                      Users Admin
                    </Link>

                    <Link
                      href="/admin/products"
                      onClick={closeMenu}
                      className="block font-semibold"
                    >
                      Products Admin
                    </Link>

                    <Link
                      href="/admin/categories"
                      onClick={closeMenu}
                      className="block font-semibold"
                    >
                      Categories Admin
                    </Link>

                    <Link
                      href="/admin/reviews"
                      onClick={closeMenu}
                      className="block font-semibold"
                    >
                      Reviews Admin
                    </Link>
                  </div>
                </div>
              )}
            </>
          )}

          {/* Authentication */}
          {hydrated && (
            <div className="mt-2 border-t pt-4">
              {!isLoggedIn && (
                <Link href="/auth/login" onClick={closeMenu} className="block">
                  Login
                </Link>
              )}

              {isLoggedIn && (
                <button
                  type="button"
                  onClick={handleLogout}
                  className="w-full text-left"
                >
                  Logout
                </button>
              )}
            </div>
          )}
        </div>
      </div>
    </header>
  );
}

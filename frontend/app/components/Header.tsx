"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";

import { useAuthStore } from "@/src/store/authStore";
import { logout as logoutApi } from "@/src/api/auth";
import { clientIsAdmin, api } from "@/src/lib/api";

export default function Header() {
  const router = useRouter();

  const isLoggedInStore = useAuthStore((state) => state.isLoggedIn);
  const setLoggedIn = useAuthStore((state) => state.setLoggedIn);

  const [hydrated, setHydrated] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const [username, setUsername] = useState<string | null>(null);

  useEffect(() => {
    setHydrated(true);
    setIsLoggedIn(isLoggedInStore);

    if (isLoggedInStore) {
      setIsAdmin(clientIsAdmin());

      const token = localStorage.getItem("token");
      const payloadPart = token?.split(".")[1];

      if (payloadPart) {
        try {
          const base64 = payloadPart.replace(/-/g, "+").replace(/_/g, "/");
          const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, "=");
          const payload = JSON.parse(atob(padded));

          const userId = payload.sub;

          if (userId) {
            api.getUserProfile(userId).then((profile) => {
              setUsername(profile.fullName ?? profile.email);
            });
          }
        } catch {
          setUsername(null);
        }
      }
    } else {
      setIsAdmin(false);
      setUsername(null);
    }
  }, [isLoggedInStore]);

  // Prevent hydration mismatch
  if (!hydrated) {
    return (
      <header className="sticky top-0 z-50 border-b bg-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-4">
          <Link href="/" className="text-xl font-bold">
            OnlineShop
          </Link>
        </div>
      </header>
    );
  }

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
        <div className="flex items-center gap-3">
          <Link href="/" className="text-xl font-bold">
            OnlineShop
          </Link>

          {username && (
            <span className="hidden md:inline text-gray-600 font-medium text-sm">
              Welcome, {username}
            </span>
          )}
        </div>

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

          {isLoggedIn && (
            <>
              <Link href="/orders" className="text-black hover:underline">
                Orders
              </Link>

              <Link href="/coupons" className="text-black hover:underline">
                My Coupons
              </Link>

              <Link href="/profile" className="text-black hover:underline">
                Profile
              </Link>

              {isAdmin && (
                <div className="relative group">
                  <button className="font-semibold text-black hover:underline">
                    Admin ▾
                  </button>

                  {/* Dropdown */}
                  <div className="absolute right-0 mt-2 w-48 bg-white border rounded-lg shadow-lg opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200">
                    <Link
                      href="/admin/users"
                      className="block px-4 py-2 hover:bg-gray-100"
                    >
                      Users
                    </Link>

                    <Link
                      href="/admin/products"
                      className="block px-4 py-2 hover:bg-gray-100"
                    >
                      Products
                    </Link>

                    <Link
                      href="/admin/categories"
                      className="block px-4 py-2 hover:bg-gray-100"
                    >
                      Categories
                    </Link>

                    <Link
                      href="/admin/reviews"
                      className="block px-4 py-2 hover:bg-gray-100"
                    >
                      Reviews
                    </Link>

                    <Link
                      href="/admin/orders"
                      className="block px-4 py-2 hover:bg-gray-100"
                    >
                      Orders
                    </Link>

                    <Link
                      href="/admin/coupons"
                      className="block px-4 py-2 hover:bg-gray-100"
                    >
                      Coupons
                    </Link>
                  </div>
                </div>
              )}
            </>
          )}

          {/* Authentication */}
          {!isLoggedIn ? (
            <Link href="/auth/login" className="text-black hover:underline">
              Login
            </Link>
          ) : (
            <button
              type="button"
              onClick={handleLogout}
              className="text-red-600 hover:text-red-700 font-semibold"
            >
              Logout
            </button>
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

          {/* Mobile Welcome */}
          {username && (
            <p className="text-gray-600 font-medium">Welcome, {username}</p>
          )}

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

          {isLoggedIn && (
            <>
              <Link href="/orders" onClick={closeMenu} className="block">
                Orders
              </Link>

              <Link href="/coupons" onClick={closeMenu} className="block">
                My Coupons
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

                    <Link
                      href="/admin/orders"
                      onClick={closeMenu}
                      className="block font-semibold"
                    >
                      Orders Admin
                    </Link>

                    <Link
                      href="/admin/coupons"
                      onClick={closeMenu}
                      className="block font-semibold"
                    >
                      Coupons Admin
                    </Link>
                  </div>
                </div>
              )}
            </>
          )}

          {/* Authentication */}
          <div className="mt-2 border-t pt-4">
            {!isLoggedIn ? (
              <Link href="/auth/login" onClick={closeMenu} className="block">
                Login
              </Link>
            ) : (
              <button
                type="button"
                onClick={handleLogout}
                className="w-full text-left text-red-600 hover:text-red-700 font-semibold"
              >
                Logout
              </button>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}

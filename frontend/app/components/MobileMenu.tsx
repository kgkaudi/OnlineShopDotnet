"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { clientIsAdmin, getClientToken } from "@/src/lib/api";

export default function MobileMenu() {
  const [open, setOpen] = useState(false);
  const [hydrated, setHydrated] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);

  const router = useRouter();

  useEffect(() => {
    // Mark hydration complete
    setHydrated(true);

    // Now safe to read browser-only APIs
    const token = getClientToken();

    if (token) {
      setIsLoggedIn(true);
      setIsAdmin(clientIsAdmin());
    } else {
      setIsLoggedIn(false);
      setIsAdmin(false);
    }
  }, []);

  // 🚨 FIX: Prevent hydration mismatch
  // Render a stable placeholder until hydration is complete
  if (!hydrated) {
    return (
      <button
        type="button"
        className="md:hidden p-2 rounded border border-gray-300"
        aria-label="Open navigation menu"
      >
        <span className="text-xl">☰</span>
      </button>
    );
  }

  function closeMenu() {
    setOpen(false);
  }

  function handleLogout() {
    localStorage.removeItem("token");
    setIsLoggedIn(false);
    setIsAdmin(false);
    closeMenu();
    router.push("/auth/login");
  }

  return (
    <>
      {/* Hamburger button */}
      <button
        type="button"
        className="md:hidden p-2 rounded border border-gray-300"
        onClick={() => setOpen(true)}
        aria-label="Open navigation menu"
        aria-expanded={open}
      >
        <span className="text-xl">☰</span>
      </button>

      {/* Overlay */}
      {open && (
        <div
          className="fixed inset-0 bg-black/40 backdrop-blur-sm z-40"
          onClick={closeMenu}
          aria-hidden="true"
        />
      )}

      {/* Slide-in menu */}
      <div
        className={`fixed top-0 right-0 w-64 h-full bg-white shadow-lg z-50 transform transition-transform duration-300 ease-out ${
          open ? "translate-x-0" : "translate-x-full"
        }`}
      >
        <div className="p-6 flex flex-col space-y-4">
          {/* Close button */}
          <button
            type="button"
            onClick={closeMenu}
            className="self-end text-2xl leading-none mb-2"
            aria-label="Close navigation menu"
          >
            ×
          </button>

          {/* Public links */}
          <Link href="/" onClick={closeMenu} className="block text-lg">
            Home
          </Link>

          <Link href="/products" onClick={closeMenu} className="block text-lg">
            Products
          </Link>

          <Link href="/cart" onClick={closeMenu} className="block text-lg">
            Cart
          </Link>

          <Link href="/wishlist" onClick={closeMenu} className="block text-lg">
            Wishlist
          </Link>

          <Link href="/coupons" onClick={closeMenu} className="block text-lg">
            Coupons
          </Link>

          <Link href="/orders" onClick={closeMenu} className="block text-lg">
            Orders
          </Link>

          {/* Logged-in user section */}
          {isLoggedIn && (
            <>
              <Link
                href="/profile"
                onClick={closeMenu}
                className="block text-lg"
              >
                Profile
              </Link>

              {/* Admin section */}
              {isAdmin && (
                <div className="border-t pt-4 mt-2">
                  <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-3">
                    Admin
                  </p>

                  <div className="space-y-3">
                    <Link
                      href="/admin/users"
                      onClick={closeMenu}
                      className="block text-lg font-semibold"
                    >
                      Users
                    </Link>

                    <Link
                      href="/admin/products"
                      onClick={closeMenu}
                      className="block text-lg font-semibold"
                    >
                      Products Admin
                    </Link>

                    <Link
                      href="/admin/categories"
                      onClick={closeMenu}
                      className="block text-lg font-semibold"
                    >
                      Categories Admin
                    </Link>

                    <Link
                      href="/admin/reviews"
                      onClick={closeMenu}
                      className="block text-lg font-semibold"
                    >
                      Reviews Admin
                    </Link>

                    <Link
                      href="/admin/coupons"
                      onClick={closeMenu}
                      className="block text-lg font-semibold"
                    >
                      Coupons Admin
                    </Link>
                  </div>
                </div>
              )}
            </>
          )}

          {/* Login / Logout */}
          <div className="border-t pt-4 mt-2">
            {!isLoggedIn ? (
              <Link
                href="/auth/login"
                onClick={closeMenu}
                className="block text-lg"
              >
                Login
              </Link>
            ) : (
              <button
                type="button"
                onClick={handleLogout}
                className="block text-lg text-left w-full text-red-600 hover:text-red-700 font-semibold"
              >
                Logout
              </button>
            )}
          </div>
        </div>
      </div>
    </>
  );
}

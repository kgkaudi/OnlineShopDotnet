"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { clientIsAdmin } from "@/src/lib/api";

export default function MobileMenu() {
  const [open, setOpen] = useState(false);
  const [hydrated, setHydrated] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);

  useEffect(() => {
    setHydrated(true);

    const token = localStorage.getItem("token");

    if (token) {
      setIsLoggedIn(true);
      setIsAdmin(clientIsAdmin());
    } else {
      setIsLoggedIn(false);
      setIsAdmin(false);
    }
  }, []);

  function closeMenu() {
    setOpen(false);
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

          <Link href="/orders" onClick={closeMenu} className="block text-lg">
            Orders
          </Link>

          {hydrated && isLoggedIn && (
            <>
              <Link
                href="/profile"
                onClick={closeMenu}
                className="block text-lg"
              >
                Profile
              </Link>

              {isAdmin && (
                <>
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
                    </div>
                  </div>
                </>
              )}
            </>
          )}

          {hydrated && (
            <div className="border-t pt-4 mt-2">
              {!isLoggedIn && (
                <Link
                  href="/auth/login"
                  onClick={closeMenu}
                  className="block text-lg"
                >
                  Login
                </Link>
              )}

              {isLoggedIn && (
                <button
                  type="button"
                  onClick={() => {
                    localStorage.removeItem("token");
                    setIsLoggedIn(false);
                    setIsAdmin(false);
                    closeMenu();
                    window.location.href = "/auth/login";
                  }}
                  className="block text-lg text-left w-full"
                >
                  Logout
                </button>
              )}
            </div>
          )}
        </div>
      </div>
    </>
  );
}

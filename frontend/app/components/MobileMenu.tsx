"use client";

import { useState } from "react";
import Link from "next/link";

export default function MobileMenu() {
  const [open, setOpen] = useState(false);

  return (
    <>
      {/* Hamburger button */}
      <button
        className="md:hidden p-2 rounded border border-gray-300"
        onClick={() => setOpen(!open)}
      >
        <span className="text-xl">☰</span>
      </button>

      {/* Overlay */}
      {open && (
        <div
          className="fixed inset-0 bg-black/40 backdrop-blur-sm z-40"
          onClick={() => setOpen(false)}
        />
      )}

      {/* Slide-in menu */}
      <div
        className={`fixed top-0 right-0 w-64 h-full bg-white shadow-lg z-50 transform transition-transform ${
          open ? "translate-x-0" : "translate-x-full"
        }`}
      >
        <div className="p-6 space-y-4">
          <Link href="/" onClick={() => setOpen(false)} className="block text-lg">
            Home
          </Link>
          <Link href="/products" onClick={() => setOpen(false)} className="block text-lg">
            Products
          </Link>
          <Link href="/wishlist" onClick={() => setOpen(false)} className="block text-lg">
            Wishlist
          </Link>
          <Link href="/orders" onClick={() => setOpen(false)} className="block text-lg">
            Orders
          </Link>
        </div>
      </div>
    </>
  );
}

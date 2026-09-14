"use client";

import Link from "next/link";

export default function Header() {
  return (
    <header className="border-b bg-white">
      <div className="max-w-6xl mx-auto px-4 py-4 flex items-center justify-between">
        <Link href="/" className="text-xl font-bold">
          OnlineShop
        </Link>

        <nav className="flex gap-6 text-sm">
          <Link href="/products">Products</Link>
          <Link href="/cart">Cart</Link>
          <Link href="/wishlist">Wishlist</Link>
          <Link href="/auth/login">Login</Link>
        </nav>
      </div>
    </header>
  );
}

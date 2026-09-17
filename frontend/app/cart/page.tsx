"use client";

import { useEffect, useMemo, useState } from "react";
import Container from "../components/Container";
import {
  api,
  CartResponse,
  Product,
  Coupon,
  getClientToken,
} from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

interface DisplayCartItem {
  id: string;
  productId: string;
  quantity: number;
  product: Product | null;
}

export default function CartPage() {
  const [cart, setCart] = useState<CartResponse | null>(null);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [updatingProductId, setUpdatingProductId] = useState<string | null>(
    null,
  );
  const [error, setError] = useState("");
  const { showSnackbar } = useSnackbar();
  const [couponCode, setCouponCode] = useState("");
  const [appliedCoupon, setAppliedCoupon] = useState<Coupon | null>(null);
  const [applyingCoupon, setApplyingCoupon] = useState(false);

  async function loadCart() {
    const token = getClientToken();

    if (!token) {
      setError("You must be logged in to view your cart.");
      setLoading(false);
      return;
    }

    try {
      setError("");

      const [cartResponse, productResponse] = await Promise.all([
        api.getCart(),
        api.getProducts(),
      ]);

      setCart(cartResponse);
      setProducts(productResponse);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load cart.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadCart();
  }, []);

  const displayItems: DisplayCartItem[] = useMemo(() => {
    if (!cart) return [];

    return cart.items.map((item) => ({
      ...item,
      product:
        products.find((product) => product.id === item.productId) ?? null,
    }));
  }, [cart, products]);

  const totalItems = useMemo(
    () => displayItems.reduce((sum, item) => sum + item.quantity, 0),
    [displayItems],
  );

  const subtotal = useMemo(
    () =>
      displayItems.reduce(
        (sum, item) =>
          sum + (item.product ? Number(item.product.price) * item.quantity : 0),
        0,
      ),
    [displayItems],
  );

  const discount = useMemo(() => {
    if (!appliedCoupon || subtotal <= 0) return 0;

    if (appliedCoupon.type.toLowerCase() === "percentage") {
      return Math.min(subtotal, (subtotal * appliedCoupon.value) / 100);
    }

    if (appliedCoupon.type.toLowerCase() === "fixed") {
      return Math.min(subtotal, appliedCoupon.value);
    }

    return 0;
  }, [appliedCoupon, subtotal]);

  const total = Math.max(0, subtotal - discount);

  async function updateQuantity(productId: string, quantity: number) {
    if (quantity < 1) return;

    try {
      setUpdatingProductId(productId);

      const updatedCart = await api.updateCartItem(productId, quantity);
      setCart(updatedCart);
    } catch (err) {
      showSnackbar(
        err instanceof Error ? err.message : "Failed to update quantity.",
        "error",
      );
    } finally {
      setUpdatingProductId(null);
    }
  }

  async function removeItem(productId: string) {
    try {
      setUpdatingProductId(productId);

      const updatedCart = await api.removeFromCart(productId);
      setCart(updatedCart);
      showSnackbar("Item removed from cart.", "success");
    } catch (err) {
      showSnackbar(
        err instanceof Error ? err.message : "Failed to remove item.",
        "error",
      );
    } finally {
      setUpdatingProductId(null);
    }
  }

  async function clearCart() {
    try {
      await api.clearCart();
      setCart((current) => (current ? { ...current, items: [] } : current));
      showSnackbar("Cart cleared.", "success");
    } catch (err) {
      showSnackbar(
        err instanceof Error ? err.message : "Failed to clear cart.",
        "error",
      );
    }
  }

  async function handleApplyCoupon() {
    const code = couponCode.trim();

    if (!code) {
      showSnackbar("Please enter a coupon code.", "error");
      return;
    }

    try {
      setApplyingCoupon(true);

      const coupon = await api.validateCoupon(code);

      setAppliedCoupon(coupon);
      setCouponCode(coupon.code);

      showSnackbar("Coupon applied successfully!", "success");
    } catch (err) {
      setAppliedCoupon(null);

      showSnackbar(
        err instanceof Error ? err.message : "Invalid or expired coupon.",
        "error",
      );
    } finally {
      setApplyingCoupon(false);
    }
  }

  function handleRemoveCoupon() {
    setAppliedCoupon(null);
    setCouponCode("");
    showSnackbar("Coupon removed.", "success");
  }

  if (loading) {
    return (
      <Container>
        <h1 className="text-2xl font-bold mb-6">Your Cart</h1>
        <p className="text-gray-600">Loading your cart...</p>
      </Container>
    );
  }

  if (error) {
    return (
      <Container>
        <h1 className="text-2xl font-bold mb-6">Your Cart</h1>
        <p className="text-red-600 bg-red-100 px-3 py-2 rounded">{error}</p>
      </Container>
    );
  }

  if (!cart || cart.items.length === 0) {
    return (
      <Container>
        <h1 className="text-2xl font-bold mb-6">Your Cart</h1>

        <div className="border rounded-lg bg-white p-8 text-center">
          <p className="text-gray-600 mb-4">Your cart is empty.</p>

          <a
            href="/products"
            className="inline-block bg-black text-white px-5 py-2 rounded hover:bg-gray-800"
          >
            Continue Shopping
          </a>
        </div>
      </Container>
    );
  }

  async function handleCheckout() {
    try {
      if (!cart || displayItems.length === 0) {
        showSnackbar("Your cart is empty.", "error");
        return;
      }

      // Convert cart items → order items
      const orderItems = displayItems.map((item) => ({
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.product ? Number(item.product.price) : 0,
      }));

      // Create order
      const order = await api.createOrder(orderItems, total);

      // Clear cart
      await api.clearCart();
      setCart((current) => (current ? { ...current, items: [] } : current));

      showSnackbar("Order placed successfully!", "success");

      // Redirect
      window.location.href = "/orders";
    } catch (err) {
      showSnackbar(
        err instanceof Error ? err.message : "Failed to place order.",
        "error",
      );
    }
  }

  return (
    <Container>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Your Cart</h1>
          <p className="text-sm text-gray-500 mt-1">
            {totalItems} {totalItems === 1 ? "item" : "items"}
          </p>
        </div>

        <button
          onClick={clearCart}
          className="border border-red-300 text-red-600 px-4 py-2 rounded hover:bg-red-50"
        >
          Clear Cart
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-4">
          {displayItems.map((item) => {
            const isUpdating = updatingProductId === item.productId;

            return (
              <div
                key={item.id || item.productId}
                className="border rounded-lg bg-white p-4 shadow-sm"
              >
                <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                  <div className="min-w-0">
                    <h2 className="font-semibold text-lg">
                      {item.product?.name || "Product unavailable"}
                    </h2>

                    <p className="text-sm text-gray-500 mt-1">
                      {item.product
                        ? `€${Number(item.product.price).toFixed(2)} each`
                        : "Product details unavailable"}
                    </p>

                    <p className="text-sm text-gray-400 mt-1">
                      Product ID: {item.productId}
                    </p>
                  </div>

                  <div className="flex items-center gap-3">
                    <button
                      onClick={() =>
                        updateQuantity(item.productId, item.quantity - 1)
                      }
                      disabled={isUpdating || item.quantity <= 1}
                      className="w-9 h-9 border rounded hover:bg-gray-100 disabled:opacity-40"
                      aria-label={`Decrease quantity of ${
                        item.product?.name || "product"
                      }`}
                    >
                      −
                    </button>

                    <span className="w-8 text-center font-semibold">
                      {item.quantity}
                    </span>

                    <button
                      onClick={() =>
                        updateQuantity(item.productId, item.quantity + 1)
                      }
                      disabled={isUpdating}
                      className="w-9 h-9 border rounded hover:bg-gray-100 disabled:opacity-40"
                      aria-label={`Increase quantity of ${
                        item.product?.name || "product"
                      }`}
                    >
                      +
                    </button>

                    <button
                      onClick={() => removeItem(item.productId)}
                      disabled={isUpdating}
                      className="ml-2 text-red-600 hover:underline disabled:opacity-40"
                    >
                      Remove
                    </button>
                  </div>
                </div>

                {item.product && (
                  <div className="border-t mt-4 pt-3 text-right font-semibold">
                    €{(Number(item.product.price) * item.quantity).toFixed(2)}
                  </div>
                )}
              </div>
            );
          })}
        </div>

        <aside className="border rounded-lg bg-white p-5 h-fit shadow-sm">
          <h2 className="font-semibold text-lg mb-4">Order Summary</h2>

          <div className="flex justify-between text-sm mb-2">
            <span>Items</span>
            <span>{totalItems}</span>
          </div>

          <div className="border-t pt-3 mt-3 flex justify-between font-bold text-lg">
            <span>Subtotal</span>
            <span>€{subtotal.toFixed(2)}</span>
          </div>

          {appliedCoupon && (
            <div className="flex justify-between text-sm text-green-700">
              <span>Discount</span>
              <span>−€{discount.toFixed(2)}</span>
            </div>
          )}

          <div className="border-t pt-4 mt-4">
            <label
              htmlFor="coupon-code"
              className="block text-sm font-semibold mb-2"
            >
              Coupon code
            </label>

            {appliedCoupon ? (
              <div className="flex items-center justify-between gap-3 rounded-lg border border-green-200 bg-green-50 p-3">
                <div className="min-w-0">
                  <p className="font-semibold text-green-800">
                    {appliedCoupon.code}
                  </p>
                  <p className="text-xs text-green-700 mt-1">
                    {appliedCoupon.type.toLowerCase() === "percentage"
                      ? `${appliedCoupon.value}% discount`
                      : `€${appliedCoupon.value.toFixed(2)} discount`}
                  </p>
                </div>

                <button
                  type="button"
                  onClick={handleRemoveCoupon}
                  className="text-sm font-medium text-red-600 hover:underline"
                >
                  Remove
                </button>
              </div>
            ) : (
              <div className="flex gap-2">
                <input
                  id="coupon-code"
                  type="text"
                  value={couponCode}
                  onChange={(event) => setCouponCode(event.target.value)}
                  placeholder="Enter coupon"
                  disabled={applyingCoupon}
                  className="min-w-0 flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm"
                />

                <button
                  type="button"
                  onClick={handleApplyCoupon}
                  disabled={applyingCoupon || !couponCode.trim()}
                  className="rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-700 disabled:opacity-50"
                >
                  {applyingCoupon ? "Checking..." : "Apply"}
                </button>
              </div>
            )}
          </div>

          {/* TOTAL */}
          <div className="border-t pt-4 mt-4 flex justify-between font-bold text-xl">
            <span>Total</span>
            <span>€{total.toFixed(2)}</span>
          </div>

          {/* CHECKOUT */}
          <button
            onClick={handleCheckout}
            className="mt-5 w-full bg-black text-white py-3 rounded hover:bg-gray-800"
          >
            Place Order
          </button>
        </aside>
      </div>
    </Container>
  );
}

"use client";

import { useEffect, useMemo, useState } from "react";
import Container from "../components/Container";

import CartItemsGrid from "../components/Cart/CartItemsGrid";
import CartSummary from "../components/Cart/CartSummary";
import CouponForm from "../components/Cart/CouponForm";
import CheckoutButton from "../components/Cart/CheckoutButton";
import EmptyCart from "../components/Cart/EmptyCart";

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
  const [updatingProductId, setUpdatingProductId] = useState<string | null>(null);
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
      product: products.find((p) => p.id === item.productId) ?? null,
    }));
  }, [cart, products]);

  const totalItems = useMemo(
    () => displayItems.reduce((sum, item) => sum + item.quantity, 0),
    [displayItems]
  );

  const subtotal = useMemo(
    () =>
      displayItems.reduce(
        (sum, item) =>
          sum + (item.product ? Number(item.product.price) * item.quantity : 0),
        0
      ),
    [displayItems]
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
        "error"
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
        "error"
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
        "error"
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
        "error"
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

  async function handleCheckout() {
    try {
      if (!cart || displayItems.length === 0) {
        showSnackbar("Your cart is empty.", "error");
        return;
      }

      const orderItems = displayItems.map((item) => ({
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.product ? Number(item.product.price) : 0,
      }));

      await api.createOrder(orderItems, total);

      await api.clearCart();
      setCart((current) => (current ? { ...current, items: [] } : current));

      showSnackbar("Order placed successfully!", "success");

      window.location.href = "/orders";
    } catch (err) {
      showSnackbar(
        err instanceof Error ? err.message : "Failed to place order.",
        "error"
      );
    }
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
        <EmptyCart />
      </Container>
    );
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
        {/* Items */}
        <CartItemsGrid
          items={displayItems}
          updatingProductId={updatingProductId}
          onUpdateQuantity={updateQuantity}
          onRemoveItem={removeItem}
        />

        {/* Summary */}
        <aside className="border rounded-lg bg-white p-5 h-fit shadow-sm">
          <h2 className="font-semibold text-lg mb-4">Order Summary</h2>

          <CartSummary
            totalItems={totalItems}
            subtotal={subtotal}
            discount={discount}
            total={total}
          />

          <CouponForm
            couponCode={couponCode}
            appliedCoupon={appliedCoupon}
            applyingCoupon={applyingCoupon}
            onChangeCode={setCouponCode}
            onApply={handleApplyCoupon}
            onRemove={handleRemoveCoupon}
          />

          <CheckoutButton onCheckout={handleCheckout} />
        </aside>
      </div>
    </Container>
  );
}

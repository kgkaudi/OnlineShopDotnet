export const API_URL = process.env.NEXT_PUBLIC_API_URL!;

/**
 * Shared types
 */
export interface Product {
  id: string;
  name: string;
  description?: string | null;
  price: number;
  stockQuantity?: number;
  categoryId?: string | null;
}

export interface CartItem {
  id: string;
  productId: string;
  quantity: number;
}

export interface CartResponse {
  id: string;
  userId: string;
  items: CartItem[];
}

/**
 * Core request wrapper with optional token support
 */
async function request<T>(
  path: string,
  options: RequestInit = {},
  token?: string
): Promise<T> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string> | undefined),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const res = await fetch(`${API_URL}${path}`, {
    ...options,
    headers,
    cache: "no-store",
  });

  if (!res.ok) {
    const message = await res.text();
    throw new Error(message || `Request failed: ${res.status}`);
  }

  return res.json() as T;
}

/**
 * Client-side token helper
 */
export function getClientToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("token");
}

/**
 * Logout helper
 */
export function logout() {
  if (typeof window !== "undefined") {
    localStorage.removeItem("token");
  }
}

/**
 * API endpoints
 */
export const api = {
  // AUTH
  login: (data: { email: string; password: string }) =>
    request<{ token: string }>("/auth/login", {
      method: "POST",
      body: JSON.stringify(data),
    }),

  // PRODUCTS
  getProducts: () => request<Product[]>("/products"),
  getProduct: (id: string) => request<Product>(`/products/${id}`),

  // CART
  getCart: () =>
    request<CartResponse>("/cart", {}, getClientToken() || undefined),

  addToCart: (productId: string, quantity = 1) => {
    const params = new URLSearchParams({
      productId,
      quantity: String(quantity),
    });

    return request<CartResponse>(
      `/cart/add?${params.toString()}`,
      {
        method: "POST",
      },
      getClientToken() || undefined
    );
  },

  updateCartItem: (productId: string, quantity: number) => {
    const params = new URLSearchParams({
      productId,
      quantity: String(quantity),
    });

    return request<CartResponse>(
      `/cart/update?${params.toString()}`,
      {
        method: "PUT",
      },
      getClientToken() || undefined
    );
  },

  removeFromCart: (productId: string) => {
    const params = new URLSearchParams({ productId });

    return request<CartResponse>(
      `/cart/remove?${params.toString()}`,
      {
        method: "DELETE",
      },
      getClientToken() || undefined
    );
  },

  clearCart: () =>
    request<{ message: string }>(
      "/cart/clear",
      {
        method: "DELETE",
      },
      getClientToken() || undefined
    ),

  // WISHLIST
  getWishlist: () =>
    request<Product[]>("/wishlist", {}, getClientToken() || undefined),

  addToWishlist: (productId: string) =>
    request<{ message: string }>(
      "/wishlist/add",
      {
        method: "POST",
        body: JSON.stringify({ productId }),
      },
      getClientToken() || undefined
    ),
};

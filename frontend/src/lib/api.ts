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

export interface Category {
  id: string;
  name: string;
}

export interface CreateProductRequest {
  name: string;
  description?: string | null;
  price: number;
  stockQuantity: number;
  categoryId?: string | null;
}

export interface UpdateProductRequest {
  name: string;
  description?: string | null;
  price: number;
  stockQuantity: number;
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

export interface Address {
  street?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  country?: string | null;
}

export interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  roles: string[];

  phoneNumber?: string | null;
  shippingAddress?: Address | null;
  billingAddress?: Address | null;

  createdAt?: string;
  updatedAt?: string;
  isEmailVerified?: boolean;
}

/**
 * Admin user has the same safe fields as UserProfile.
 */
export type AdminUser = UserProfile;

export interface UpdateRolesRequest {
  roles: string[];
}

export interface AddRoleResponse {
  message: string;
}

export interface WishlistItem {
  productId: string;
  productName: string;
  productDescription: string;
  productPrice: number;
  addedAt: string;
}

/**
 * Core request wrapper
 */
async function request<T>(
  path: string,
  options: RequestInit = {},
  token?: string,
): Promise<T> {
  const headers = new Headers(options.headers);

  headers.set("Content-Type", "application/json");

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers,
    cache: "no-store",
  });

  if (!response.ok) {
    const message = await response.text();

    throw new Error(
      message || `Request failed with status ${response.status}`,
    );
  }

  /*
   * Some successful endpoints may return an empty response.
   */
  const contentType = response.headers.get("content-type");

  if (!contentType?.includes("application/json")) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

/**
 * Client-side token helper
 */
export function getClientToken(): string | null {
  if (typeof window === "undefined") {
    return null;
  }

  return localStorage.getItem("token");
}

/**
 * Decode the JWT payload.
 *
 * This does NOT validate the token.
 * Token validation is performed by the ASP.NET API.
 */
function getTokenPayload(): Record<string, unknown> | null {
  const token = getClientToken();

  if (!token) {
    return null;
  }

  try {
    const parts = token.split(".");

    if (parts.length !== 3) {
      return null;
    }

    const payloadPart = parts[1];

    if (!payloadPart) {
      return null;
    }

    const base64 = payloadPart
      .replace(/-/g, "+")
      .replace(/_/g, "/");

    const padded = base64.padEnd(
      Math.ceil(base64.length / 4) * 4,
      "=",
    );

    const payload = JSON.parse(atob(padded));

    if (!payload || typeof payload !== "object") {
      return null;
    }

    return payload as Record<string, unknown>;
  } catch {
    return null;
  }
}

/**
 * Get the currently authenticated user's ID from the JWT.
 *
 * Supports the claims used by the ASP.NET backend:
 * - sub
 * - NameIdentifier
 * - nameid
 * - id
 * - userId
 */
export function getClientUserId(): string | null {
  const payload = getTokenPayload();

  if (!payload) {
    return null;
  }

  const possibleIds = [
    payload.sub,
    payload[
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
    ],
    payload.nameid,
    payload.id,
    payload.userId,
  ];

  for (const value of possibleIds) {
    if (typeof value === "string" && value.trim()) {
      return value.trim();
    }
  }

  return null;
}

/**
 * Get roles from the JWT.
 *
 * This is useful for frontend UI decisions only.
 * The backend remains responsible for authorization.
 */
export function getClientRoles(): string[] {
  const payload = getTokenPayload();

  if (!payload) {
    return [];
  }

  const roleClaim =
    payload.role ??
    payload.roles ??
    payload[
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ];

  if (typeof roleClaim === "string") {
    return [roleClaim.trim()].filter(Boolean);
  }

  if (Array.isArray(roleClaim)) {
    return roleClaim
      .filter(
        (role): role is string =>
          typeof role === "string",
      )
      .map((role) => role.trim())
      .filter(Boolean);
  }

  return [];
}

/**
 * Check whether the current JWT contains the Admin role.
 *
 * Frontend UI visibility only.
 * The API must still enforce authorization.
 */
export function clientIsAdmin(): boolean {
  return getClientRoles().some(
    (role) => role.toLowerCase() === "admin",
  );
}

/**
 * Logout helper
 */
export function logout(): void {
  if (typeof window !== "undefined") {
    localStorage.removeItem("token");
  }
}

/**
 * API endpoints
 */
export const api = {
  // =========================================================
  // AUTH
  // =========================================================

  login: (data: {
    email: string;
    password: string;
  }) =>
    request<{ token: string; expires?: string }>(
      "/auth/login",
      {
        method: "POST",
        body: JSON.stringify(data),
      },
    ),

  // =========================================================
  // PROFILE
  // =========================================================

  getUserProfile: (id: string) =>
    request<UserProfile>(
      `/users/${encodeURIComponent(id)}`,
      {},
      getClientToken() || undefined,
    ),

  updateUserProfile: (
    id: string,
    data: {
      fullName: string;
      email: string;
      phoneNumber?: string | null;
      shippingAddress?: Address | null;
      billingAddress?: Address | null;
    },
  ) =>
    request<UserProfile>(
      `/users/${encodeURIComponent(id)}`,
      {
        method: "PUT",
        body: JSON.stringify(data),
      },
      getClientToken() || undefined,
    ),

  // =========================================================
  // PRODUCTS - PUBLIC
  // =========================================================

  getProducts: () =>
    request<Product[]>("/products"),

  getProduct: (id: string) =>
    request<Product>(
      `/products/${encodeURIComponent(id)}`,
    ),

  // =========================================================
  // PRODUCTS - ADMIN
  // =========================================================

  createProduct: (data: CreateProductRequest) =>
    request<Product>(
      "/products",
      {
        method: "POST",
        body: JSON.stringify(data),
      },
      getClientToken() || undefined,
    ),

  updateProduct: (
    id: string,
    data: UpdateProductRequest,
  ) =>
    request<Product>(
      `/products/${encodeURIComponent(id)}`,
      {
        method: "PUT",
        body: JSON.stringify(data),
      },
      getClientToken() || undefined,
    ),

  deleteProduct: (id: string) =>
    request<{ message: string }>(
      `/products/${encodeURIComponent(id)}`,
      {
        method: "DELETE",
      },
      getClientToken() || undefined,
    ),

  // =========================================================
  // CATEGORIES
  // =========================================================

  getCategories: () =>
    request<Category[]>("/categories"),

  getCategory: (id: string) =>
    request<Category>(
      `/categories/${encodeURIComponent(id)}`,
    ),

  // =========================================================
  // CART
  // =========================================================

  getCart: () =>
    request<CartResponse>(
      "/cart",
      {},
      getClientToken() || undefined,
    ),

  addToCart: (
    productId: string,
    quantity = 1,
  ) => {
    const params = new URLSearchParams({
      productId,
      quantity: String(quantity),
    });

    return request<CartResponse>(
      `/cart/add?${params.toString()}`,
      {
        method: "POST",
      },
      getClientToken() || undefined,
    );
  },

  updateCartItem: (
    productId: string,
    quantity: number,
  ) => {
    const params = new URLSearchParams({
      productId,
      quantity: String(quantity),
    });

    return request<CartResponse>(
      `/cart/update?${params.toString()}`,
      {
        method: "PUT",
      },
      getClientToken() || undefined,
    );
  },

  removeFromCart: (productId: string) => {
    const params = new URLSearchParams({
      productId,
    });

    return request<CartResponse>(
      `/cart/remove?${params.toString()}`,
      {
        method: "DELETE",
      },
      getClientToken() || undefined,
    );
  },

  clearCart: () =>
    request<{ message: string }>(
      "/cart/clear",
      {
        method: "DELETE",
      },
      getClientToken() || undefined,
    ),

  // =========================================================
  // WISHLIST
  // =========================================================

  getWishlist: () =>
    request<WishlistItem[]>(
      "/wishlist",
      {},
      getClientToken() || undefined,
    ),

  addToWishlist: (productId: string) =>
    request<{ message: string }>(
      "/wishlist/add",
      {
        method: "POST",
        body: JSON.stringify({
          productId,
        }),
      },
      getClientToken() || undefined,
    ),

  removeFromWishlist: (productId: string) =>
    request<{ message: string }>(
      "/wishlist/remove",
      {
        method: "DELETE",
        body: JSON.stringify({
          productId,
        }),
      },
      getClientToken() || undefined,
    ),

  // =========================================================
  // ADMIN - USERS
  // =========================================================

  getUsers: () =>
    request<AdminUser[]>(
      "/users",
      {},
      getClientToken() || undefined,
    ),

  getUserById: (id: string) =>
    request<AdminUser>(
      `/users/${encodeURIComponent(id)}`,
      {},
      getClientToken() || undefined,
    ),

  updateUserRoles: (
    id: string,
    roles: string[],
  ) =>
    request<AdminUser>(
      `/users/${encodeURIComponent(id)}/roles`,
      {
        method: "PUT",
        body: JSON.stringify({
          roles,
        } satisfies UpdateRolesRequest),
      },
      getClientToken() || undefined,
    ),

  addUserRole: (
    id: string,
    role: string,
  ) =>
    request<AddRoleResponse>(
      `/users/${encodeURIComponent(id)}/roles`,
      {
        method: "POST",
        body: JSON.stringify({
          role,
        }),
      },
      getClientToken() || undefined,
    ),

  deleteUser: (id: string) =>
    request<{ message: string }>(
      `/users/${encodeURIComponent(id)}`,
      {
        method: "DELETE",
      },
      getClientToken() || undefined,
    ),
};
import { API_URL } from "../lib/api";

/**
 * Register a new user
 */
export async function register(data: {
  fullName: string;
  email: string;
  password: string;
}) {
  const res = await fetch(`${API_URL}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!res.ok) {
    const message = await res.text();
    throw new Error(message || `Registration failed: ${res.status}`);
  }

  return res.json();
}

/**
 * Login user
 */
export async function login(data: { email: string; password: string }) {
  const res = await fetch(`${API_URL}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!res.ok) {
    const message = await res.text();
    throw new Error(message || `Login failed: ${res.status}`);
  }

  return res.json();
}

/**
 * Logout user (backend + frontend)
 */
export async function logout() {
  const token = localStorage.getItem("token");
  if (!token) return;

  // Call backend logout to invalidate token
  await fetch(`${API_URL}/auth/logout`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
  });

  // Remove token locally
  localStorage.removeItem("token");
}

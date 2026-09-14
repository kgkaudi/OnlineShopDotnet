export const API_URL = process.env.NEXT_PUBLIC_API_URL!;

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {}),
    },
    cache: "no-store",
  });

  if (!res.ok) throw new Error(await res.text());
  return res.json();
}

export const api = {
  getProducts: () => request("/products"),
  getProduct: (id: string) => request(`/products/${id}`),
  login: (data: { email: string; password: string }) =>
    request("/auth/login", { method: "POST", body: JSON.stringify(data) }),
};

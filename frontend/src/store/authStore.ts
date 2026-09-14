import { create } from "zustand";

interface AuthState {
  isLoggedIn: boolean;
  setLoggedIn: (value: boolean) => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  // Initialize from localStorage safely (only in browser)
  isLoggedIn: typeof window !== "undefined" && !!localStorage.getItem("token"),

  setLoggedIn: (value) => set({ isLoggedIn: value }),
}));

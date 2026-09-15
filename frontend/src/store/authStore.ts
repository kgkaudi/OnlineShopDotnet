import { create } from "zustand";

interface AuthState {
  isLoggedIn: boolean;
  isAdmin: boolean;

  setLoggedIn: (value: boolean) => void;
  setAdmin: (value: boolean) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  isLoggedIn:
    typeof window !== "undefined" &&
    !!localStorage.getItem("token"),

  isAdmin: false,

  setLoggedIn: (value) =>
    set({
      isLoggedIn: value,
    }),

  setAdmin: (value) =>
    set({
      isAdmin: value,
    }),

  logout: () =>
    set({
      isLoggedIn: false,
      isAdmin: false,
    }),
}));
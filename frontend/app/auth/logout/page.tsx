"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { logout } from "@/src/lib/api";

export default function LogoutPage() {
  const router = useRouter();

  useEffect(() => {
    logout();
    router.push("/auth/login");
  }, [router]);

  return (
    <div className="flex items-center justify-center min-h-screen">
      <p className="text-gray-600">Logging out...</p>
    </div>
  );
}

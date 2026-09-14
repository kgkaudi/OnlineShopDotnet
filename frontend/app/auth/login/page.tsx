"use client";

import Container from "../../components/Container";

export default function LoginPage() {
  return (
    <Container>
      <h1 className="text-2xl font-bold mb-6">Login</h1>

      <form className="max-w-sm space-y-4">
        <input
          type="email"
          placeholder="Email"
          className="w-full border px-3 py-2 rounded"
        />
        <input
          type="password"
          placeholder="Password"
          className="w-full border px-3 py-2 rounded"
        />
        <button className="w-full bg-black text-white py-2 rounded">
          Login
        </button>
      </form>
    </Container>
  );
}

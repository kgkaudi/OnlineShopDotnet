"use client";

import Link from "next/link";
import { useEffect, useState, type FormEvent } from "react";
import Container from "../components/Container";
import { api, getClientUserId, type UserProfile } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

export default function ProfilePage() {
  const { showSnackbar } = useSnackbar();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    const token = localStorage.getItem("token");
    if (!token) {
      window.location.href = "/auth/login";
      return;
    }

    const userId = getClientUserId();
    if (!userId) {
      setError("Invalid user token.");
      setLoading(false);
      return;
    }

    api.getUserProfile(userId)
      .then((data) => {
        setProfile(data);
        setFullName(data.fullName || "");
        setEmail(data.email || "");
      })
      .catch((err) => setError(err instanceof Error ? err.message : "Failed to load profile."))
      .finally(() => setLoading(false));
  }, []);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!fullName.trim()) return showSnackbar("Full name is required.", "error");
    if (!email.trim() || !email.includes("@")) return showSnackbar("Please enter a valid email address.", "error");

    try {
      setSaving(true);
      const userId = getClientUserId();
      if (!userId) {
        showSnackbar("Invalid user token.", "error");
        return;
      }

      const updated = await api.updateUserProfile(userId, {
        fullName: fullName.trim(),
        email: email.trim(),
      });
      setProfile(updated);
      setFullName(updated.fullName || "");
      setEmail(updated.email || "");
      showSnackbar("Profile updated successfully.", "success");
    } catch (err) {
      console.error(err);
      showSnackbar(err instanceof Error ? err.message : "Something went wrong while updating your profile.", "error");
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return <Container><div className="py-12 text-center text-gray-600">Loading profile...</div></Container>;
  }

  return (
    <Container>
      <div className="max-w-2xl mx-auto py-6 sm:py-10">
        <div className="mb-8">
          <Link href="/products" className="text-sm text-gray-600 hover:text-black">← Back to Products</Link>
          <h1 className="text-3xl font-bold mt-4">My Profile</h1>
          <p className="text-gray-600 mt-2">Update your personal details.</p>
        </div>

        {error && <div className="mb-6 rounded-lg bg-red-100 text-red-700 p-4">{error}</div>}

        {profile && (
          <form onSubmit={handleSubmit} className="bg-white border rounded-2xl shadow-sm p-6 sm:p-8">
            <div className="space-y-6">
              <div>
                <label htmlFor="fullName" className="block text-sm font-semibold mb-2">Full name</label>
                <input id="fullName" type="text" value={fullName} onChange={(event) => setFullName(event.target.value)} className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black" autoComplete="name" disabled={saving} />
              </div>

              <div>
                <label htmlFor="email" className="block text-sm font-semibold mb-2">Email address</label>
                <input id="email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black" autoComplete="email" disabled={saving} />
              </div>

              <div>
                <label className="block text-sm font-semibold mb-2">Roles</label>
                <div className="flex flex-wrap gap-2">
                  {profile.roles.map((role) => <span key={role} className="bg-gray-100 text-gray-700 px-3 py-1.5 rounded-full text-sm">{role}</span>)}
                </div>
                <p className="text-xs text-gray-500 mt-2">Roles can only be changed by an administrator.</p>
              </div>

              <button type="submit" disabled={saving} className="w-full bg-black text-white py-3 rounded-lg font-semibold hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed transition">
                {saving ? "Saving..." : "Save Changes"}
              </button>
            </div>
          </form>
        )}
      </div>
    </Container>
  );
}

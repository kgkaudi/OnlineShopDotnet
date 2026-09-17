"use client";

import { useEffect, useState, FormEvent } from "react";
import Container from "../components/Container";

import ProfileHeader from "../components/Profile/ProfileHeader";
import ProfileError from "../components/Profile/ProfileError";
import LoadingProfile from "../components/Profile/LoadingProfile";
import ProfileForm from "../components/Profile/ProfileForm";

import { api, getClientUserId, UserProfile } from "@/src/lib/api";
import { useSnackbar } from "@/src/context/SnackbarContext";

type Address = {
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
};

const emptyAddress: Address = {
  street: "",
  city: "",
  state: "",
  postalCode: "",
  country: "",
};

export default function ProfilePage() {
  const { showSnackbar } = useSnackbar();

  const [profile, setProfile] = useState<UserProfile | null>(null);

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");

  const [shippingAddress, setShippingAddress] = useState<Address>(emptyAddress);
  const [billingAddress, setBillingAddress] = useState<Address>(emptyAddress);

  const [sameBillingAddress, setSameBillingAddress] = useState(false);

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

    api
      .getUserProfile(userId)
      .then((data) => {
        setProfile(data);

        setFullName(data.fullName || "");
        setEmail(data.email || "");
        setPhoneNumber(data.phoneNumber || "");

        setShippingAddress({
          street: data.shippingAddress?.street || "",
          city: data.shippingAddress?.city || "",
          state: data.shippingAddress?.state || "",
          postalCode: data.shippingAddress?.postalCode || "",
          country: data.shippingAddress?.country || "",
        });

        setBillingAddress({
          street: data.billingAddress?.street || "",
          city: data.billingAddress?.city || "",
          state: data.billingAddress?.state || "",
          postalCode: data.billingAddress?.postalCode || "",
          country: data.billingAddress?.country || "",
        });
      })
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Failed to load profile.")
      )
      .finally(() => setLoading(false));
  }, []);

  function updateShippingAddress(field: keyof Address, value: string) {
    setShippingAddress((prev) => ({ ...prev, [field]: value }));
  }

  function updateBillingAddress(field: keyof Address, value: string) {
    setBillingAddress((prev) => ({ ...prev, [field]: value }));
  }

  function handleSameBillingAddressChange(checked: boolean) {
    setSameBillingAddress(checked);
    if (checked) setBillingAddress({ ...shippingAddress });
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!fullName.trim()) {
      showSnackbar("Full name is required.", "error");
      return;
    }

    if (!email.trim() || !email.includes("@")) {
      showSnackbar("Please enter a valid email address.", "error");
      return;
    }

    try {
      setSaving(true);

      const userId = getClientUserId();
      if (!userId) {
        showSnackbar("Invalid user token.", "error");
        return;
      }

      const finalBillingAddress = sameBillingAddress
        ? shippingAddress
        : billingAddress;

      const updated = await api.updateUserProfile(userId, {
        fullName: fullName.trim(),
        email: email.trim(),
        phoneNumber: phoneNumber.trim() || null,

        shippingAddress: {
          street: shippingAddress.street.trim(),
          city: shippingAddress.city.trim(),
          state: shippingAddress.state.trim(),
          postalCode: shippingAddress.postalCode.trim(),
          country: shippingAddress.country.trim(),
        },

        billingAddress: {
          street: finalBillingAddress.street.trim(),
          city: finalBillingAddress.city.trim(),
          state: finalBillingAddress.state.trim(),
          postalCode: finalBillingAddress.postalCode.trim(),
          country: finalBillingAddress.country.trim(),
        },
      });

      setProfile(updated);

      setFullName(updated.fullName || "");
      setEmail(updated.email || "");
      setPhoneNumber(updated.phoneNumber || "");

      setShippingAddress({
        street: updated.shippingAddress?.street || "",
        city: updated.shippingAddress?.city || "",
        state: updated.shippingAddress?.state || "",
        postalCode: updated.shippingAddress?.postalCode || "",
        country: updated.shippingAddress?.country || "",
      });

      setBillingAddress({
        street: updated.billingAddress?.street || "",
        city: updated.billingAddress?.city || "",
        state: updated.billingAddress?.state || "",
        postalCode: updated.billingAddress?.postalCode || "",
        country: updated.billingAddress?.country || "",
      });

      showSnackbar("Profile updated successfully.", "success");
    } catch (err) {
      showSnackbar(
        err instanceof Error
          ? err.message
          : "Something went wrong while updating your profile.",
        "error"
      );
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <Container>
        <LoadingProfile />
      </Container>
    );
  }

  return (
    <Container>
      <div className="max-w-3xl mx-auto py-6 sm:py-10">
        <ProfileHeader />

        {error && <ProfileError message={error} />}

        {profile && (
          <ProfileForm
            profile={profile}
            fullName={fullName}
            email={email}
            phoneNumber={phoneNumber}
            shippingAddress={shippingAddress}
            billingAddress={billingAddress}
            sameBillingAddress={sameBillingAddress}
            saving={saving}
            setFullName={setFullName}
            setEmail={setEmail}
            setPhoneNumber={setPhoneNumber}
            updateShippingAddress={updateShippingAddress}
            updateBillingAddress={updateBillingAddress}
            handleSameBillingAddressChange={handleSameBillingAddressChange}
            onSubmit={handleSubmit}
          />
        )}
      </div>
    </Container>
  );
}

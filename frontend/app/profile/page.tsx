"use client";

import Link from "next/link";
import { useEffect, useState, type FormEvent } from "react";
import Container from "../components/Container";
import {
  api,
  getClientUserId,
  type UserProfile,
} from "@/src/lib/api";
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

  const [shippingAddress, setShippingAddress] =
    useState<Address>(emptyAddress);

  const [billingAddress, setBillingAddress] =
    useState<Address>(emptyAddress);

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
        setError(
          err instanceof Error
            ? err.message
            : "Failed to load profile."
        )
      )
      .finally(() => setLoading(false));
  }, []);

  function updateShippingAddress(
    field: keyof Address,
    value: string
  ) {
    setShippingAddress((previous) => ({
      ...previous,
      [field]: value,
    }));
  }

  function updateBillingAddress(
    field: keyof Address,
    value: string
  ) {
    setBillingAddress((previous) => ({
      ...previous,
      [field]: value,
    }));
  }

  function handleSameBillingAddressChange(
    checked: boolean
  ) {
    setSameBillingAddress(checked);

    if (checked) {
      setBillingAddress({
        ...shippingAddress,
      });
    }
  }

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    if (!fullName.trim()) {
      showSnackbar("Full name is required.", "error");
      return;
    }

    if (!email.trim() || !email.includes("@")) {
      showSnackbar(
        "Please enter a valid email address.",
        "error"
      );
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

      showSnackbar(
        "Profile updated successfully.",
        "success"
      );
    } catch (err) {
      console.error(err);

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
        <div className="py-12 text-center text-gray-600">
          Loading profile...
        </div>
      </Container>
    );
  }

  return (
    <Container>
      <div className="max-w-3xl mx-auto py-6 sm:py-10">
        <div className="mb-8">
          <Link
            href="/products"
            className="text-sm text-gray-600 hover:text-black"
          >
            ← Back to Products
          </Link>

          <h1 className="text-3xl font-bold mt-4">
            My Profile
          </h1>

          <p className="text-gray-600 mt-2">
            Update your personal and delivery details.
          </p>
        </div>

        {error && (
          <div className="mb-6 rounded-lg bg-red-100 text-red-700 p-4">
            {error}
          </div>
        )}

        {profile && (
          <form
            onSubmit={handleSubmit}
            className="bg-white border rounded-2xl shadow-sm p-6 sm:p-8"
          >
            <div className="space-y-8">

              {/* =================================================
                  PERSONAL INFORMATION
              ================================================= */}

              <section>
                <h2 className="text-lg font-semibold mb-4">
                  Personal Information
                </h2>

                <div className="space-y-5">
                  <div>
                    <label
                      htmlFor="fullName"
                      className="block text-sm font-semibold mb-2"
                    >
                      Full name
                    </label>

                    <input
                      id="fullName"
                      type="text"
                      value={fullName}
                      onChange={(event) =>
                        setFullName(event.target.value)
                      }
                      className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                      autoComplete="name"
                      disabled={saving}
                    />
                  </div>

                  <div>
                    <label
                      htmlFor="email"
                      className="block text-sm font-semibold mb-2"
                    >
                      Email address
                    </label>

                    <input
                      id="email"
                      type="email"
                      value={email}
                      onChange={(event) =>
                        setEmail(event.target.value)
                      }
                      className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                      autoComplete="email"
                      disabled={saving}
                    />
                  </div>

                  <div>
                    <label
                      htmlFor="phoneNumber"
                      className="block text-sm font-semibold mb-2"
                    >
                      Phone number
                    </label>

                    <input
                      id="phoneNumber"
                      type="tel"
                      value={phoneNumber}
                      onChange={(event) =>
                        setPhoneNumber(event.target.value)
                      }
                      className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                      autoComplete="tel"
                      disabled={saving}
                    />
                  </div>
                </div>
              </section>

              {/* =================================================
                  SHIPPING ADDRESS
              ================================================= */}

              <section>
                <h2 className="text-lg font-semibold mb-4">
                  Shipping Address
                </h2>

                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-semibold mb-2">
                      Street address
                    </label>

                    <input
                      type="text"
                      value={shippingAddress.street}
                      onChange={(event) =>
                        updateShippingAddress(
                          "street",
                          event.target.value
                        )
                      }
                      className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                      disabled={saving}
                    />
                  </div>

                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-semibold mb-2">
                        City
                      </label>

                      <input
                        type="text"
                        value={shippingAddress.city}
                        onChange={(event) =>
                          updateShippingAddress(
                            "city",
                            event.target.value
                          )
                        }
                        className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                        disabled={saving}
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-semibold mb-2">
                        State / Province
                      </label>

                      <input
                        type="text"
                        value={shippingAddress.state}
                        onChange={(event) =>
                          updateShippingAddress(
                            "state",
                            event.target.value
                          )
                        }
                        className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                        disabled={saving}
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-semibold mb-2">
                        Postal code
                      </label>

                      <input
                        type="text"
                        value={shippingAddress.postalCode}
                        onChange={(event) =>
                          updateShippingAddress(
                            "postalCode",
                            event.target.value
                          )
                        }
                        className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                        disabled={saving}
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-semibold mb-2">
                        Country
                      </label>

                      <input
                        type="text"
                        value={shippingAddress.country}
                        onChange={(event) =>
                          updateShippingAddress(
                            "country",
                            event.target.value
                          )
                        }
                        className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                        disabled={saving}
                      />
                    </div>
                  </div>
                </div>
              </section>

              {/* =================================================
                  BILLING ADDRESS
              ================================================= */}

              <section>
                <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-4">
                  <h2 className="text-lg font-semibold">
                    Billing Address
                  </h2>

                  <label className="flex items-center gap-2 text-sm cursor-pointer">
                    <input
                      type="checkbox"
                      checked={sameBillingAddress}
                      onChange={(event) =>
                        handleSameBillingAddressChange(
                          event.target.checked
                        )
                      }
                      disabled={saving}
                    />

                    Same as shipping
                  </label>
                </div>

                {!sameBillingAddress && (
                  <div className="space-y-4">
                    <div>
                      <label className="block text-sm font-semibold mb-2">
                        Street address
                      </label>

                      <input
                        type="text"
                        value={billingAddress.street}
                        onChange={(event) =>
                          updateBillingAddress(
                            "street",
                            event.target.value
                          )
                        }
                        className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                        disabled={saving}
                      />
                    </div>

                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-semibold mb-2">
                          City
                        </label>

                        <input
                          type="text"
                          value={billingAddress.city}
                          onChange={(event) =>
                            updateBillingAddress(
                              "city",
                              event.target.value
                            )
                          }
                          className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                          disabled={saving}
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-semibold mb-2">
                          State / Province
                        </label>

                        <input
                          type="text"
                          value={billingAddress.state}
                          onChange={(event) =>
                            updateBillingAddress(
                              "state",
                              event.target.value
                            )
                          }
                          className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                          disabled={saving}
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-semibold mb-2">
                          Postal code
                        </label>

                        <input
                          type="text"
                          value={billingAddress.postalCode}
                          onChange={(event) =>
                            updateBillingAddress(
                              "postalCode",
                              event.target.value
                            )
                          }
                          className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                          disabled={saving}
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-semibold mb-2">
                          Country
                        </label>

                        <input
                          type="text"
                          value={billingAddress.country}
                          onChange={(event) =>
                            updateBillingAddress(
                              "country",
                              event.target.value
                            )
                          }
                          className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
                          disabled={saving}
                        />
                      </div>
                    </div>
                  </div>
                )}
              </section>

              {/* =================================================
                  ROLES
              ================================================= */}

              <section>
                <label className="block text-sm font-semibold mb-2">
                  Roles
                </label>

                <div className="flex flex-wrap gap-2">
                  {profile.roles.map((role) => (
                    <span
                      key={role}
                      className="bg-gray-100 text-gray-700 px-3 py-1.5 rounded-full text-sm"
                    >
                      {role}
                    </span>
                  ))}
                </div>

                <p className="text-xs text-gray-500 mt-2">
                  Roles can only be changed by an administrator.
                </p>
              </section>

              {/* =================================================
                  SAVE
              ================================================= */}

              <button
                type="submit"
                disabled={saving}
                className="w-full bg-black text-white py-3 rounded-lg font-semibold hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed transition"
              >
                {saving ? "Saving..." : "Save Changes"}
              </button>
            </div>
          </form>
        )}
      </div>
    </Container>
  );
}
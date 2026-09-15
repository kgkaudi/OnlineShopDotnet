"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Container from "../../components/Container";

export default function RegisterPage() {
  const router = useRouter();

  const [form, setForm] = useState({
    fullName: "",
    email: "",
    password: "",

    phoneNumber: "",

    shippingAddress: {
      street: "",
      city: "",
      state: "",
      postalCode: "",
      country: "",
    },

    billingAddress: {
      street: "",
      city: "",
      state: "",
      postalCode: "",
      country: "",
    },
  });

  const [sameBillingAddress, setSameBillingAddress] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const { name, value } = e.target;

    setForm((previous) => ({
      ...previous,
      [name]: value,
    }));
  };

  const handleAddressChange = (
    type: "shippingAddress" | "billingAddress",
    field: keyof typeof form.shippingAddress,
    value: string
  ) => {
    setForm((previous) => ({
      ...previous,
      [type]: {
        ...previous[type],
        [field]: value,
      },
    }));
  };

  const handleSameBillingChange = (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const checked = e.target.checked;

    setSameBillingAddress(checked);

    if (checked) {
      setForm((previous) => ({
        ...previous,
        billingAddress: {
          ...previous.shippingAddress,
        },
      }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const billingAddress = sameBillingAddress
        ? form.shippingAddress
        : form.billingAddress;

      const payload = {
        fullName: form.fullName,
        email: form.email,
        password: form.password,

        phoneNumber: form.phoneNumber.trim() || null,

        shippingAddress: {
          street: form.shippingAddress.street.trim(),
          city: form.shippingAddress.city.trim(),
          state: form.shippingAddress.state.trim(),
          postalCode: form.shippingAddress.postalCode.trim(),
          country: form.shippingAddress.country.trim(),
        },

        billingAddress: {
          street: billingAddress.street.trim(),
          city: billingAddress.city.trim(),
          state: billingAddress.state.trim(),
          postalCode: billingAddress.postalCode.trim(),
          country: billingAddress.country.trim(),
        },
      };

      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/auth/register`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(payload),
        }
      );

      if (!res.ok) {
        const msg = await res.text();
        throw new Error(msg);
      }

      router.push("/auth/login");
    } catch (err: any) {
      setError(err.message || "Registration failed.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container>
      <div className="flex flex-col items-center justify-center min-h-screen bg-gray-50 py-10">
        <div className="w-full max-w-2xl bg-white p-8 rounded shadow">
          <h1 className="text-2xl font-semibold mb-6 text-center">
            Create Account
          </h1>

          <form onSubmit={handleSubmit} className="space-y-6">
            {error && (
              <div className="bg-red-100 text-red-700 px-3 py-2 rounded text-center">
                {error}
              </div>
            )}

            {/* =====================================================
                PERSONAL INFORMATION
            ====================================================== */}

            <div>
              <h2 className="text-lg font-semibold mb-3">
                Personal Information
              </h2>

              <div className="space-y-4">
                <input
                  type="text"
                  name="fullName"
                  placeholder="Full Name"
                  value={form.fullName}
                  onChange={handleChange}
                  className="w-full border border-gray-300 rounded px-3 py-2"
                  required
                />

                <input
                  type="email"
                  name="email"
                  placeholder="Email"
                  value={form.email}
                  onChange={handleChange}
                  className="w-full border border-gray-300 rounded px-3 py-2"
                  required
                />

                <input
                  type="tel"
                  name="phoneNumber"
                  placeholder="Phone Number"
                  value={form.phoneNumber}
                  onChange={handleChange}
                  className="w-full border border-gray-300 rounded px-3 py-2"
                />

                <div className="relative">
                  <input
                    type={showPassword ? "text" : "password"}
                    name="password"
                    placeholder="Password"
                    value={form.password}
                    onChange={handleChange}
                    className="w-full border border-gray-300 rounded px-3 py-2 pr-10"
                    required
                  />

                  <button
                    type="button"
                    onClick={() =>
                      setShowPassword(!showPassword)
                    }
                    className="absolute right-3 top-2.5 text-gray-500 hover:text-black"
                  >
                    {showPassword ? "🙈" : "👁️"}
                  </button>
                </div>
              </div>
            </div>

            {/* =====================================================
                SHIPPING ADDRESS
            ====================================================== */}

            <div>
              <h2 className="text-lg font-semibold mb-3">
                Shipping Address
              </h2>

              <div className="space-y-4">
                <input
                  type="text"
                  placeholder="Street Address"
                  value={form.shippingAddress.street}
                  onChange={(e) =>
                    handleAddressChange(
                      "shippingAddress",
                      "street",
                      e.target.value
                    )
                  }
                  className="w-full border border-gray-300 rounded px-3 py-2"
                  required
                />

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <input
                    type="text"
                    placeholder="City"
                    value={form.shippingAddress.city}
                    onChange={(e) =>
                      handleAddressChange(
                        "shippingAddress",
                        "city",
                        e.target.value
                      )
                    }
                    className="w-full border border-gray-300 rounded px-3 py-2"
                    required
                  />

                  <input
                    type="text"
                    placeholder="State / Province"
                    value={form.shippingAddress.state}
                    onChange={(e) =>
                      handleAddressChange(
                        "shippingAddress",
                        "state",
                        e.target.value
                      )
                    }
                    className="w-full border border-gray-300 rounded px-3 py-2"
                  />

                  <input
                    type="text"
                    placeholder="Postal Code"
                    value={form.shippingAddress.postalCode}
                    onChange={(e) =>
                      handleAddressChange(
                        "shippingAddress",
                        "postalCode",
                        e.target.value
                      )
                    }
                    className="w-full border border-gray-300 rounded px-3 py-2"
                    required
                  />

                  <input
                    type="text"
                    placeholder="Country"
                    value={form.shippingAddress.country}
                    onChange={(e) =>
                      handleAddressChange(
                        "shippingAddress",
                        "country",
                        e.target.value
                      )
                    }
                    className="w-full border border-gray-300 rounded px-3 py-2"
                    required
                  />
                </div>
              </div>
            </div>

            {/* =====================================================
                BILLING ADDRESS
            ====================================================== */}

            <div>
              <div className="flex items-center justify-between mb-3">
                <h2 className="text-lg font-semibold">
                  Billing Address
                </h2>

                <label className="flex items-center gap-2 text-sm cursor-pointer">
                  <input
                    type="checkbox"
                    checked={sameBillingAddress}
                    onChange={handleSameBillingChange}
                  />

                  Same as shipping
                </label>
              </div>

              {!sameBillingAddress && (
                <div className="space-y-4">
                  <input
                    type="text"
                    placeholder="Street Address"
                    value={form.billingAddress.street}
                    onChange={(e) =>
                      handleAddressChange(
                        "billingAddress",
                        "street",
                        e.target.value
                      )
                    }
                    className="w-full border border-gray-300 rounded px-3 py-2"
                    required
                  />

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <input
                      type="text"
                      placeholder="City"
                      value={form.billingAddress.city}
                      onChange={(e) =>
                        handleAddressChange(
                          "billingAddress",
                          "city",
                          e.target.value
                        )
                      }
                      className="w-full border border-gray-300 rounded px-3 py-2"
                      required
                    />

                    <input
                      type="text"
                      placeholder="State / Province"
                      value={form.billingAddress.state}
                      onChange={(e) =>
                        handleAddressChange(
                          "billingAddress",
                          "state",
                          e.target.value
                        )
                      }
                      className="w-full border border-gray-300 rounded px-3 py-2"
                    />

                    <input
                      type="text"
                      placeholder="Postal Code"
                      value={form.billingAddress.postalCode}
                      onChange={(e) =>
                        handleAddressChange(
                          "billingAddress",
                          "postalCode",
                          e.target.value
                        )
                      }
                      className="w-full border border-gray-300 rounded px-3 py-2"
                      required
                    />

                    <input
                      type="text"
                      placeholder="Country"
                      value={form.billingAddress.country}
                      onChange={(e) =>
                        handleAddressChange(
                          "billingAddress",
                          "country",
                          e.target.value
                        )
                      }
                      className="w-full border border-gray-300 rounded px-3 py-2"
                      required
                    />
                  </div>
                </div>
              )}
            </div>

            {/* =====================================================
                REGISTER
            ====================================================== */}

            <button
              type="submit"
              disabled={loading}
              className="w-full bg-black text-white py-2 rounded hover:bg-gray-800 disabled:opacity-50"
            >
              {loading ? "Registering..." : "Register"}
            </button>
          </form>

          <p className="text-center mt-4 text-sm">
            Already have an account?{" "}
            <a
              href="/auth/login"
              className="text-black hover:underline"
            >
              Login
            </a>
          </p>
        </div>
      </div>
    </Container>
  );
}
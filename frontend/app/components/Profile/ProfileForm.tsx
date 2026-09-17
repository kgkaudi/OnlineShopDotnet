"use client";

import PersonalInfoSection from "./PersonalInfoSection";
import AddressSection from "./AddressSection";
import BillingAddressSection from "./BillingAddressSection";

interface Address {
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

interface ProfileFormProps {
  profile: any;
  fullName: string;
  email: string;
  phoneNumber: string;
  shippingAddress: Address;
  billingAddress: Address;
  sameBillingAddress: boolean;
  saving: boolean;

  setFullName: (v: string) => void;
  setEmail: (v: string) => void;
  setPhoneNumber: (v: string) => void;

  updateShippingAddress: (field: keyof Address, value: string) => void;
  updateBillingAddress: (field: keyof Address, value: string) => void;
  handleSameBillingAddressChange: (checked: boolean) => void;

  onSubmit: (e: React.FormEvent<HTMLFormElement>) => void;
}

export default function ProfileForm({
  profile,
  fullName,
  email,
  phoneNumber,
  shippingAddress,
  billingAddress,
  sameBillingAddress,
  saving,
  setFullName,
  setEmail,
  setPhoneNumber,
  updateShippingAddress,
  updateBillingAddress,
  handleSameBillingAddressChange,
  onSubmit,
}: ProfileFormProps) {
  return (
    <form
      onSubmit={onSubmit}
      className="bg-white border rounded-2xl shadow-sm p-6 sm:p-8"
    >
      <div className="space-y-8">
        <PersonalInfoSection
          fullName={fullName}
          email={email}
          phoneNumber={phoneNumber}
          saving={saving}
          setFullName={setFullName}
          setEmail={setEmail}
          setPhoneNumber={setPhoneNumber}
        />

        <AddressSection
          title="Shipping Address"
          address={shippingAddress}
          saving={saving}
          onChange={updateShippingAddress}
        />

        <BillingAddressSection
          sameBillingAddress={sameBillingAddress}
          saving={saving}
          billingAddress={billingAddress}
          shippingAddress={shippingAddress}
          onToggleSame={handleSameBillingAddressChange}
          onChangeBilling={updateBillingAddress}
        />

        {/* Roles */}
        <section>
          <label className="block text-sm font-semibold mb-2">Roles</label>

          <div className="flex flex-wrap gap-2">
            {profile.roles.map((role: string) => (
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

        <button
          type="submit"
          disabled={saving}
          className="w-full bg-black text-white py-3 rounded-lg font-semibold hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed transition"
        >
          {saving ? "Saving..." : "Save Changes"}
        </button>
      </div>
    </form>
  );
}

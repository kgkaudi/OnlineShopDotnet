"use client";

import AddressSection from "./AddressSection";

interface Address {
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

interface BillingAddressSectionProps {
  sameBillingAddress: boolean;
  saving: boolean;
  billingAddress: Address;
  shippingAddress: Address;
  onToggleSame: (checked: boolean) => void;
  onChangeBilling: (field: keyof Address, value: string) => void;
}

export default function BillingAddressSection({
  sameBillingAddress,
  saving,
  billingAddress,
  shippingAddress,
  onToggleSame,
  onChangeBilling,
}: BillingAddressSectionProps) {
  return (
    <section>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-4">
        <h2 className="text-lg font-semibold">Billing Address</h2>

        <label className="flex items-center gap-2 text-sm cursor-pointer">
          <input
            type="checkbox"
            checked={sameBillingAddress}
            onChange={(e) => onToggleSame(e.target.checked)}
            disabled={saving}
          />
          Same as shipping
        </label>
      </div>

      {!sameBillingAddress && (
        <AddressSection
          title=""
          address={billingAddress}
          saving={saving}
          onChange={onChangeBilling}
        />
      )}
    </section>
  );
}

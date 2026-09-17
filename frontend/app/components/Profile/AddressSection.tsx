"use client";

interface Address {
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

interface AddressSectionProps {
  title: string;
  address: Address;
  saving: boolean;
  onChange: (field: keyof Address, value: string) => void;
}

export default function AddressSection({
  title,
  address,
  saving,
  onChange,
}: AddressSectionProps) {
  return (
    <section>
      <h2 className="text-lg font-semibold mb-4">{title}</h2>

      <div className="space-y-4">
        <div>
          <label className="block text-sm font-semibold mb-2">Street address</label>
          <input
            type="text"
            value={address.street}
            onChange={(e) => onChange("street", e.target.value)}
            className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
            disabled={saving}
          />
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-semibold mb-2">City</label>
            <input
              type="text"
              value={address.city}
              onChange={(e) => onChange("city", e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
              disabled={saving}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold mb-2">State / Province</label>
            <input
              type="text"
              value={address.state}
              onChange={(e) => onChange("state", e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
              disabled={saving}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold mb-2">Postal code</label>
            <input
              type="text"
              value={address.postalCode}
              onChange={(e) => onChange("postalCode", e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
              disabled={saving}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold mb-2">Country</label>
            <input
              type="text"
              value={address.country}
              onChange={(e) => onChange("country", e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
              disabled={saving}
            />
          </div>
        </div>
      </div>
    </section>
  );
}

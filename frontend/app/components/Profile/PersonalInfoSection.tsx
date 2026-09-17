"use client";

interface PersonalInfoSectionProps {
  fullName: string;
  email: string;
  phoneNumber: string;
  saving: boolean;
  setFullName: (v: string) => void;
  setEmail: (v: string) => void;
  setPhoneNumber: (v: string) => void;
}

export default function PersonalInfoSection({
  fullName,
  email,
  phoneNumber,
  saving,
  setFullName,
  setEmail,
  setPhoneNumber,
}: PersonalInfoSectionProps) {
  return (
    <section>
      <h2 className="text-lg font-semibold mb-4">Personal Information</h2>

      <div className="space-y-5">
        <div>
          <label className="block text-sm font-semibold mb-2">Full name</label>
          <input
            type="text"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
            disabled={saving}
          />
        </div>

        <div>
          <label className="block text-sm font-semibold mb-2">Email address</label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
            disabled={saving}
          />
        </div>

        <div>
          <label className="block text-sm font-semibold mb-2">Phone number</label>
          <input
            type="tel"
            value={phoneNumber}
            onChange={(e) => setPhoneNumber(e.target.value)}
            className="w-full border border-gray-300 rounded-lg px-4 py-3 outline-none focus:ring-2 focus:ring-black"
            disabled={saving}
          />
        </div>
      </div>
    </section>
  );
}

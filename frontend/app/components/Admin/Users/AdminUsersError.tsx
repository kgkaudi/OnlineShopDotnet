"use client";

interface Props {
  message: string;
}

export default function AdminUsersError({ message }: Props) {
  return (
    <div className="mb-6 rounded border border-red-300 bg-red-50 px-4 py-3 text-red-700">
      {message}
    </div>
  );
}

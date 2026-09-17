"use client";

interface Props {
  count: number;
}

export default function AdminUsersSummary({ count }: Props) {
  return (
    <p className="mt-4 text-sm text-gray-500">Total users: {count}</p>
  );
}

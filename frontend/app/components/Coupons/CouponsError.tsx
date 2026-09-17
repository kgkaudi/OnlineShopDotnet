"use client";

interface CouponsErrorProps {
  message: string;
}

export default function CouponsError({ message }: CouponsErrorProps) {
  return (
    <p className="text-red-600 bg-red-100 px-3 py-2 rounded">{message}</p>
  );
}

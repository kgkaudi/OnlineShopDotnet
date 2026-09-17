"use client";

interface ProfileErrorProps {
  message: string;
}

export default function ProfileError({ message }: ProfileErrorProps) {
  return (
    <div className="mb-6 rounded-lg bg-red-100 text-red-700 p-4">
      {message}
    </div>
  );
}

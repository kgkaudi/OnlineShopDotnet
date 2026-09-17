"use client";

interface Props {
  orderId: string;
}

export default function OrderDetailsHeader({ orderId }: Props) {
  return (
    <h1 className="text-2xl font-bold mb-4">Order {orderId}</h1>
  );
}

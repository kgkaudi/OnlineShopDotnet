"use client";

interface Props {
  price: number;
}

export default function ProductPrice({ price }: Props) {
  return (
    <div className="mt-6 text-3xl font-bold">
      €{Number(price).toFixed(2)}
    </div>
  );
}

"use client";

interface Props {
  items: any[];
}

export default function OrderDetailsItems({ items }: Props) {
  return (
    <>
      <h2 className="text-xl font-semibold mb-2">Items</h2>

      <ul className="space-y-3 mb-6">
        {items.map((item) => (
          <li key={item.productId} className="border p-3 rounded">
            <p>
              <strong>Product:</strong> {item.productName ?? item.productId}
            </p>
            <p>
              <strong>Quantity:</strong> {item.quantity}
            </p>
            <p>
              <strong>Unit Price:</strong> ${item.unitPrice.toFixed(2)}
            </p>
          </li>
        ))}
      </ul>
    </>
  );
}

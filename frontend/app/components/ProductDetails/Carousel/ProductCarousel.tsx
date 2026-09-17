"use client";

import React, { useRef, useState } from "react";

interface Props {
  productId: string;
  productName: string;
}

export default function ProductCarousel({ productId, productName }: Props) {
  const sampleImages = [
    "https://i.pinimg.com/736x/be/46/20/be4620077e92408d7f5f615801fe17eb.jpg",
    "https://i.pinimg.com/1200x/ba/8a/3f/ba8a3f82284aacbd1ab415243f7d19d3.jpg",
    "https://i.pinimg.com/1200x/8c/7b/fe/8c7bfe27502914f48958c870d945f496.jpg",
  ];

  const [index, setIndex] = useState(0);
  const touchStart = useRef<number | null>(null);

  function prev() {
    setIndex((i) => (i === 0 ? sampleImages.length - 1 : i - 1));
  }

  function next() {
    setIndex((i) => (i === sampleImages.length - 1 ? 0 : i + 1));
  }

  function handleTouchStart(e: React.TouchEvent) {
    touchStart.current = e.touches[0].clientX;
  }

  function handleTouchEnd(e: React.TouchEvent) {
    if (touchStart.current === null) return;
    const diff = e.changedTouches[0].clientX - touchStart.current;

    if (diff > 50) prev();
    if (diff < -50) next();

    touchStart.current = null;
  }

  return (
    <div className="w-full">
      <div
        className="relative w-full aspect-square bg-gray-100 overflow-hidden group"
        onTouchStart={handleTouchStart}
        onTouchEnd={handleTouchEnd}
      >
        <img
          src={sampleImages[index]}
          alt={productName}
          className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-110"
        />

        <button
          onClick={prev}
          className="absolute left-3 top-1/2 -translate-y-1/2 bg-white/70 hover:bg-white text-black rounded-full w-10 h-10 flex items-center justify-center shadow"
        >
          ‹
        </button>

        <button
          onClick={next}
          className="absolute right-3 top-1/2 -translate-y-1/2 bg-white/70 hover:bg-white text-black rounded-full w-10 h-10 flex items-center justify-center shadow"
        >
          ›
        </button>

        <div className="absolute bottom-4 left-0 right-0 flex justify-center gap-2">
          {sampleImages.map((_, i) => (
            <button
              key={i}
              onClick={() => setIndex(i)}
              className={`w-3 h-3 rounded-full ${
                i === index ? "bg-black" : "bg-white border"
              }`}
            />
          ))}
        </div>
      </div>

      <div className="flex gap-3 mt-4 justify-center">
        {sampleImages.map((img, i) => (
          <button
            key={i}
            onClick={() => setIndex(i)}
            className={`border rounded-lg overflow-hidden w-20 h-20 ${
              i === index ? "ring-2 ring-black" : ""
            }`}
          >
            <img src={img} alt="" className="object-cover w-full h-full" />
          </button>
        ))}
      </div>
    </div>
  );
}

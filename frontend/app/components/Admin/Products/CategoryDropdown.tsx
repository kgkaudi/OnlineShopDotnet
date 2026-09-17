"use client";

import { useEffect, useRef, useState } from "react";
import { Category } from "@/src/lib/api";

interface CategoryDropdownProps {
  value: string;
  categories: Category[];
  disabled?: boolean;
  onChange: (categoryId: string) => void;
}

export default function CategoryDropdown({
  value,
  categories,
  disabled = false,
  onChange,
}: CategoryDropdownProps) {
  const [open, setOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setOpen(false);
      }
    }

    if (open) {
      document.addEventListener("mousedown", handleClickOutside);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [open]);

  useEffect(() => {
    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setOpen(false);
      }
    }

    if (open) {
      document.addEventListener("keydown", handleEscape);
    }

    return () => {
      document.removeEventListener("keydown", handleEscape);
    };
  }, [open]);

  const selectedCategory = categories.find((c) => c.id === value);
  const selectedName = selectedCategory?.name ?? "Select a category";

  function handleSelect(categoryId: string) {
    onChange(categoryId);
    setOpen(false);
  }

  return (
    <div ref={dropdownRef} className="relative w-full">
      <button
        type="button"
        disabled={disabled || categories.length === 0}
        onClick={() => setOpen((prev) => !prev)}
        aria-haspopup="listbox"
        aria-expanded={open}
        className={`w-full rounded border bg-white px-3 py-2 text-left flex items-center justify-between focus:outline-none focus:ring-2 focus:ring-black ${
          disabled || categories.length === 0
            ? "cursor-not-allowed opacity-50"
            : "cursor-pointer hover:bg-gray-50"
        }`}
      >
        <span className={value ? "text-gray-900" : "text-gray-500"}>
          {selectedName}
        </span>

        <svg
          className={`h-5 w-5 transition-transform ${open ? "rotate-180" : ""}`}
          viewBox="0 0 20 20"
          fill="currentColor"
        >
          <path
            fillRule="evenodd"
            d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.51a.75.75 0 01-1.08 0l-4.25-4.51a.75.75 0 01.02-1.06z"
            clipRule="evenodd"
          />
        </svg>
      </button>

      {open && categories.length > 0 && (
        <div
          className="absolute z-50 mt-1 w-full rounded-md border bg-white shadow-lg overflow-hidden"
          role="listbox"
        >
          <button
            type="button"
            onClick={() => handleSelect("")}
            className={`w-full px-3 py-2 text-left hover:bg-gray-100 ${
              !value ? "bg-gray-100 font-medium" : ""
            }`}
            role="option"
            aria-selected={!value}
          >
            Select a category
          </button>

          {categories.map((category) => {
            const isSelected = value === category.id;

            return (
              <button
                key={category.id}
                type="button"
                onClick={() => handleSelect(category.id)}
                className={`w-full px-3 py-2 text-left hover:bg-gray-100 flex items-center justify-between ${
                  isSelected ? "bg-gray-100 font-medium" : ""
                }`}
                role="option"
                aria-selected={isSelected}
              >
                <span>{category.name}</span>

                {isSelected && (
                  <svg
                    className="h-5 w-5"
                    viewBox="0 0 20 20"
                    fill="currentColor"
                  >
                    <path
                      fillRule="evenodd"
                      d="M16.704 5.29a.75.75 0 010 1.06l-7.25 7.25a.75.75 0 01-1.06 0l-3.25-3.25a.75.75 0 111.06-1.06l2.72 2.72 6.72-6.72a.75.75 0 011.06 0z"
                      clipRule="evenodd"
                    />
                  </svg>
                )}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}

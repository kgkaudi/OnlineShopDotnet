"use client";

import { useEffect, useState } from "react";
import Container from "./components/Container";
import LatestProducts from "./components/Home/LatestProducts";
import HeroBanner from "./components/Home/HeroBanner";
import PromotionalBanners from "./components/Home/PromotionalBanners";
import { api, type Product } from "@/src/lib/api";

export default function HomePage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .getProducts()
      .then(setProducts)
      .finally(() => setLoading(false));
  }, []);

  return (
    <Container>
      {/* Hero Banner */}
      <HeroBanner />

      {/* Latest Products */}
      <LatestProducts products={products} loading={loading} />

      {/* Promotional Banners */}
      <PromotionalBanners />
    </Container>
  );
}

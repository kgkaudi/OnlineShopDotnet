import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";

import Header from "./components/Header";
import Footer from "./components/Footer";
import { SnackbarProvider } from "@/src/context/SnackbarContext";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "OnlineShop",
  description: "Frontend for OnlineShop built with Next.js 14 and Tailwind CSS v4",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className={`${geistSans.variable} ${geistMono.variable}`}>
      <body className="bg-gray-50">
        <SnackbarProvider>
          <Header />
          <main className="py-10">{children}</main>
          <Footer />
        </SnackbarProvider>
      </body>
    </html>
  );
}

import Container from "./components/Container";

export default function HomePage() {
  return (
    <Container>
      <h1 className="text-3xl font-bold mb-4">Welcome to OnlineShop</h1>
      <p className="text-gray-600">
        Browse products, manage your cart, and enjoy a smooth shopping experience.
      </p>
    </Container>
  );
}

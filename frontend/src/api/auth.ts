import api from "./api";

export async function register(data: {
  username: string;
  email: string;
  password: string;
}) {
  const res = await api.post("/user/register", data);
  return res.data;
}

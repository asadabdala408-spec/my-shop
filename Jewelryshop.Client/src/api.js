const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';

function getToken() {
  const user = JSON.parse(localStorage.getItem('jewelryUser') || 'null');
  return user?.token;
}

async function request(path, options = {}) {
  const token = getToken();
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      ...(options.body instanceof FormData ? {} : { 'Content-Type': 'application/json' }),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      localStorage.removeItem('jewelryUser');
    }
    const text = await response.text();
    let message = text;
    try {
      const data = JSON.parse(text);
      message = data.title || data.message || data.errors ? Object.values(data.errors || {}).flat().join(' ') || data.title || data.message : text;
    } catch {
      message = text;
    }
    throw new Error(message || `Request failed with status ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export const api = {
  getProducts: () => request('/api/products'),
  getProduct: (id) => request(`/api/products/${id}`),
  getCategories: () => request('/api/categories'),
  getCart: () => request('/api/cart'),
  getOrders: () => request('/api/orders'),
  getAdminOrders: () => request('/api/orders/admin'),
  getDashboard: () => request('/api/admin/dashboard'),
  login: (body) => request('/api/auth/login', { method: 'POST', body: JSON.stringify(body) }),
  register: (body) => request('/api/auth/register', { method: 'POST', body: JSON.stringify(body) }),
  addToCart: (productId, quantity = 1) => request('/api/cart', { method: 'POST', body: JSON.stringify({ productId, quantity }) }),
  updateCartItem: (id, productId, quantity) => request(`/api/cart/${id}`, { method: 'PUT', body: JSON.stringify({ productId, quantity }) }),
  removeCartItem: (id) => request(`/api/cart/${id}`, { method: 'DELETE' }),
  checkout: (body) => request('/api/orders/checkout', { method: 'POST', body: JSON.stringify(body) }),
  createCategory: (body) => request('/api/categories', { method: 'POST', body: JSON.stringify(body) }),
  deleteCategory: (id) => request(`/api/categories/${id}`, { method: 'DELETE' }),
  createProduct: (body) => request('/api/products', { method: 'POST', body: JSON.stringify(body) }),
  updateProduct: (id, body) => request(`/api/products/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteProduct: (id) => request(`/api/products/${id}`, { method: 'DELETE' }),
  updateOrderStatus: (id, status) => request(`/api/orders/${id}/status`, { method: 'PUT', body: JSON.stringify({ status }) }),
  uploadProductImage: (id, file) => {
    const formData = new FormData();
    formData.append('file', file);
    return request(`/api/products/${id}/image`, { method: 'POST', body: formData });
  },
};

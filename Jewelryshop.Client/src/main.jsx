import React, { useEffect, useMemo, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { Eye, Gem, LayoutDashboard, LogIn, RefreshCw, Search, ShoppingBag, ShoppingCart, Sparkles, Trash2, Upload } from 'lucide-react';
import { api } from './api';
import './styles.css';

const fallbackImage = 'https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?auto=format&fit=crop&w=900&q=80';
const emptyProduct = { name: '', description: '', price: '', stockQuantity: '', imageUrl: '', categoryId: '' };

function normalizeProduct(product) {
  return {
    name: product.name || '',
    description: product.description || '',
    price: product.price ?? '',
    stockQuantity: product.stockQuantity ?? '',
    imageUrl: product.imageUrl || '',
    categoryId: product.categoryId || '',
  };
}

function productPayload(product) {
  return {
    name: product.name,
    description: product.description,
    price: Number(product.price),
    stockQuantity: Number(product.stockQuantity),
    imageUrl: product.imageUrl || null,
    categoryId: Number(product.categoryId),
  };
}

function App() {
  const [user, setUser] = useState(() => JSON.parse(localStorage.getItem('jewelryUser') || 'null'));
  const [view, setView] = useState(() => JSON.parse(localStorage.getItem('jewelryUser') || 'null') ? 'shop' : 'login');
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [cart, setCart] = useState([]);
  const [orders, setOrders] = useState([]);
  const [adminOrders, setAdminOrders] = useState([]);
  const [dashboard, setDashboard] = useState(null);
  const [confirmedOrder, setConfirmedOrder] = useState(null);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const cartTotal = useMemo(() => cart.reduce((total, item) => total + (item.product?.price || 0) * item.quantity, 0), [cart]);

  async function loadData() {
    setLoading(true);
    setError('');
    try {
      const [productsData, categoriesData] = await Promise.all([api.getProducts(), api.getCategories()]);
      setProducts(productsData);
      setCategories(categoriesData);
      if (user) {
        const [cartData, ordersData] = await Promise.all([api.getCart().catch(() => []), api.getOrders().catch(() => [])]);
        setCart(cartData);
        setOrders(ordersData);
      } else {
        setCart([]);
        setOrders([]);
        setDashboard(null);
      }
      if (user?.role === 2) {
        setDashboard(await api.getDashboard().catch(() => null));
        setAdminOrders(await api.getAdminOrders().catch(() => []));
      }
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { loadData(); }, [user?.role]);

  async function runAction(action, success) {
    setLoading(true);
    setError('');
    setMessage('');
    try {
      const result = await action();
      setMessage(success);
      await loadData();
      return result;
    } catch (err) {
      setError(err.message);
      return null;
    } finally {
      setLoading(false);
    }
  }

  async function openProductDetails(productId) {
    const product = await runAction(() => api.getProduct(productId), 'Product details loaded.');
    if (product) setSelectedProduct(product);
  }

  async function handleLogin(email, password) {
    setLoading(true);
    setError('');
    setMessage('');
    try {
      const loggedInUser = await api.login({ email, password });
      setUser(loggedInUser);
      localStorage.setItem('jewelryUser', JSON.stringify(loggedInUser));
      setView(loggedInUser.role === 2 ? 'admin' : 'shop');
      setMessage('Login successful.');
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  async function handleRegister(body) {
    setLoading(true);
    setError('');
    setMessage('');
    try {
      const newUser = await api.register(body);
      setUser(newUser);
      localStorage.setItem('jewelryUser', JSON.stringify(newUser));
      setView(newUser.role === 2 ? 'admin' : 'shop');
      setMessage('Account created successfully.');
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  function logout() {
    setUser(null);
    setDashboard(null);
    setCart([]);
    setOrders([]);
    setAdminOrders([]);
    setConfirmedOrder(null);
    localStorage.removeItem('jewelryUser');
    setView('login');
    setMessage('Logged out.');
  }

  const isAuthView = !user && (view === 'login' || view === 'register');

  return <div className="app">
    {!isAuthView && <header className="topbar">
      {!user && <div className="brand" onClick={() => setView('shop')}>
        <span className="brandIcon"><Gem size={28} /></span>
        <div><strong>Jewelry Shop</strong><small>Luxury React storefront</small></div>
      </div>}
      {view !== 'admin' && <nav>
        {!user && <button className={view === 'about' ? 'active' : ''} onClick={() => setView('about')}>About</button>}
        {!user && <button className={view === 'contact' ? 'active' : ''} onClick={() => setView('contact')}>Contact</button>}
        {!user && <button className={view === 'support' ? 'active' : ''} onClick={() => setView('support')}>Support</button>}
        {user?.role === 2 && <button className={view === 'admin' ? 'active' : ''} onClick={() => setView('admin')}><LayoutDashboard size={18} /> Admin</button>}
      </nav>}
      {view === 'admin' && <div className="adminTopTitle"><LayoutDashboard size={18} /> Admin Dashboard</div>}
      <div className="userBox">{user ? <><span>{user.fullName}</span><button onClick={logout}>Logout</button></> : <><button className={view === 'login' ? 'active' : ''} onClick={() => setView('login')}><LogIn size={18} /> Login</button><button className={view === 'register' ? 'active' : ''} onClick={() => setView('register')}>Register</button></>}</div>
    </header>}

    <main>
      {user && user.role !== 2 && view !== 'admin' && <div className="customerShell">
        <aside className="customerSidebar">
          <h2>Menu</h2>
          <button className={view === 'shop' ? 'active' : ''} onClick={() => setView('shop')}><ShoppingBag size={18} /> Shop</button>
          <button className={view === 'cart' ? 'active' : ''} onClick={() => setView('cart')}><ShoppingCart size={18} /> Cart ({cart.length})</button>
          <button className={view === 'orders' ? 'active' : ''} onClick={() => setView('orders')}>Orders</button>
          <button className={view === 'account' ? 'active' : ''} onClick={() => setView('account')}>Account</button>
        </aside>
        <div className="customerContent">
          <section className="welcomeStrip">
            <div>
              <span className="badge">Customer account</span>
              <strong>Hi {user.fullName}, your personalized shopping experience is ready.</strong>
            </div>
            <button className="secondary" onClick={() => setView('account')}>View Account</button>
          </section>

          {message && <div className="alert success">{message}</div>}
          {error && <div className="alert error">{error}</div>}
          {loading && <div className="loading">Loading...</div>}

          {view === 'shop' && <Shop products={products} categories={categories} loading={loading} user={user} onRefresh={loadData} onAdd={(id) => runAction(() => api.addToCart(id, 1), 'Product added to cart.')} onDetails={openProductDetails} onAdminAction={runAction} />}
          {view === 'cart' && <Cart cart={cart} total={cartTotal} confirmedOrder={confirmedOrder} onUpdate={(item, quantity) => runAction(() => api.updateCartItem(item.id, item.productId, quantity), 'Cart updated.')} onRemove={(id) => runAction(() => api.removeCartItem(id), 'Item removed.')} onCheckout={(body) => runAction(async () => { const order = await api.checkout(body); setConfirmedOrder(order); return order; }, 'Order placed successfully.')} />}
          {view === 'orders' && <Orders orders={orders} />}
          {view === 'account' && <Account user={user} setView={setView} orders={orders} cart={cart} total={cartTotal} />}
        </div>
      </div>}
      {(!user || user.role === 2) && <>
        {message && <div className="alert success">{message}</div>}
        {error && <div className="alert error">{error}</div>}
        {loading && <div className="loading">Loading...</div>}
      </>}

      {!user && view === 'login' && <AuthPage mode="login" setView={setView} onLogin={handleLogin} onRegister={handleRegister} />}
      {!user && view === 'register' && <AuthPage mode="register" setView={setView} onLogin={handleLogin} onRegister={handleRegister} />}
      {user?.role === 2 && view === 'admin' && <Admin dashboard={dashboard} categories={categories} products={products} orders={adminOrders} onAction={runAction} />}
    </main>

    {!isAuthView && <Footer user={user} setView={setView} />}

    {selectedProduct && <ProductDetails product={selectedProduct} relatedProducts={products.filter((item) => item.categoryId === selectedProduct.categoryId && item.id !== selectedProduct.id).slice(0, 3)} onClose={() => setSelectedProduct(null)} onDetails={openProductDetails} onAdd={(id, quantity) => runAction(() => api.addToCart(id, quantity), 'Product added to cart.')} />}
  </div>;
}

function Footer({ user, setView }) {
  return <footer className="footer">
    <div className="footerGrid">
      <div className="footerBrand">
        <span className="brandIcon"><Gem size={26} /></span>
        <div>
          <strong>Jewelry Shop</strong>
          <p>Premium jewelry crafted for elegant moments, secure shopping, and a smooth customer experience.</p>
        </div>
      </div>
      <div>
        <h4>Explore</h4>
        <button onClick={() => setView(user ? 'shop' : 'login')}>{user ? 'Shop Collection' : 'Login / Register'}</button>
        {user && <button onClick={() => setView('cart')}>Shopping Cart</button>}
        {user && <button onClick={() => setView('orders')}>My Orders</button>}
      </div>
      <div>
        <h4>About</h4>
        <span>Premium jewelry catalog</span>
        <span>Private customer experience</span>
        <span>Modern admin management</span>
      </div>
      <div>
        <h4>Contact & Support</h4>
        <span>support@jewelryshop.com</span>
        <span>Open daily: 9:00 - 18:00</span>
        <span>Mogadishu, Somalia</span>
      </div>
    </div>
    <div className="footerBottom">
      <span>© {new Date().getFullYear()} Jewelry Shop. All rights reserved.</span>
      <span>Luxury React storefront powered by ASP.NET Core API.</span>
    </div>
  </footer>;
}

function AuthPage({ mode, setView, onLogin, onRegister }) {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [authError, setAuthError] = useState('');
  const isRegister = mode === 'register';

  function submitAuth() {
    setAuthError('');
    if (isRegister && !fullName.trim()) {
      setAuthError('Please enter your full name.');
      return;
    }
    if (!email.trim()) {
      setAuthError('Please enter your email address.');
      return;
    }
    if (!password) {
      setAuthError('Please enter your password.');
      return;
    }
    if (password.length < 6) {
      setAuthError('Password must be at least 6 characters.');
      return;
    }
    if (isRegister) {
      onRegister({ fullName: fullName.trim(), email: email.trim(), password });
      return;
    }
    onLogin(email.trim(), password);
  }

  return <section className="authShell">
    <div className="authShowcase">
      <div className="authShield">◇</div>
      <div>
        <h2>{isRegister ? 'Create your account' : 'Welcome back'}</h2>
        <p>{isRegister ? 'Join our luxury jewelry store and enjoy a private cart, smooth checkout, and order tracking.' : 'Access your shopping account, manage your cart, and track your jewelry orders with ease.'}</p>
      </div>
      <div className="authFeatureGrid">
        <span>Premium Collection</span>
        <span>Secure Checkout</span>
      </div>
    </div>
    <div className="authCard">
      <div className="authLogo"><Gem size={34} /><strong>Jewelry Shop</strong></div>
      <h2>{isRegister ? 'Create Account' : 'Sign In'}</h2>
      <p>{isRegister ? 'Enter your information to create your account' : 'Enter your credentials to access your account'}</p>
      {authError && <div className="alert error">{authError}</div>}
      {isRegister && <label><span>Full Name</span><input value={fullName} onChange={(event) => setFullName(event.target.value)} placeholder="Enter your full name" /></label>}
      <label><span>Email Address</span><input value={email} onChange={(event) => setEmail(event.target.value)} placeholder="Enter your email address" /></label>
      <label><span>Password</span><input value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Enter your password" type="password" /></label>
      <button className="primary full" onClick={submitAuth}>{isRegister ? 'Create Account' : 'Sign In'}</button>
      <div className="authSwitch">
        {isRegister ? <span>Already have an account?</span> : <span>Need a new account?</span>}
        <button className="secondary" onClick={() => setView(isRegister ? 'login' : 'register')}>{isRegister ? 'Go to Login' : 'Create Account'}</button>
      </div>
    </div>
  </section>;
}

function Shop({ products, categories, loading, user, onRefresh, onAdd, onDetails, onAdminAction }) {
  const [query, setQuery] = useState('');
  const [categoryId, setCategoryId] = useState('all');
  const [maxPrice, setMaxPrice] = useState('');
  const [stockOnly, setStockOnly] = useState(false);
  const [sort, setSort] = useState('name');

  const filtered = useMemo(() => {
    return products
      .filter((product) => categoryId === 'all' || product.categoryId === Number(categoryId))
      .filter((product) => !query || `${product.name} ${product.description}`.toLowerCase().includes(query.toLowerCase()))
      .filter((product) => !maxPrice || Number(product.price) <= Number(maxPrice))
      .filter((product) => !stockOnly || product.stockQuantity > 0)
      .sort((a, b) => {
        if (sort === 'priceLow') return Number(a.price) - Number(b.price);
        if (sort === 'priceHigh') return Number(b.price) - Number(a.price);
        if (sort === 'stock') return Number(b.stockQuantity) - Number(a.stockQuantity);
        return a.name.localeCompare(b.name);
      });
  }, [products, categories, query, categoryId, maxPrice, stockOnly, sort]);

  const featuredCategories = categories.slice(0, 4);

  function deleteProduct(productId) {
    return onAdminAction(() => api.deleteProduct(productId), 'Product deleted.');
  }

  function editProduct(product) {
    const name = prompt('Product name', product.name);
    if (!name) return null;
    const price = prompt('Product price', product.price);
    if (!price) return null;
    const stockQuantity = prompt('Stock quantity', product.stockQuantity);
    if (!stockQuantity) return null;
    return onAdminAction(() => api.updateProduct(product.id, productPayload({ ...product, name, price, stockQuantity, categoryId: product.categoryId })), 'Product updated.');
  }

  return <section>
    <div className="collectionIntro">
      <div>
        <p className="eyebrow dark">Curated collection</p>
        <h2>Shop premium jewelry selected for modern elegance.</h2>
      </div>
      <div className="categoryPills">{featuredCategories.map((category) => <button key={category.id} className="pill" onClick={() => setCategoryId(String(category.id))}>{category.name}</button>)}</div>
    </div>
    <div className="sectionHead"><h2>Products</h2><span className="resultCount">{filtered.length} items found</span></div>
    <div className="filtersPanel">
      <label className="searchBox"><Search size={18} /><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search products..." /></label>
      <select value={categoryId} onChange={(event) => setCategoryId(event.target.value)}><option value="all">All categories</option>{categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select>
      <input value={maxPrice} onChange={(event) => setMaxPrice(event.target.value)} placeholder="Max price" type="number" />
      <select value={sort} onChange={(event) => setSort(event.target.value)}><option value="name">Sort by name</option><option value="priceLow">Price low to high</option><option value="priceHigh">Price high to low</option><option value="stock">Most stock</option></select>
      <label className="checkFilter"><input type="checkbox" checked={stockOnly} onChange={(event) => setStockOnly(event.target.checked)} /> In stock only</label>
    </div>
    {filtered.length === 0 ? <div className="emptyState">
      <Gem size={42} />
      <h3>{loading ? 'Loading collection...' : 'No products are showing yet'}</h3>
      <p>If you just logged in, refresh the collection. If it stays empty, make sure the backend is running and the database seed data has been applied.</p>
      <button className="primary" onClick={onRefresh}><RefreshCw size={18} /> Refresh Products</button>
    </div> : <div className="grid">{filtered.map((product) => <ProductCard key={product.id} product={product} isAdmin={user?.role === 2} onAdd={onAdd} onDetails={onDetails} onEdit={editProduct} onDelete={deleteProduct} />)}</div>}
  </section>;
}

function ProductCard({ product, isAdmin, onAdd, onDetails, onEdit, onDelete }) {
  return <article className="card productCard elevatedCard">
    <img src={product.imageUrl || fallbackImage} alt={product.name} />
    <div className="cardBody">
      <small>{product.category?.name || 'Jewelry'}</small>
      <h3>{product.name}</h3>
      <p>{product.description}</p>
      <div className="productMeta"><strong>${Number(product.price).toFixed(2)}</strong><span>{product.stockQuantity} in stock</span></div>
      <div className="splitActions"><button className="secondary" onClick={() => onDetails(product.id)}><Eye size={16} /> Details</button><button className="primary" onClick={() => onAdd(product.id)}>Add</button></div>
      {isAdmin && <div className="adminCardActions"><button className="secondary" onClick={() => onEdit(product)}>Edit</button><button className="danger" onClick={() => onDelete(product.id)}>Delete</button></div>}
    </div>
  </article>;
}

function ProductDetails({ product, relatedProducts, onClose, onDetails, onAdd }) {
  const [quantity, setQuantity] = useState(1);
  const inStock = product.stockQuantity > 0;
  return <div className="modalBackdrop" onClick={onClose}>
    <article className="detailsModal" onClick={(event) => event.stopPropagation()}>
      <img src={product.imageUrl || fallbackImage} alt={product.name} />
      <div className="detailsBody">
        <button className="closeButton" onClick={onClose}>×</button>
        <span className="badge">{product.category?.name || 'Jewelry'}</span><span className={inStock ? 'stockBadge inStock' : 'stockBadge outStock'}>{inStock ? `${product.stockQuantity} in stock` : 'Out of stock'}</span>
        <h2>{product.name}</h2>
        <p>{product.description}</p>
        <div className="detailsStats"><Stat label="Price" value={`$${Number(product.price).toFixed(2)}`} /><Stat label="Stock" value={product.stockQuantity} /><Stat label="Product ID" value={`#${product.id}`} /></div>
        <div className="quantitySelector"><button className="secondary" disabled={quantity <= 1} onClick={() => setQuantity(quantity - 1)}>-</button><strong>{quantity}</strong><button className="secondary" disabled={quantity >= product.stockQuantity} onClick={() => setQuantity(quantity + 1)}>+</button></div>
        <button className="primary full" disabled={!inStock} onClick={() => onAdd(product.id, quantity)}>Add {quantity} to cart</button>
        {relatedProducts.length > 0 && <div className="relatedProducts"><h3>Related products</h3>{relatedProducts.map((item) => <button key={item.id} className="relatedItem" onClick={() => onDetails(item.id)}><img src={item.imageUrl || fallbackImage} alt={item.name} /><span>{item.name}</span><strong>${Number(item.price).toFixed(2)}</strong></button>)}</div>}
      </div>
    </article>
  </div>;
}

function Cart({ cart, total, confirmedOrder, onUpdate, onRemove, onCheckout }) {
  const [confirmCheckout, setConfirmCheckout] = useState(false);
  const [shippingAddress, setShippingAddress] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('Cash on delivery');
  return <section className="customerLayout">
    <div className="panel customerPanel">
      <h2>Shopping Cart</h2>
      {confirmedOrder && <div className="confirmationCard"><h3>Order confirmed</h3><p>Order #{confirmedOrder.id} has been placed successfully.</p><div className="detailsStats"><Stat label="Status" value={confirmedOrder.status} /><Stat label="Total" value={`$${Number(confirmedOrder.totalAmount).toFixed(2)}`} /><Stat label="Payment" value={confirmedOrder.paymentMethod || paymentMethod} /></div></div>}
      {cart.length === 0 ? <p>Your cart is empty. Go to Shop and add beautiful jewelry pieces.</p> : cart.map((item) => <div className="row cartLine" key={item.id}>
        <div><strong>{item.product?.name}</strong><small>${Number(item.product?.price || 0).toFixed(2)} each</small></div>
        <div className="quantityControl"><button className="secondary" disabled={item.quantity <= 1} onClick={() => onUpdate(item, item.quantity - 1)}>-</button><span>{item.quantity}</span><button className="secondary" onClick={() => onUpdate(item, item.quantity + 1)}>+</button></div>
        <div className="rowActions"><strong>${((item.product?.price || 0) * item.quantity).toFixed(2)}</strong><button className="danger" onClick={() => onRemove(item.id)}><Trash2 size={16} /></button></div>
      </div>)}
    </div>
    <aside className="panel checkoutCard">
      <h2>Checkout</h2>
      <div className="total"><span>Subtotal</span><strong>${total.toFixed(2)}</strong></div>
      <input value={shippingAddress} onChange={(event) => setShippingAddress(event.target.value)} placeholder="Shipping address" />
      <input value={phoneNumber} onChange={(event) => setPhoneNumber(event.target.value)} placeholder="Phone number" />
      <select value={paymentMethod} onChange={(event) => setPaymentMethod(event.target.value)}><option>Cash on delivery</option><option>Card payment placeholder</option><option>Mobile money placeholder</option></select>
      <p>Order status will start as Pending. Payment integrations can be connected later.</p>
      {!confirmCheckout ? <button className="primary full" disabled={cart.length === 0 || !shippingAddress || !phoneNumber} onClick={() => setConfirmCheckout(true)}>Continue Checkout</button> : <><div className="alert success">Ready to place your order?</div><button className="primary full" onClick={() => onCheckout({ shippingAddress, phoneNumber, paymentMethod })}>Place Order</button><button className="secondary full" onClick={() => setConfirmCheckout(false)}>Cancel</button></>}
    </aside>
  </section>;
}

function Orders({ orders }) {
  return <section className="panel"><h2>My Orders</h2>{orders.length === 0 ? <p>No orders yet.</p> : orders.map((order) => <div className="row" key={order.id}><div><strong>Order #{order.id}</strong><small>{new Date(order.createdAt).toLocaleString()}</small></div><div><span className="badge">{order.status}</span><strong>${Number(order.totalAmount).toFixed(2)}</strong></div></div>)}</section>;
}

function Account({ user, setView, orders, cart, total }) {
  if (!user) return <section className="panel accountHero"><h2>Customer Account</h2><p>Login ama register samee si aad account-kaaga u aragto.</p><button className="primary" onClick={() => setView('login')}>Login / Register</button></section>;
  return <section className="customerLayout">
    <div className="panel accountHero"><h2>Welcome, {user.fullName}</h2><p>{user.email}</p><span className="badge">{user.role === 2 ? 'Admin' : 'Customer'}</span><button className="primary" onClick={() => setView('shop')}>Continue Shopping</button></div>
    <div className="panel"><h2>Customer Summary</h2><div className="stats"><Stat label="Cart Items" value={cart.length} /><Stat label="Cart Total" value={`$${total.toFixed(2)}`} /><Stat label="Orders" value={orders.length} /></div></div>
  </section>;
}
function Admin({ dashboard, categories, products, orders, onAction }) {
  const [adminPage, setAdminPage] = useState('dashboard');
  const [categoryName, setCategoryName] = useState('');
  const [categoryDescription, setCategoryDescription] = useState('');
  const [product, setProduct] = useState(emptyProduct);
  const [editingId, setEditingId] = useState('');
  const [editProduct, setEditProduct] = useState(emptyProduct);
  const [imageProductId, setImageProductId] = useState('');
  const [file, setFile] = useState(null);

  function startEdit(item) {
    setEditingId(item.id);
    setEditProduct(normalizeProduct(item));
  }

  return <section className="adminShell">
    <aside className="adminSidebar">
      <h2>Admin Panel</h2>
      <button className={adminPage === 'dashboard' ? 'active' : ''} onClick={() => setAdminPage('dashboard')}>Dashboard</button>
      <button className={adminPage === 'orders' ? 'active' : ''} onClick={() => setAdminPage('orders')}>Orders</button>
      <button className={adminPage === 'products' ? 'active' : ''} onClick={() => setAdminPage('products')}>Products</button>
      <button className={adminPage === 'categories' ? 'active' : ''} onClick={() => setAdminPage('categories')}>Categories</button>
      <button className={adminPage === 'upload' ? 'active' : ''} onClick={() => setAdminPage('upload')}>Upload Images</button>
    </aside>
    <div className="adminContent">
      {adminPage === 'dashboard' && <>
        <div className="panel"><h2>Admin Dashboard</h2><div className="stats"><Stat label="Products" value={dashboard?.totalProducts ?? products.length} /><Stat label="Categories" value={dashboard?.totalCategories ?? categories.length} /><Stat label="Orders" value={dashboard?.totalOrders ?? 0} /><Stat label="Revenue" value={`$${Number(dashboard?.totalRevenue ?? 0).toFixed(2)}`} /><Stat label="Pending" value={dashboard?.pendingOrders ?? 0} /><Stat label="Low Stock" value={dashboard?.lowStockProducts ?? 0} /></div></div>
        <div className="panel analyticsGrid">
          <AnalyticsCard title="Order Status" items={(dashboard?.statusBreakdown || []).map((item) => ({ label: item.status, value: item.count, helper: `$${Number(item.totalAmount).toFixed(2)}` }))} empty="No order status data yet." />
          <AnalyticsCard title="Best Sellers" items={(dashboard?.bestSellers || []).map((item) => ({ label: item.productName, value: `${item.quantitySold} sold`, helper: `$${Number(item.revenue).toFixed(2)}` }))} empty="No sales data yet." />
          <AnalyticsCard title="Low Stock Alerts" items={(dashboard?.lowStockItems || []).map((item) => ({ label: item.name, value: `${item.stockQuantity} left`, helper: item.categoryName }))} empty="No low stock products." />
          <AnalyticsCard title="Recent Orders" items={(dashboard?.recentOrders || []).map((item) => ({ label: `#${item.id} - ${item.customerName}`, value: item.status, helper: `$${Number(item.totalAmount).toFixed(2)}` }))} empty="No recent orders." />
        </div>
      </>}
      {adminPage === 'orders' && <AdminOrders orders={orders} onAction={onAction} />}
      {adminPage === 'products' && <>
        <ProductForm title="Create Product" product={product} setProduct={setProduct} categories={categories} buttonText="Create Product" onSubmit={() => onAction(async () => { await api.createProduct(productPayload(product)); setProduct(emptyProduct); }, 'Product created.')} />
        {editingId && <ProductForm title={`Edit Product #${editingId}`} product={editProduct} setProduct={setEditProduct} categories={categories} buttonText="Save Changes" onSubmit={() => onAction(async () => { await api.updateProduct(editingId, productPayload(editProduct)); setEditingId(''); setEditProduct(emptyProduct); }, 'Product updated.')} onCancel={() => { setEditingId(''); setEditProduct(emptyProduct); }} />}
        <AdminProducts products={products} startEdit={startEdit} onAction={onAction} />
      </>}
      {adminPage === 'categories' && <div className="adminGrid"><div className="formCard"><h3>Create Category</h3><input value={categoryName} onChange={(event) => setCategoryName(event.target.value)} placeholder="Category name" /><textarea value={categoryDescription} onChange={(event) => setCategoryDescription(event.target.value)} placeholder="Description" /><button className="primary" onClick={() => onAction(async () => { await api.createCategory({ name: categoryName, description: categoryDescription }); setCategoryName(''); setCategoryDescription(''); }, 'Category created.')}>Create Category</button></div><AdminCategories categories={categories} /></div>}
      {adminPage === 'upload' && <div className="formCard"><h3>Upload Product Image</h3><select value={imageProductId} onChange={(event) => setImageProductId(event.target.value)}><option value="">Select product</option>{products.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select><input type="file" accept="image/*" onChange={(event) => setFile(event.target.files?.[0] || null)} /><button className="primary" disabled={!imageProductId || !file} onClick={() => onAction(async () => { await api.uploadProductImage(imageProductId, file); setFile(null); }, 'Image uploaded to Cloudinary.')}><Upload size={16} /> Upload Image</button></div>}
    </div>
  </section>;
}

function AdminOrders({ orders, onAction }) {
  return <div className="panel"><h2>Orders Table</h2><div className="tableWrap"><table><thead><tr><th>Order</th><th>Customer</th><th>Shipping</th><th>Items</th><th>Total</th><th>Status</th></tr></thead><tbody>{orders.map((order) => <tr key={order.id}><td>#{order.id}<small>{new Date(order.createdAt).toLocaleString()}</small></td><td>{order.user?.fullName || 'Customer'}<small>{order.user?.email || 'No email'}</small></td><td>{order.shippingAddress || 'No address'}<small>{order.phoneNumber || 'No phone'} - {order.paymentMethod || 'No payment'}</small></td><td>{order.items?.map((item) => <small key={item.id}>{item.product?.name || `Product #${item.productId}`} × {item.quantity}</small>)}</td><td>${Number(order.totalAmount).toFixed(2)}</td><td><select value={order.status} onChange={(event) => onAction(() => api.updateOrderStatus(order.id, event.target.value), 'Order status updated.')}><option>Pending</option><option>Processing</option><option>Shipped</option><option>Completed</option><option>Cancelled</option></select></td></tr>)}</tbody></table></div>{orders.length === 0 && <p>No orders yet.</p>}</div>;
}

function AdminProducts({ products, startEdit, onAction }) {
  return <div className="panel"><h2>Products Table</h2><div className="tableWrap"><table><thead><tr><th>Product</th><th>Category</th><th>Price</th><th>Stock</th><th>Actions</th></tr></thead><tbody>{products.map((item) => <tr key={item.id}><td>{item.name}<small>#{item.id}</small></td><td>{item.category?.name || 'No category'}</td><td>${Number(item.price).toFixed(2)}</td><td>{item.stockQuantity}</td><td><div className="tableActions"><button className="secondary" onClick={() => startEdit(item)}>Edit</button><button className="danger" onClick={() => onAction(() => api.deleteProduct(item.id), 'Product deleted.')}>Delete</button></div></td></tr>)}</tbody></table></div></div>;
}

function AdminCategories({ categories }) {
  return <div className="panel"><h2>Categories Table</h2><div className="tableWrap"><table><thead><tr><th>ID</th><th>Name</th><th>Description</th></tr></thead><tbody>{categories.map((category) => <tr key={category.id}><td>#{category.id}</td><td>{category.name}</td><td>{category.description}</td></tr>)}</tbody></table></div></div>;
}

function AnalyticsCard({ title, items, empty }) {
  return <div className="analyticsCard">
    <h3>{title}</h3>
    {items.length === 0 ? <p>{empty}</p> : items.map((item) => <div className="analyticsRow" key={`${title}-${item.label}`}>
      <span>{item.label}</span>
      <strong>{item.value}</strong>
      <small>{item.helper}</small>
    </div>)}
  </div>;
}

function ProductForm({ title, product, setProduct, categories, buttonText, onSubmit, onCancel }) {
  return <div className="formCard"><h3>{title}</h3><input value={product.name} onChange={(event) => setProduct({ ...product, name: event.target.value })} placeholder="Name" /><textarea value={product.description} onChange={(event) => setProduct({ ...product, description: event.target.value })} placeholder="Description" /><input value={product.price} onChange={(event) => setProduct({ ...product, price: event.target.value })} placeholder="Price" type="number" /><input value={product.stockQuantity} onChange={(event) => setProduct({ ...product, stockQuantity: event.target.value })} placeholder="Stock" type="number" /><input value={product.imageUrl} onChange={(event) => setProduct({ ...product, imageUrl: event.target.value })} placeholder="Image URL optional" /><select value={product.categoryId} onChange={(event) => setProduct({ ...product, categoryId: event.target.value })}><option value="">Select category</option>{categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select><div className="splitActions"><button className="primary" onClick={onSubmit}>{buttonText}</button>{onCancel && <button className="secondary" onClick={onCancel}>Cancel</button>}</div></div>;
}

function Stat({ label, value }) {
  return <div className="stat"><span>{label}</span><strong>{value}</strong></div>;
}

createRoot(document.getElementById('root')).render(<App />);




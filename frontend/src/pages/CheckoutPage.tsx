import { useEffect, useMemo, useState } from 'react';
import './CheckoutPage.css';
import { getPaymentMethods } from '../api/paymentMethods';
import { useCart } from '../contexts/CartContext';

const CheckoutPage = () => {
  const { items, total } = useCart();
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');

  const [methods, setMethods] = useState<string[]>([]);
  const [loadingMethods, setLoadingMethods] = useState(true);
  const [selectedMethod, setSelectedMethod] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const data = await getPaymentMethods();
        if (cancelled) return;
        const next = data.length ? data : ['Банковская карта', 'Онлайн‑кошелёк', 'Безналичный расчёт'];
        setMethods(next);
        setSelectedMethod(next[0] ?? null);
      } catch {
        if (cancelled) return;
        const fallback = ['Банковская карта', 'Онлайн‑кошелёк', 'Безналичный расчёт'];
        setMethods(fallback);
        setSelectedMethod(fallback[0] ?? null);
      } finally {
        if (!cancelled) setLoadingMethods(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const totalFormatted = useMemo(() => `${total} ₽`, [total]);

  return (
    <div className="checkout-page">
      <div className="checkout-inner">
        <h1>Оформление заказа</h1>

        <div className="checkout-layout">
          <section className="checkout-main">
            <div className="checkout-card">
              <h2>Контактные данные</h2>
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="firstName">Имя</label>
                  <input
                    id="firstName"
                    type="text"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    placeholder="Введите имя"
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="lastName">Фамилия</label>
                  <input
                    id="lastName"
                    type="text"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    placeholder="Введите фамилию"
                  />
                </div>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="phone">Телефон</label>
                  <input
                    id="phone"
                    type="tel"
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                    placeholder="+7"
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="email">Email</label>
                  <input
                    id="email"
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="user@mail.com"
                  />
                </div>
              </div>
            </div>

            <div className="checkout-card">
              <h2>Способ оплаты</h2>

              {loadingMethods ? (
                <p className="checkout-methods-loading">Загрузка способов оплаты...</p>
              ) : methods.length === 0 ? (
                <p className="checkout-methods-empty">Способы оплаты недоступны</p>
              ) : (
                methods.map((m) => {
                  const active = selectedMethod === m;
                  return (
                    <div
                      key={m}
                      className={`payment-option ${active ? 'active' : ''}`}
                      role="button"
                      tabIndex={0}
                      onClick={() => setSelectedMethod(m)}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter' || e.key === ' ') setSelectedMethod(m);
                      }}
                    >
                      <div className="payment-radio" />
                      <div>
                        <div className="payment-title">{m}</div>
                        <div className="payment-subtitle">Выбран</div>
                      </div>
                    </div>
                  );
                })
              )}
            </div>
          </section>

          <aside className="checkout-summary">
            <h2>Итого</h2>
            {items.length === 0 ? (
              <p className="checkout-methods-empty">Корзина пуста</p>
            ) : (
              items.map((it) => (
                <div key={it.courseId} className="checkout-summary-row">
                  <span>{it.title}</span>
                  <span>{it.price} ₽</span>
                </div>
              ))
            )}
            <div className="checkout-summary-total">
              <span>К оплате</span>
              <span>{totalFormatted}</span>
            </div>
            <button className="btn btn-primary btn-full" type="button">
              Оплатить заказ
            </button>
            <p className="checkout-hint">
              Нажимая кнопку, вы подтверждаете своё согласие с политикой конфиденциальности и
              договором оферты.
            </p>
          </aside>
        </div>
      </div>
    </div>
  );
};

export default CheckoutPage;


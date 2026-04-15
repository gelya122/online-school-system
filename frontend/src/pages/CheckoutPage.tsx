import { useEffect, useMemo, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { QRCodeSVG } from 'qrcode.react';
import './CheckoutPage.css';
import { getPaymentMethods, type PaymentMethodOption } from '../api/paymentMethods';
import { postCheckout } from '../api/checkout';
import { useCart } from '../contexts/CartContext';
import { useAuth } from '../contexts/AuthContext';
import { isValidEmailFormat } from '../utils/emailValidation';
import { MaskedPhoneInput } from '../components/MaskedPhoneInput';
import { isRuPhoneComplete } from '../utils/phoneMask';
import { getPaymentMethodKind, pickDefaultPaymentMethod } from '../utils/paymentMethodKind';
import {
  formatCardExpiryInput,
  formatCardNumberInput,
  isCardCvcValid,
  isCardExpiryValid,
  isCardNumberValid,
} from '../utils/cardInputMask';

type CheckoutLocationState = {
  promoCode?: string | null;
};

const CheckoutPage = () => {
  const { items, total, clearCart } = useCart();
  const { user, isAuthenticated } = useAuth();
  const location = useLocation();
  const promoFromCart = (location.state as CheckoutLocationState | null)?.promoCode ?? null;

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phoneDigits, setPhoneDigits] = useState('');
  const [email, setEmail] = useState('');

  const [methods, setMethods] = useState<PaymentMethodOption[]>([]);
  const [loadingMethods, setLoadingMethods] = useState(true);
  const [methodsLoadError, setMethodsLoadError] = useState<string | null>(null);
  const [selectedMethod, setSelectedMethod] = useState<PaymentMethodOption | null>(null);

  const [cardHolder, setCardHolder] = useState('');
  const [cardNumber, setCardNumber] = useState('');
  const [cardExpiry, setCardExpiry] = useState('');
  const [cardCvc, setCardCvc] = useState('');

  const [installmentCount, setInstallmentCount] = useState(3);

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successOrder, setSuccessOrder] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      setMethodsLoadError(null);
      try {
        const data = await getPaymentMethods();
        if (cancelled) return;
        const list = Array.isArray(data) ? data : [];
        setMethods(list);
        setSelectedMethod(pickDefaultPaymentMethod(list));
      } catch {
        if (cancelled) return;
        setMethods([]);
        setSelectedMethod(null);
        setMethodsLoadError('Не удалось загрузить способы оплаты.');
      } finally {
        if (!cancelled) setLoadingMethods(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const paymentKind = useMemo(() => {
    if (!selectedMethod) return 'other' as const;
    return getPaymentMethodKind(selectedMethod.methodName, selectedMethod.description);
  }, [selectedMethod]);

  const showCardForm = paymentKind === 'card' || paymentKind === 'other';
  const showSbp = paymentKind === 'sbp';
  const showInstallmentFields = paymentKind === 'installment';

  const sbpQrValue = useMemo(
    () =>
      `https://example.com/sbp-pay?amount=${encodeURIComponent(String(total))}&user=${user?.id ?? 0}&src=onlineschool`,
    [total, user?.id],
  );

  const totalFormatted = useMemo(() => `${total} ₽`, [total]);

  const handlePay = async () => {
    setError(null);
    if (!isAuthenticated || !user?.id) {
      return;
    }
    if (items.length === 0) {
      setError('Корзина пуста.');
      return;
    }

    const em = email.trim();
    if (em && !isValidEmailFormat(em)) {
      setError('Введите корректный адрес электронной почты.');
      return;
    }
    if (phoneDigits.length > 0 && !isRuPhoneComplete(phoneDigits)) {
      setError('Введите полный номер телефона в формате +7 (000) 000-00-00.');
      return;
    }

    if (!selectedMethod || selectedMethod.methodId <= 0) {
      setError('Выберите способ оплаты.');
      return;
    }

    if (showCardForm) {
      if (!cardHolder.trim() || cardHolder.trim().length < 2) {
        setError('Укажите имя на карте.');
        return;
      }
      if (!isCardNumberValid(cardNumber)) {
        setError('Номер карты должен содержать 16 цифр.');
        return;
      }
      if (!isCardExpiryValid(cardExpiry)) {
        setError('Укажите срок действия карты (ММ/ГГ).');
        return;
      }
      if (!isCardCvcValid(cardCvc)) {
        setError('Укажите CVC/CVV (3 или 4 цифры).');
        return;
      }
    }

    const methodId = selectedMethod.methodId;
    const useInstallment = showInstallmentFields;

    setSubmitting(true);
    try {
      const res = await postCheckout({
        userId: user.id,
        methodId,
        promoCode: promoFromCart?.trim() || undefined,
        useInstallment,
        installmentCount: useInstallment ? installmentCount : undefined,
        items: items.map((it) => ({
          courseId: it.courseId,
          quantity: 1,
        })),
      });

      setSuccessOrder(res.orderNumber);
      clearCart();
    } catch (e: unknown) {
      const ax = e as { response?: { data?: unknown } };
      const d = ax.response?.data;
      let text = 'Не удалось оформить заказ. Проверьте данные и попробуйте снова.';
      if (typeof d === 'string') text = d;
      else if (d && typeof d === 'object') {
        if ('detail' in d && (d as { detail?: string }).detail)
          text = String((d as { detail: string }).detail);
        else if ('title' in d && (d as { title?: string }).title)
          text = String((d as { title: string }).title);
      }
      setError(text);
    } finally {
      setSubmitting(false);
    }
  };

  if (successOrder) {
    return (
      <div className="checkout-page">
        <div className="checkout-inner">
          <h1>Заказ оформлен</h1>
          <p className="checkout-success">
            Номер заказа: <strong>{successOrder}</strong>
          </p>
          <Link to="/courses" className="btn btn-primary">
            К каталогу
          </Link>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className="checkout-page">
        <div className="checkout-inner">
          <h1>Оформление заказа</h1>
          <div className="checkout-gate">
            <p className="checkout-gate-text">
              Для оформления заказа необходимо <strong>зарегистрироваться на платформе</strong> (или войти в
              существующий аккаунт).
            </p>
            <Link to="/register" className="btn btn-primary btn-full checkout-gate-primary">
              Зарегистрироваться
            </Link>
            <p className="checkout-gate-sub">
              Уже зарегистрированы? <Link to="/login">Войти</Link>
            </p>
            <Link to="/cart" className="checkout-gate-back">
              ← Вернуться в корзину
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="checkout-page">
      <div className="checkout-inner">
        <h1>Оформление заказа</h1>

        {error && <div className="checkout-error">{error}</div>}

        {promoFromCart && (
          <p className="checkout-promo-hint">
            Промокод: <strong>{promoFromCart}</strong>
          </p>
        )}

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
                  <MaskedPhoneInput id="phone" valueDigits={phoneDigits} onDigitsChange={setPhoneDigits} />
                </div>
                <div className="form-group">
                  <label htmlFor="email">Email</label>
                  <input
                    id="email"
                    type="text"
                    inputMode="email"
                    autoComplete="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="user@mail.com"
                  />
                </div>
              </div>
            </div>

            <div className="checkout-card">
              <h2>Способ оплаты</h2>

              {methodsLoadError && <p className="checkout-methods-error">{methodsLoadError}</p>}

              {loadingMethods ? (
                <p className="checkout-methods-loading">Загрузка способов оплаты...</p>
              ) : methods.length === 0 ? (
                <p className="checkout-methods-empty">
                  Способы оплаты не найдены в базе. Добавьте записи в таблицу способов оплаты или обратитесь к
                  администратору.
                </p>
              ) : (
                methods.map((m) => {
                  const active =
                    selectedMethod?.methodId === m.methodId &&
                    selectedMethod?.methodName === m.methodName;
                  return (
                    <div
                      key={`${m.methodId}-${m.methodName}`}
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
                        <div className="payment-title">{m.methodName}</div>
                        {active ? (
                          <div className="payment-subtitle">Выбран</div>
                        ) : m.description ? (
                          <div className="payment-subtitle payment-subtitle-muted">{m.description}</div>
                        ) : null}
                      </div>
                    </div>
                  );
                })
              )}

              {!loadingMethods && methods.length > 0 && selectedMethod && (
                <>
                  {showCardForm && (
                    <div className="checkout-payment-extra checkout-card-fields">
                      <h3 className="checkout-payment-extra-title">Данные карты</h3>
                      <div className="form-group checkout-card-full">
                        <label htmlFor="card-holder">Имя на карте</label>
                        <input
                          id="card-holder"
                          type="text"
                          autoComplete="cc-name"
                          value={cardHolder}
                          onChange={(e) => setCardHolder(e.target.value.toUpperCase())}
                          placeholder="IVAN IVANOV"
                        />
                      </div>
                      <div className="form-group checkout-card-full">
                        <label htmlFor="card-number">Номер карты</label>
                        <input
                          id="card-number"
                          type="text"
                          inputMode="numeric"
                          autoComplete="cc-number"
                          value={cardNumber}
                          onChange={(e) => setCardNumber(formatCardNumberInput(e.target.value))}
                          placeholder="0000 0000 0000 0000"
                        />
                      </div>
                      <div className="form-row checkout-card-row-expiry">
                        <div className="form-group">
                          <label htmlFor="card-expiry">Срок (ММ/ГГ)</label>
                          <input
                            id="card-expiry"
                            type="text"
                            inputMode="numeric"
                            autoComplete="cc-exp"
                            value={cardExpiry}
                            onChange={(e) => setCardExpiry(formatCardExpiryInput(e.target.value))}
                            placeholder="ММ/ГГ"
                          />
                        </div>
                        <div className="form-group">
                          <label htmlFor="card-cvc">CVC / CVV</label>
                          <input
                            id="card-cvc"
                            type="password"
                            inputMode="numeric"
                            autoComplete="cc-csc"
                            maxLength={4}
                            value={cardCvc}
                            onChange={(e) => setCardCvc(e.target.value.replace(/\D/g, '').slice(0, 4))}
                            placeholder="•••"
                          />
                        </div>
                      </div>
                      <p className="checkout-card-demo-hint">
                        Проверка формата данных карты на клиенте. Не передавайте реквизиты в незащищённых каналах.
                      </p>
                    </div>
                  )}

                  {showSbp && (
                    <div className="checkout-payment-extra checkout-sbp-block">
                      <h3 className="checkout-payment-extra-title">Оплата через СБП</h3>
                      <p className="checkout-sbp-text">Отсканируйте QR-код в приложении вашего банка.</p>
                      <div className="checkout-qr-box">
                        <QRCodeSVG value={sbpQrValue} size={200} level="M" includeMargin />
                      </div>
                      <p className="checkout-sbp-note">Сумма к оплате: {totalFormatted}. В бою сюда подставляется ссылка СБП от банка.</p>
                    </div>
                  )}

                  {showInstallmentFields && (
                    <div className="checkout-payment-extra checkout-installment-block">
                      <h3 className="checkout-payment-extra-title">Рассрочка</h3>
                      <p className="checkout-installment-text">
                        Выберите число платежей. План и график будут созданы на сервере после оформления заказа.
                      </p>
                      <div className="form-group">
                        <label htmlFor="installment-count">Число платежей</label>
                        <select
                          id="installment-count"
                          value={installmentCount}
                          onChange={(e) => setInstallmentCount(Number(e.target.value))}
                        >
                          {[2, 3, 4, 5, 6, 9, 12].map((n) => (
                            <option key={n} value={n}>
                              {n}
                            </option>
                          ))}
                        </select>
                      </div>
                    </div>
                  )}
                </>
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
            <button
              className="btn btn-primary btn-full"
              type="button"
              disabled={
                submitting ||
                items.length === 0 ||
                loadingMethods ||
                methods.length === 0 ||
                !selectedMethod ||
                selectedMethod.methodId <= 0
              }
              onClick={() => void handlePay()}
            >
              {submitting ? 'Оформление...' : 'Оплатить заказ'}
            </button>
            <p className="checkout-hint">
              Нажимая кнопку, вы подтверждаете своё согласие с политикой конфиденциальности и договором оферты.
            </p>
          </aside>
        </div>
      </div>
    </div>
  );
};

export default CheckoutPage;

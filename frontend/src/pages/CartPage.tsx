import { Link, useNavigate } from 'react-router-dom';
import { useEffect, useMemo, useState } from 'react';
import { useCart } from '../contexts/CartContext';
import { useAuth } from '../contexts/AuthContext';
import { validatePromoCode } from '../api/promoCodes';
import './CartPage.css';

const CartPage = () => {
  const { items, total, removeFromCart } = useCart();
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [registerHint, setRegisterHint] = useState(false);
  const [promoCode, setPromoCode] = useState('');
  const [discountAmount, setDiscountAmount] = useState(0);
  const [promoMessage, setPromoMessage] = useState<string | null>(null);
  const [isApplyingPromo, setIsApplyingPromo] = useState(false);
  const [appliedPromoCode, setAppliedPromoCode] = useState<string | null>(null);

  const itemsCount = items.length;

  const finalTotal = useMemo(() => Math.max(0, total - discountAmount), [total, discountAmount]);
  const finalTotalFormatted = useMemo(() => `${finalTotal} ₽`, [finalTotal]);
  const itemsTotalWithDiscountFormatted = useMemo(() => `${finalTotal} ₽`, [finalTotal]);
  const originalTotalFormatted = useMemo(() => `${total} ₽`, [total]);
  const discountFormatted = useMemo(() => `${discountAmount} ₽`, [discountAmount]);

  useEffect(() => {
    // Если пользователь поменял корзину — скидку нужно пересчитать заново.
    const t = window.setTimeout(() => {
      setDiscountAmount(0);
      setPromoMessage(null);
      setAppliedPromoCode(null);
    }, 0);

    return () => window.clearTimeout(t);
  }, [total]);

  useEffect(() => {
    if (isAuthenticated) setRegisterHint(false);
  }, [isAuthenticated]);

  const handleApplyPromo = () => {
    (async () => {
      setPromoMessage(null);
      const code = promoCode.trim();
      
      if (!code) {
        setDiscountAmount(0);
        setPromoMessage('Введите промокод');
        return;
      }

      if (appliedPromoCode === code) {
        setPromoMessage('Этот промокод уже применён');
        return;
      }

      setIsApplyingPromo(true);

      try {
        const res = await validatePromoCode(code, total);
        if (res.isValid) {
          setDiscountAmount(res.discountAmount);
          setPromoMessage(res.message ?? 'Промокод применён');
          setAppliedPromoCode(code);
          setPromoCode('');
        } else {
          setDiscountAmount(0);
          setPromoMessage(res.message ?? 'Промокод недействителен');
          setAppliedPromoCode(null);
        }
      } catch {
        setDiscountAmount(0);
        setPromoMessage('Не удалось применить промокод');
        setAppliedPromoCode(null);
      } finally {
        setIsApplyingPromo(false);
      }
    })();
  };

  const handleRemovePromo = () => {
    setDiscountAmount(0);
    setPromoMessage(null);
    setAppliedPromoCode(null);
    setPromoCode('');
  };

  const handleCheckoutClick = () => {
    setRegisterHint(false);
    if (itemsCount === 0) return;
    if (!isAuthenticated) {
      setRegisterHint(true);
      return;
    }
    navigate('/checkout', { state: { promoCode: appliedPromoCode } });
  };

  return (
    <div className="cart-page">
      <div className="cart-inner">
        <h1>Корзина</h1>

        <div className="cart-layout">
          <div className="cart-items">
            {itemsCount === 0 ? (
              <p className="cart-empty">Корзина пуста</p>
            ) : (
              items.map((it) => (
                <div key={it.courseId} className="cart-item">
                  <div className="cart-item-info">
                    <div className="cart-item-title">{it.title}</div>
                    <div className="cart-item-subtitle">Курс</div>
                  </div>
                  <div className="cart-item-actions">
                    <div className="cart-item-price">{it.price} ₽</div>
                    <button
                      type="button"
                      className="cart-remove-btn"
                      onClick={() => removeFromCart(it.courseId)}
                      aria-label={`Удалить ${it.title}`}
                    >
                      Удалить
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>

          <aside className="cart-summary">
            <h2>Итого</h2>

            <div className="cart-summary-row">
              <span>Товары ({itemsCount})</span>
              <span>{itemsTotalWithDiscountFormatted}</span>
            </div>

            {discountAmount > 0 && (
              <>
                <div className="cart-summary-row discount-row">
                  <span>Скидка по промокоду</span>
                  <span className="discount-value">-{discountFormatted}</span>
                </div>

                <div className="cart-summary-row original-price-row">
                  <span>Было</span>
                  <span className="original-price">{originalTotalFormatted}</span>
                </div>
              </>
            )}

            <div className="cart-promo">
              <div className="cart-promo-label">Промокод</div>
              <div className="cart-promo-actions">
                <input
                  className="cart-promo-input"
                  value={promoCode}
                  onChange={(e) => setPromoCode(e.target.value)}
                  placeholder="Введите промокод"
                  disabled={isApplyingPromo || !!appliedPromoCode}
                />
                {!appliedPromoCode ? (
                  <button 
                    type="button" 
                    className="cart-apply-btn" 
                    onClick={handleApplyPromo}
                    disabled={isApplyingPromo}
                  >
                    {isApplyingPromo ? 'Проверка...' : 'Применить промокод'}
                  </button>
                ) : (
                  <button 
                    type="button" 
                    className="cart-apply-btn cart-remove-promo-btn"
                    onClick={handleRemovePromo}
                  >
                    Отменить промокод
                  </button>
                )}
              </div>
              {promoMessage && <div className="cart-promo-message">{promoMessage}</div>}
            </div>

            <div className="cart-summary-total">
              <span>К оплате</span>
              <span>{finalTotalFormatted}</span>
            </div>

            {registerHint && (
              <div className="cart-register-hint" role="alert">
                <p>
                  Для оформления заказа необходимо{' '}
                  <strong>зарегистрироваться на платформе</strong>.
                </p>
                <Link to="/register" className="btn btn-primary btn-full cart-register-link">
                  Перейти к регистрации
                </Link>
              </div>
            )}

            <button
              type="button"
              className="btn btn-primary btn-full"
              onClick={handleCheckoutClick}
              disabled={itemsCount === 0}
            >
              Оформить заказ
            </button>
          </aside>
        </div>
      </div>
    </div>
  );
};

export default CartPage;
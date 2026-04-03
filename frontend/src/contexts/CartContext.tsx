/* eslint-disable react-refresh/only-export-components */
import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import type { Course } from '../types';

type CartItem = {
  courseId: number;
  title: string;
  price: number;
  imageUrl?: string;
};

type CartContextType = {
  items: CartItem[];
  total: number;
  addToCart: (course: Course) => void;
  removeFromCart: (courseId: number) => void;
  clearCart: () => void;
};

const CartContext = createContext<CartContextType | undefined>(undefined);

const CART_KEY = 'cartItems';

function safeParseCartItems(raw: string | null): CartItem[] {
  if (!raw) return [];
  try {
    const data = JSON.parse(raw) as CartItem[];
    if (!Array.isArray(data)) return [];
    return data
      .filter((x) => typeof x?.courseId === 'number')
      .map((x) => ({
        courseId: x.courseId,
        title: typeof x.title === 'string' ? x.title : 'Курс',
        price: typeof x.price === 'number' ? x.price : 0,
        imageUrl: typeof x.imageUrl === 'string' ? x.imageUrl : undefined,
      }));
  } catch {
    return [];
  }
}

export const useCart = () => {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error('useCart must be used within CartProvider');
  return ctx;
};

export const CartProvider = ({ children }: { children: ReactNode }) => {
  const [items, setItems] = useState<CartItem[]>(() =>
    safeParseCartItems(localStorage.getItem(CART_KEY)),
  );

  useEffect(() => {
    localStorage.setItem(CART_KEY, JSON.stringify(items));
  }, [items]);

  const total = useMemo(() => items.reduce((sum, x) => sum + (x.price ?? 0), 0), [items]);

  const addToCart = (course: Course) => {
    if (!course?.id) return;
    const courseId = course.id;
    const price = typeof course.price === 'number' ? course.price : 0;

    setItems((prev) => {
      const exists = prev.some((x) => x.courseId === courseId);
      if (exists) return prev;
      return [
        ...prev,
        {
          courseId,
          title: course.title,
          price,
          imageUrl: course.imageUrl,
        },
      ];
    });
  };

  const removeFromCart = (courseId: number) => {
    setItems((prev) => prev.filter((x) => x.courseId !== courseId));
  };

  const clearCart = () => setItems([]);

  const value: CartContextType = { items, total, addToCart, removeFromCart, clearCart };

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
};


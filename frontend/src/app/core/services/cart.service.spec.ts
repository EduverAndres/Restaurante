import { describe, it, expect, beforeEach } from 'vitest';
import { CartService } from './cart.service';
import { MenuItem, Restaurant } from './restaurant.service';

const CART_KEY = 'restaurante_cart';
const RESTAURANT_KEY = 'restaurante_cart_restaurant';

function makeItem(id: string, restaurantId = 'r1', price = 10): MenuItem {
  return {
    id,
    restaurantId,
    categoryId: 'c1',
    name: `Item ${id}`,
    description: '',
    price,
    isAvailable: true,
    displayOrder: 0,
  };
}

function makeRestaurant(id = 'r1', name = 'Resto A', slug = 'resto-a'): Restaurant {
  return {
    id,
    name,
    slug,
    description: '',
    themeConfig: {} as Restaurant['themeConfig'],
    isActive: true,
    ownerId: 'o1',
    createdAt: '2026-01-01T00:00:00Z',
    deliveryFee: 5,
    minOrderAmount: 20,
  };
}

describe('CartService', () => {
  let service: CartService;

  beforeEach(() => {
    localStorage.clear();
    service = new CartService();
  });

  it('adds a new item with quantity and notes', () => {
    service.setRestaurant(makeRestaurant());
    service.addItem(makeItem('m1'), 2, 'sin cebolla');

    expect(service.items()).toHaveLength(1);
    expect(service.items()[0].menuItem.id).toBe('m1');
    expect(service.items()[0].quantity).toBe(2);
    expect(service.items()[0].notes).toBe('sin cebolla');
  });

  it('increments quantity when adding an existing item', () => {
    service.setRestaurant(makeRestaurant());
    service.addItem(makeItem('m1'));
    service.addItem(makeItem('m1'), 2);

    expect(service.items()).toHaveLength(1);
    expect(service.items()[0].quantity).toBe(3);
  });

  it('adds different items separately', () => {
    service.setRestaurant(makeRestaurant());
    service.addItem(makeItem('m1'));
    service.addItem(makeItem('m2'));

    expect(service.items()).toHaveLength(2);
  });

  it('caps item quantity at 99', () => {
    service.setRestaurant(makeRestaurant());
    service.addItem(makeItem('m1'), 150);

    expect(service.items()[0].quantity).toBe(99);
  });

  it('updates the quantity of an item', () => {
    service.setRestaurant(makeRestaurant());
    service.addItem(makeItem('m1'));
    service.updateQuantity('m1', 4);

    expect(service.items()[0].quantity).toBe(4);
  });

  it('removes an item when quantity reaches 0', () => {
    service.setRestaurant(makeRestaurant());
    service.addItem(makeItem('m1'));
    service.updateQuantity('m1', 0);

    expect(service.items()).toHaveLength(0);
  });

  it('removes an item by id', () => {
    service.setRestaurant(makeRestaurant());
    service.addItem(makeItem('m1'));
    service.addItem(makeItem('m2'));
    service.removeItem('m1');

    expect(service.items().map(i => i.menuItem.id)).toEqual(['m2']);
  });

  it('clears items and coupon code', () => {
    service.setRestaurant(makeRestaurant());
    service.addItem(makeItem('m1'));
    service.setCouponCode('WELCOME10');

    service.clear();

    expect(service.items()).toHaveLength(0);
    expect(service.couponCode()).toBeNull();
    expect(service.count()).toBe(0);
  });

  it('clears the cart when switching to a different restaurant with items', () => {
    service.setRestaurant(makeRestaurant('r1'));
    service.addItem(makeItem('m1', 'r1'));

    service.setRestaurant(makeRestaurant('r2', 'Resto B', 'resto-b'));

    expect(service.items()).toHaveLength(0);
    expect(service.restaurant()?.id).toBe('r2');
  });

  it('clears the cart when adding an item from a different restaurant', () => {
    service.setRestaurant(makeRestaurant('r1'));
    service.addItem(makeItem('m1', 'r1'));

    service.addItem(makeItem('m2', 'r2'));

    expect(service.items()).toHaveLength(1);
    expect(service.items()[0].menuItem.id).toBe('m2');
  });

  it('keeps items when setRestaurant is called for the same restaurant', () => {
    service.setRestaurant(makeRestaurant('r1'));
    service.addItem(makeItem('m1', 'r1'));

    service.setRestaurant(makeRestaurant('r1'));

    expect(service.items()).toHaveLength(1);
  });

  it('computes count and subtotal', () => {
    service.setRestaurant(makeRestaurant());
    service.addItem(makeItem('m1', 'r1', 10), 2);
    service.addItem(makeItem('m2', 'r1', 15.5));

    expect(service.count()).toBe(3);
    expect(service.subtotal()).toBeCloseTo(35.5);
  });

  it('exposes restaurantId from the active restaurant', () => {
    expect(service.restaurantId()).toBeNull();
    service.setRestaurant(makeRestaurant('r1'));
    expect(service.restaurantId()).toBe('r1');
  });

  it('persists items and restaurant to localStorage', () => {
    service.setRestaurant(makeRestaurant('r1'));
    service.addItem(makeItem('m1', 'r1'), 3);

    const storedItems = JSON.parse(localStorage.getItem(CART_KEY)!);
    const storedRestaurant = JSON.parse(localStorage.getItem(RESTAURANT_KEY)!);

    expect(storedItems).toHaveLength(1);
    expect(storedItems[0].quantity).toBe(3);
    expect(storedRestaurant.id).toBe('r1');
    expect(storedRestaurant.deliveryFee).toBe(5);
  });

  it('hydrates items and restaurant from localStorage on construction', () => {
    service.setRestaurant(makeRestaurant('r1'));
    service.addItem(makeItem('m1', 'r1'), 2);

    const restored = new CartService();

    expect(restored.items()).toHaveLength(1);
    expect(restored.items()[0].menuItem.id).toBe('m1');
    expect(restored.items()[0].quantity).toBe(2);
    expect(restored.restaurant()?.name).toBe('Resto A');
    expect(restored.count()).toBe(2);
  });

  it('recovers from corrupted localStorage data', () => {
    localStorage.setItem(CART_KEY, '{not valid json');
    localStorage.setItem(RESTAURANT_KEY, '{{{{');

    const restored = new CartService();

    expect(restored.items()).toHaveLength(0);
    expect(restored.restaurant()).toBeNull();
  });

  it('updates localStorage after mutations', () => {
    service.setRestaurant(makeRestaurant());
    service.addItem(makeItem('m1'));
    service.removeItem('m1');

    expect(JSON.parse(localStorage.getItem(CART_KEY)!)).toHaveLength(0);
  });

  it('stores coupon code and clears it', () => {
    service.setCouponCode('  WELCOME10  ');
    expect(service.couponCode()).toBe('WELCOME10');

    service.setCouponCode('');
    expect(service.couponCode()).toBeNull();
  });
});

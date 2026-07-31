import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { roleGuard } from './role.guard';
import { signal } from '@angular/core';

function mockAuth(overrides: Partial<{
  isLoggedIn: boolean;
  userRole: 'customer' | 'restaurant' | null;
}>) {
  return {
    isLoggedIn: signal(overrides.isLoggedIn ?? false),
    userRole: signal<'customer' | 'restaurant' | null>(overrides.userRole ?? null),
  };
}

describe('roleGuard', () => {
  let mockRouter: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    mockRouter = { navigate: vi.fn() };
  });

  function configureTest(authOverrides: Parameters<typeof mockAuth>[0]) {
    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthService, useValue: mockAuth(authOverrides) },
      ],
    });
  }

  describe('allowed role', () => {
    it('should return true when user has the allowed role', () => {
      configureTest({ isLoggedIn: true, userRole: 'customer' });
      const result = TestBed.runInInjectionContext(roleGuard(['customer']));
      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should return true when any role in the list matches', () => {
      configureTest({ isLoggedIn: true, userRole: 'restaurant' });
      const result = TestBed.runInInjectionContext(roleGuard(['customer', 'restaurant']));
      expect(result).toBe(true);
    });
  });

  describe('denied role', () => {
    it('should navigate to / and return false when user role is not in allowed list', () => {
      configureTest({ isLoggedIn: true, userRole: 'customer' });
      const result = TestBed.runInInjectionContext(roleGuard(['restaurant']));
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
    });
  });

  describe('unauthenticated', () => {
    it('should navigate to / and return false when user is not logged in', () => {
      configureTest({ isLoggedIn: false, userRole: null });
      const result = TestBed.runInInjectionContext(roleGuard(['customer']));
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
    });
  });
});

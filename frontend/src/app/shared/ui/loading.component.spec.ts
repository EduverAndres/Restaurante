import { describe, it, expect } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { LoadingComponent } from './loading.component';

describe('LoadingComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoadingComponent],
    }).compileComponents();
  });

  it('should render a spinner', () => {
    const fixture = TestBed.createComponent(LoadingComponent);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const spinner = el.querySelector('.animate-spin');
    expect(spinner).not.toBeNull();
  });
});

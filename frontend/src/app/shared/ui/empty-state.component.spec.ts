import { describe, it, expect } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { EmptyStateComponent } from './empty-state.component';

describe('EmptyStateComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmptyStateComponent],
    }).compileComponents();
  });

  it('should render the default message', () => {
    const fixture = TestBed.createComponent(EmptyStateComponent);
    fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent!;
    expect(text).toContain('No hay resultados');
  });

  it('should render a custom message', () => {
    const fixture = TestBed.createComponent(EmptyStateComponent);
    fixture.componentRef.setInput('message', 'No se encontraron pedidos');
    fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent!;
    expect(text).toContain('No se encontraron pedidos');
  });

  it('should render subtext when provided', () => {
    const fixture = TestBed.createComponent(EmptyStateComponent);
    fixture.componentRef.setInput('subtext', 'Intentá con otros filtros');
    fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent!;
    expect(text).toContain('Intentá con otros filtros');
  });
});

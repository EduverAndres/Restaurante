import { describe, it, expect } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ErrorStateComponent } from './error-state.component';

describe('ErrorStateComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ErrorStateComponent],
    }).compileComponents();
  });

  it('should render the error message', () => {
    const fixture = TestBed.createComponent(ErrorStateComponent);
    fixture.componentRef.setInput('error', 'Algo salió mal');
    fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent!;
    expect(text).toContain('Algo salió mal');
  });

  it('should emit retry when button is clicked', () => {
    const fixture = TestBed.createComponent(ErrorStateComponent);
    fixture.componentRef.setInput('error', 'Error');
    fixture.detectChanges();

    let emitted = false;
    fixture.componentInstance.retry.subscribe(() => (emitted = true));

    const btn = (fixture.nativeElement as HTMLElement).querySelector('button');
    expect(btn).not.toBeNull();
    btn!.click();

    expect(emitted).toBe(true);
  });

  it('should render retry button with text', () => {
    const fixture = TestBed.createComponent(ErrorStateComponent);
    fixture.componentRef.setInput('error', 'Error');
    fixture.detectChanges();
    const btn = (fixture.nativeElement as HTMLElement).querySelector('button');
    expect(btn?.textContent).toContain('Intentar de nuevo');
  });
});

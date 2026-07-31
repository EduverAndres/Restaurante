import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-error-state',
  standalone: true,
  template: `
    <div class="flex flex-col items-center justify-center p-12 text-center">
      <div class="text-red-400 mb-4">
        <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
            d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
        </svg>
      </div>
      <p class="text-gray-700 mb-4">{{ error }}</p>
      <button
        (click)="retry.emit()"
        class="px-4 py-2 bg-primary-500 text-white rounded-lg hover:bg-primary-600 transition-colors cursor-pointer"
      >
        Intentar de nuevo
      </button>
    </div>
  `,
})
export class ErrorStateComponent {
  @Input({ required: true }) error!: string;
  @Output() retry = new EventEmitter<void>();
}

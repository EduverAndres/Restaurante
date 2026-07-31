import { Component } from '@angular/core';

@Component({
  selector: 'app-loading',
  standalone: true,
  template: `
    <div class="flex items-center justify-center p-8">
      <div class="animate-spin rounded-full h-12 w-12 border-4 border-primary-200 border-t-primary-500"></div>
    </div>
  `,
})
export class LoadingComponent {}

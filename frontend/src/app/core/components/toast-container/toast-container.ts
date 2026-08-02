import { Component } from '@angular/core';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast-container',
  imports: [],
  templateUrl: './toast-container.html',
  styleUrl: './toast-container.css',
})
export class ToastContainer {
  constructor(protected toastService: ToastService) {}

  toastClass(type: string): string {
    const map: Record<string, string> = {
      success: 'bg-green-600',
      error: 'bg-red-600',
      info: 'bg-gray-800',
    };
    return map[type] || map['info'];
  }

  dismissToast(id: number): void {
    this.toastService.dismiss(id);
  }
}

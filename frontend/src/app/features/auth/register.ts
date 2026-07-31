import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [RouterLink, FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  name = '';
  email = '';
  password = '';
  confirmPassword = '';
  phone = '';
  role: 'customer' | 'restaurant' = 'customer';
  error = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit(): void {
    if (!this.name || !this.email || !this.password || !this.phone) {
      this.error = 'Todos los campos son obligatorios';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.error = 'Las contraseñas no coinciden';
      return;
    }

    if (this.password.length < 6) {
      this.error = 'La contraseña debe tener al menos 6 caracteres';
      return;
    }

    this.loading = true;
    this.error = '';

    this.auth.register(this.name, this.email, this.password, this.phone, this.role).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.user.role === 'restaurant') {
          this.router.navigate(['/restaurant/dashboard']);
        } else {
          this.router.navigate(['/browse']);
        }
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Error al registrar. Intenta de nuevo.';
      },
    });
  }
}

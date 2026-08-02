import { Component } from '@angular/core';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [RouterLink, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  email = '';
  password = '';
  error = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router, private route: ActivatedRoute) {}

  onSubmit(): void {
    if (!this.email || !this.password) {
      this.error = 'Todos los campos son obligatorios';
      return;
    }

    this.loading = true;
    this.error = '';

    this.auth.login(this.email, this.password).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.user.role === 'restaurant') {
          this.router.navigate(['/restaurant/dashboard']);
        } else {
          const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
          this.router.navigateByUrl(returnUrl || '/browse');
        }
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Error al iniciar sesión. Verifica tus credenciales.';
      },
    });
  }
}

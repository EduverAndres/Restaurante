import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService, User } from '../../../core/services/auth.service';
import {
  AddressService,
  CustomerAddress,
  CreateAddressRequest,
} from '../../../core/services/address.service';
import { ToastService } from '../../../core/services/toast.service';

interface AddressFormState {
  label: string;
  address: string;
  latitude: number | null;
  longitude: number | null;
  isDefault: boolean;
}

const EMPTY_FORM: AddressFormState = {
  label: '',
  address: '',
  latitude: null,
  longitude: null,
  isDefault: false,
};

@Component({
  selector: 'app-customer-profile',
  imports: [FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class CustomerProfile implements OnInit {
  user = signal<User | null>(null);
  addresses: CustomerAddress[] = [];
  loadingAddresses = true;
  showAddForm = false;
  editingId: string | null = null;
  saving = false;

  form: AddressFormState = { ...EMPTY_FORM };

  constructor(
    private auth: AuthService,
    private addressService: AddressService,
    private toast: ToastService,
  ) {}

  ngOnInit(): void {
    this.user.set(this.auth.currentUser());
    this.loadAddresses();
  }

  roleLabel(): string {
    return this.user()?.role === 'restaurant' ? 'Restaurante' : 'Cliente';
  }

  logout(): void {
    this.auth.logout();
  }

  /* ---------- Addresses ---------- */

  private loadAddresses(): void {
    this.loadingAddresses = true;
    this.addressService.getAddresses().subscribe({
      next: (data) => {
        this.addresses = data;
        this.loadingAddresses = false;
      },
      error: () => {
        this.loadingAddresses = false;
        this.toast.show('No se pudieron cargar las direcciones', 'error');
      },
    });
  }

  startAdd(): void {
    this.editingId = null;
    this.form = { ...EMPTY_FORM };
    this.showAddForm = true;
  }

  startEdit(address: CustomerAddress): void {
    this.showAddForm = false;
    this.editingId = address.id;
    this.form = {
      label: address.label,
      address: address.address,
      latitude: address.latitude ?? null,
      longitude: address.longitude ?? null,
      isDefault: address.isDefault,
    };
  }

  cancelForm(): void {
    this.showAddForm = false;
    this.editingId = null;
    this.form = { ...EMPTY_FORM };
  }

  private toPayload(): CreateAddressRequest {
    return {
      label: this.form.label.trim(),
      address: this.form.address.trim(),
      latitude: this.form.latitude,
      longitude: this.form.longitude,
      isDefault: this.form.isDefault,
    };
  }

  private formValid(): boolean {
    if (!this.form.label.trim() || !this.form.address.trim()) {
      this.toast.show('Completá nombre y dirección', 'error');
      return false;
    }
    return true;
  }

  addAddress(): void {
    if (this.saving || !this.formValid()) return;
    this.saving = true;
    this.addressService.createAddress(this.toPayload()).subscribe({
      next: () => {
        this.saving = false;
        this.showAddForm = false;
        this.form = { ...EMPTY_FORM };
        this.toast.show('Dirección agregada', 'success');
        this.loadAddresses();
      },
      error: () => {
        this.saving = false;
        this.toast.show('No se pudo agregar la dirección', 'error');
      },
    });
  }

  saveEdit(): void {
    if (!this.editingId || this.saving || !this.formValid()) return;
    this.saving = true;
    this.addressService.updateAddress(this.editingId, this.toPayload()).subscribe({
      next: () => {
        this.saving = false;
        this.editingId = null;
        this.form = { ...EMPTY_FORM };
        this.toast.show('Dirección actualizada', 'success');
        this.loadAddresses();
      },
      error: () => {
        this.saving = false;
        this.toast.show('No se pudo actualizar la dirección', 'error');
      },
    });
  }

  deleteAddress(address: CustomerAddress): void {
    if (!window.confirm(`¿Eliminar la dirección "${address.label}"?`)) return;
    this.addressService.deleteAddress(address.id).subscribe({
      next: (res) => {
        if (res === false) {
          this.toast.show('No se pudo eliminar la dirección', 'error');
          return;
        }
        this.toast.show('Dirección eliminada', 'success');
        this.loadAddresses();
      },
      error: () => this.toast.show('No se pudo eliminar la dirección', 'error'),
    });
  }
}

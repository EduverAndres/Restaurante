import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { RestaurantService, MenuCategory, MenuItem } from '../../../core/services/restaurant.service';
import { readableApiError } from '../../../core/utils/restaurant-onboarding';

@Component({
  selector: 'app-menu-manager',
  imports: [FormsModule],
  templateUrl: './menu-manager.html',
  styleUrl: './menu-manager.css',
})
export class MenuManager implements OnInit {
  categories: MenuCategory[] = [];
  loading = true;
  restaurantId = '';

  editingCategory: MenuCategory | null = null;
  editingItem: MenuItem | null = null;
  addingCategory = false;
  addingItem = false;
  selectedCategoryId = '';

  categoryForm = { name: '', description: '' };
  itemForm = { name: '', description: '', price: 0, imageUrl: '', isAvailable: true, preparationTime: 15 };
  saving = false;
  error = '';
  togglingItemId = signal('');

  constructor(
    private auth: AuthService,
    private restaurantService: RestaurantService,
    private toast: ToastService,
  ) {}

  ngOnInit(): void {
    this.restaurantService.getByOwner().subscribe({
      next: (restaurants) => {
        if (restaurants.length > 0) {
          this.restaurantId = restaurants[0].id;
          this.loadMenu();
        } else {
          this.loading = false;
        }
      },
      error: () => {
        this.loading = false;
        this.error = 'No se pudieron cargar tus restaurantes';
      },
    });
  }

  private loadMenu(): void {
    if (!this.restaurantId) {
      this.loading = false;
      return;
    }
    this.restaurantService.getMenu(this.restaurantId).subscribe({
      next: (data) => {
        this.categories = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'No se pudo cargar el menú';
      },
    });
  }

  startAddCategory(): void {
    this.addingCategory = true;
    this.editingCategory = null;
    this.categoryForm = { name: '', description: '' };
  }

  startEditCategory(cat: MenuCategory): void {
    this.editingCategory = cat;
    this.addingCategory = false;
    this.categoryForm = { name: cat.name, description: cat.description || '' };
  }

  cancelCategory(): void {
    this.addingCategory = false;
    this.editingCategory = null;
    this.categoryForm = { name: '', description: '' };
  }

  saveCategory(): void {
    if (!this.categoryForm.name.trim()) return;
    this.saving = true;
    this.error = '';

    if (this.editingCategory) {
      this.restaurantService.updateCategory(this.restaurantId, this.editingCategory.id, this.categoryForm).subscribe({
        next: () => { this.loadMenu(); this.cancelCategory(); this.saving = false; },
        error: (e) => { this.error = readableApiError(e, 'Error al guardar'); this.saving = false; },
      });
    } else {
      this.restaurantService.createCategory(this.restaurantId, this.categoryForm).subscribe({
        next: () => { this.loadMenu(); this.cancelCategory(); this.saving = false; },
        error: (e) => { this.error = readableApiError(e, 'Error al guardar'); this.saving = false; },
      });
    }
  }

  deleteCategory(id: string): void {
    if (!confirm('¿Eliminar esta categoría y todos sus items?')) return;
    this.restaurantService.deleteCategory(this.restaurantId, id).subscribe({
      next: () => this.loadMenu(),
      error: (e) => this.toast.show(readableApiError(e, 'No se pudo eliminar la categoría'), 'error'),
    });
  }

  selectCategory(id: string): void {
    this.selectedCategoryId = id;
    this.addingItem = false;
    this.editingItem = null;
  }

  startAddItem(): void {
    this.addingItem = true;
    this.editingItem = null;
    this.itemForm = { name: '', description: '', price: 0, imageUrl: '', isAvailable: true, preparationTime: 15 };
  }

  startEditItem(item: MenuItem): void {
    this.editingItem = item;
    this.addingItem = false;
    this.itemForm = {
      name: item.name,
      description: item.description,
      price: item.price,
      imageUrl: item.images?.[0] || item.imageUrl || '',
      isAvailable: item.isAvailable,
      preparationTime: item.preparationTime || 15,
    };
  }

  cancelItem(): void {
    this.addingItem = false;
    this.editingItem = null;
    this.itemForm = { name: '', description: '', price: 0, imageUrl: '', isAvailable: true, preparationTime: 15 };
  }

  saveItem(): void {
    if (!this.itemForm.name.trim() || this.itemForm.price <= 0) return;
    this.saving = true;
    this.error = '';

    const payload: any = {
      name: this.itemForm.name,
      description: this.itemForm.description,
      price: this.itemForm.price,
      images: this.itemForm.imageUrl ? [this.itemForm.imageUrl] : [],
      isAvailable: this.itemForm.isAvailable,
      categoryId: this.selectedCategoryId,
      preparationTime: this.itemForm.preparationTime,
    };

    if (this.editingItem) {
      this.restaurantService.updateMenuItem(this.restaurantId, this.editingItem.id, payload).subscribe({
        next: () => { this.loadMenu(); this.cancelItem(); this.saving = false; },
        error: (e) => { this.error = readableApiError(e, 'Error al guardar item'); this.saving = false; },
      });
    } else {
      this.restaurantService.createMenuItem(this.restaurantId, payload).subscribe({
        next: () => { this.loadMenu(); this.cancelItem(); this.saving = false; },
        error: (e) => { this.error = readableApiError(e, 'Error al guardar item'); this.saving = false; },
      });
    }
  }

  deleteItem(itemId: string): void {
    if (!confirm('¿Eliminar este item?')) return;
    this.restaurantService.deleteMenuItem(this.restaurantId, itemId).subscribe({
      next: () => this.loadMenu(),
      error: (e) => this.toast.show(readableApiError(e, 'No se pudo eliminar el item'), 'error'),
    });
  }

  toggleAvailability(item: MenuItem): void {
    if (this.togglingItemId()) return;
    const next = !item.isAvailable;
    this.togglingItemId.set(item.id);
    this.restaurantService.updateItemAvailability(this.restaurantId, item.id, next).subscribe({
      next: () => {
        item.isAvailable = next;
        this.togglingItemId.set('');
        this.toast.show(next ? 'Item disponible' : 'Item agotado', 'success');
      },
      error: (err) => {
        this.togglingItemId.set('');
        this.toast.show(readableApiError(err, 'No se pudo actualizar la disponibilidad'), 'error');
      },
    });
  }

  get selectedCategory(): MenuCategory | undefined {
    return this.categories.find(c => c.id === this.selectedCategoryId);
  }
}
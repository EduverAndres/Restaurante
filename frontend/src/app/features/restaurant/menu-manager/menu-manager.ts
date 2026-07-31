import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { RestaurantService, MenuCategory, MenuItem } from '../../../core/services/restaurant.service';

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
  itemForm = { name: '', description: '', price: 0, imageUrl: '', isAvailable: true };
  saving = false;
  error = '';

  constructor(
    private auth: AuthService,
    private restaurantService: RestaurantService,
  ) {}

  ngOnInit(): void {
    this.restaurantId = this.auth.currentUser()?.id || '';
    this.loadMenu();
  }

  private loadMenu(): void {
    if (!this.restaurantId) return;
    this.restaurantService.getMenu(this.restaurantId).subscribe({
      next: (data) => {
        this.categories = data;
        this.loading = false;
      },
      error: () => (this.loading = false),
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
        error: (e) => { this.error = 'Error al guardar'; this.saving = false; },
      });
    } else {
      this.restaurantService.createCategory(this.restaurantId, this.categoryForm).subscribe({
        next: () => { this.loadMenu(); this.cancelCategory(); this.saving = false; },
        error: (e) => { this.error = 'Error al guardar'; this.saving = false; },
      });
    }
  }

  deleteCategory(id: string): void {
    if (!confirm('¿Eliminar esta categoría y todos sus items?')) return;
    this.restaurantService.deleteCategory(this.restaurantId, id).subscribe({
      next: () => this.loadMenu(),
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
    this.itemForm = { name: '', description: '', price: 0, imageUrl: '', isAvailable: true };
  }

  startEditItem(item: MenuItem): void {
    this.editingItem = item;
    this.addingItem = false;
    this.itemForm = { name: item.name, description: item.description, price: item.price, imageUrl: item.imageUrl || '', isAvailable: item.isAvailable };
  }

  cancelItem(): void {
    this.addingItem = false;
    this.editingItem = null;
    this.itemForm = { name: '', description: '', price: 0, imageUrl: '', isAvailable: true };
  }

  saveItem(): void {
    if (!this.itemForm.name.trim() || this.itemForm.price <= 0) return;
    this.saving = true;
    this.error = '';

    if (this.editingItem) {
      this.restaurantService.updateMenuItem(this.restaurantId, this.editingItem.id, this.itemForm).subscribe({
        next: () => { this.loadMenu(); this.cancelItem(); this.saving = false; },
        error: (e) => { this.error = 'Error al guardar item'; this.saving = false; },
      });
    } else if (this.selectedCategoryId) {
      this.restaurantService.createMenuItem(this.restaurantId, this.selectedCategoryId, this.itemForm).subscribe({
        next: () => { this.loadMenu(); this.cancelItem(); this.saving = false; },
        error: (e) => { this.error = 'Error al guardar item'; this.saving = false; },
      });
    }
  }

  deleteItem(itemId: string): void {
    if (!confirm('¿Eliminar este item?')) return;
    this.restaurantService.deleteMenuItem(this.restaurantId, itemId).subscribe({
      next: () => this.loadMenu(),
    });
  }

  toggleAvailability(item: MenuItem): void {
    this.restaurantService.updateMenuItem(this.restaurantId, item.id, { isAvailable: !item.isAvailable }).subscribe({
      next: () => this.loadMenu(),
    });
  }

  get selectedCategory(): MenuCategory | undefined {
    return this.categories.find(c => c.id === this.selectedCategoryId);
  }
}

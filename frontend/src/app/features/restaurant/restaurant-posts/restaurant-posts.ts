import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { RestaurantService, Restaurant, MenuCategory, MenuItem } from '../../../core/services/restaurant.service';

export interface RestaurantPost {
  id: string;
  restaurantId: string;
  title: string;
  description: string;
  imageUrl: string;
  price: number;
  category: string;
  isPublished: boolean;
  createdAt: string;
  likes: number;
}

@Component({
  selector: 'app-restaurant-posts',
  imports: [FormsModule],
  templateUrl: './restaurant-posts.html',
  styleUrl: './restaurant-posts.css',
})
export class RestaurantPosts implements OnInit {
  restaurant: Restaurant | null = null;
  categories: MenuCategory[] = [];
  loading = true;
  saving = false;

  // Post form
  showPostForm = false;
  editingPost: RestaurantPost | null = null;
  postForm = {
    title: '',
    description: '',
    imageUrl: '',
    price: 0,
    categoryId: '',
    isPublished: true,
  };

  // Posts stored locally (in a real app, this would be from the backend)
  posts: RestaurantPost[] = [];
  selectedFilter = 'all';

  error = '';
  successMessage = '';

  constructor(
    protected auth: AuthService,
    private restaurantService: RestaurantService,
  ) {}

  ngOnInit(): void {
    this.loadRestaurant();
  }

  private loadRestaurant(): void {
    this.restaurantService.getByOwner().subscribe({
      next: (restaurants) => {
        if (restaurants.length > 0) {
          this.restaurant = restaurants[0];
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
    const restaurantId = this.restaurant?.id;
    if (!restaurantId || restaurantId === 'undefined' || restaurantId === 'null') {
      this.loading = false;
      return;
    }
    this.restaurantService.getMenu(restaurantId).subscribe({
      next: (data) => {
        this.categories = data;
        this.buildPostsFromMenu();
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'No se pudo cargar el menú';
      },
    });
  }

  private buildPostsFromMenu(): void {
    // Build posts from menu items
    this.posts = [];
    for (const cat of this.categories) {
      for (const item of cat.items) {
        this.posts.push({
          id: item.id,
          restaurantId: this.restaurant!.id,
          title: item.name,
          description: item.description,
          imageUrl: item.images?.[0] || item.imageUrl || '',
          price: item.price,
          category: cat.name,
          isPublished: item.isAvailable,
          createdAt: new Date().toISOString(),
          likes: Math.floor(Math.random() * 50),
        });
      }
    }
  }

  get filteredPosts(): RestaurantPost[] {
    if (this.selectedFilter === 'all') return this.posts;
    if (this.selectedFilter === 'published') return this.posts.filter(p => p.isPublished);
    if (this.selectedFilter === 'unpublished') return this.posts.filter(p => !p.isPublished);
    return this.posts.filter(p => p.category === this.selectedFilter);
  }

  get uniqueCategories(): string[] {
    return [...new Set(this.posts.map(p => p.category))];
  }

  startAddPost(): void {
    this.showPostForm = true;
    this.editingPost = null;
    this.postForm = {
      title: '',
      description: '',
      imageUrl: '',
      price: 0,
      categoryId: this.categories.length > 0 ? this.categories[0].id : '',
      isPublished: true,
    };
  }

  startEditPost(post: RestaurantPost): void {
    this.editingPost = post;
    this.showPostForm = true;
    const cat = this.categories.find(c => c.name === post.category);
    this.postForm = {
      title: post.title,
      description: post.description,
      imageUrl: post.imageUrl,
      price: post.price,
      categoryId: cat?.id || '',
      isPublished: post.isPublished,
    };
  }

  cancelPost(): void {
    this.showPostForm = false;
    this.editingPost = null;
  }

  savePost(): void {
    if (!this.postForm.title.trim() || !this.restaurant) return;
    this.saving = true;
    this.error = '';

    const cat = this.categories.find(c => c.id === this.postForm.categoryId);
    const categoryName = cat?.name || 'Sin categoría';

    if (this.editingPost) {
      // Update existing post
      const idx = this.posts.findIndex(p => p.id === this.editingPost!.id);
      if (idx >= 0) {
        this.posts[idx] = {
          ...this.posts[idx],
          title: this.postForm.title,
          description: this.postForm.description,
          imageUrl: this.postForm.imageUrl,
          price: this.postForm.price,
          category: categoryName,
          isPublished: this.postForm.isPublished,
        };
      }
      this.successMessage = 'Publicación actualizada';
    } else {
      // Create new post
      const newPost: RestaurantPost = {
        id: Date.now().toString(),
        restaurantId: this.restaurant.id,
        title: this.postForm.title,
        description: this.postForm.description,
        imageUrl: this.postForm.imageUrl,
        price: this.postForm.price,
        category: categoryName,
        isPublished: this.postForm.isPublished,
        createdAt: new Date().toISOString(),
        likes: 0,
      };
      this.posts.unshift(newPost);
      this.successMessage = 'Publicación creada';
    }

    this.saving = false;
    this.cancelPost();
    setTimeout(() => (this.successMessage = ''), 3000);
  }

  deletePost(post: RestaurantPost): void {
    if (!confirm('¿Eliminar esta publicación?')) return;
    this.posts = this.posts.filter(p => p.id !== post.id);
    this.successMessage = 'Publicación eliminada';
    setTimeout(() => (this.successMessage = ''), 3000);
  }

  togglePublish(post: RestaurantPost): void {
    post.isPublished = !post.isPublished;
  }

  setFilter(filter: string): void {
    this.selectedFilter = filter;
  }
}
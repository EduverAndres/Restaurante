import { Restaurant } from '../services/restaurant.service';

/**
 * Restaurantes de ejemplo usados SOLO como contenido de respaldo (fallback)
 * mientras el backend no tenga restaurantes reales cargados. En cuanto la
 * API devuelva datos, estos se ignoran automáticamente.
 *
 * Las imágenes vienen de Lorem Picsum (picsum.photos), un servicio de fotos
 * de stock de uso libre pensado exactamente para este propósito de
 * placeholder/demo. Reemplázalas por `coverImageUrl` / `logoUrl` reales
 * (por ejemplo desde Supabase Storage) cuando cada restaurante suba sus
 * propias fotos.
 *
 * Se usa en: landing (home) y browse-restaurants (explorar).
 */
export const SAMPLE_RESTAURANTS: Restaurant[] = [
  {
    id: 'sample-1',
    name: 'La Trattoria de Marco',
    slug: 'la-trattoria-de-marco',
    description: 'Pastas artesanales y pizzas al horno de leña, receta tradicional italiana.',
    logoUrl: 'https://picsum.photos/seed/trattoria-logo/100/100',
    coverImageUrl: 'https://picsum.photos/seed/trattoria-marco/600/400',
    themeConfig: { primaryColor: '#e11d2e', secondaryColor: '#171717', accentColor: '#ffd500', backgroundColor: '#ffffff', textColor: '#171717', fontFamily: 'Inter' },
    isActive: true,
    ownerId: 'sample-owner-1',
    createdAt: new Date().toISOString(),
  },
  {
    id: 'sample-2',
    name: 'Sushi Zen',
    slug: 'sushi-zen',
    description: 'Rolls frescos y cortes premium de cocina japonesa preparados al momento por nuestros itamae.',
    logoUrl: 'https://picsum.photos/seed/sushizen-logo/100/100',
    coverImageUrl: 'https://picsum.photos/seed/sushi-zen/600/400',
    themeConfig: { primaryColor: '#e11d2e', secondaryColor: '#171717', accentColor: '#ffd500', backgroundColor: '#ffffff', textColor: '#171717', fontFamily: 'Inter' },
    isActive: true,
    ownerId: 'sample-owner-2',
    createdAt: new Date().toISOString(),
  },
  {
    id: 'sample-3',
    name: 'Burger Town',
    slug: 'burger-town',
    description: 'Hamburguesas gourmet con carne 100% de res y pan brioche horneado en casa.',
    logoUrl: 'https://picsum.photos/seed/burgertown-logo/100/100',
    coverImageUrl: 'https://picsum.photos/seed/burger-town/600/400',
    themeConfig: { primaryColor: '#e11d2e', secondaryColor: '#171717', accentColor: '#ffd500', backgroundColor: '#ffffff', textColor: '#171717', fontFamily: 'Inter' },
    isActive: true,
    ownerId: 'sample-owner-3',
    createdAt: new Date().toISOString(),
  },
  {
    id: 'sample-4',
    name: 'Fiesta de Tacos',
    slug: 'fiesta-de-tacos',
    description: 'Tacos al pastor, birria y salsas caseras con el sabor auténtico de la cocina mexicana.',
    logoUrl: 'https://picsum.photos/seed/tacosfiesta-logo/100/100',
    coverImageUrl: 'https://picsum.photos/seed/tacos-fiesta/600/400',
    themeConfig: { primaryColor: '#e11d2e', secondaryColor: '#171717', accentColor: '#ffd500', backgroundColor: '#ffffff', textColor: '#171717', fontFamily: 'Inter' },
    isActive: true,
    ownerId: 'sample-owner-4',
    createdAt: new Date().toISOString(),
  },
  {
    id: 'sample-5',
    name: 'Pasta Bella',
    slug: 'pasta-bella',
    description: 'Pastas frescas hechas a mano y salsas caseras que se cocinan a fuego lento.',
    logoUrl: 'https://picsum.photos/seed/pastabella-logo/100/100',
    coverImageUrl: 'https://picsum.photos/seed/pasta-bella/600/400',
    themeConfig: { primaryColor: '#e11d2e', secondaryColor: '#171717', accentColor: '#ffd500', backgroundColor: '#ffffff', textColor: '#171717', fontFamily: 'Inter' },
    isActive: true,
    ownerId: 'sample-owner-5',
    createdAt: new Date().toISOString(),
  },
  {
    id: 'sample-6',
    name: 'Verde Fresh',
    slug: 'verde-fresh',
    description: 'Bowls saludables, ensaladas y jugos naturales para un antojo más consciente.',
    logoUrl: 'https://picsum.photos/seed/verdefresh-logo/100/100',
    coverImageUrl: 'https://picsum.photos/seed/salad-fresh/600/400',
    themeConfig: { primaryColor: '#e11d2e', secondaryColor: '#171717', accentColor: '#ffd500', backgroundColor: '#ffffff', textColor: '#171717', fontFamily: 'Inter' },
    isActive: true,
    ownerId: 'sample-owner-6',
    createdAt: new Date().toISOString(),
  },
  {
    id: 'sample-7',
    name: 'Dragón Dorado',
    slug: 'dragon-dorado',
    description: 'Cocina china tradicional: arroz frito, dim sum y wok al momento.',
    logoUrl: 'https://picsum.photos/seed/dragondorado-logo/100/100',
    coverImageUrl: 'https://picsum.photos/seed/dragon-dorado-china/600/400',
    themeConfig: { primaryColor: '#e11d2e', secondaryColor: '#171717', accentColor: '#ffd500', backgroundColor: '#ffffff', textColor: '#171717', fontFamily: 'Inter' },
    isActive: true,
    ownerId: 'sample-owner-7',
    createdAt: new Date().toISOString(),
  },
  {
    id: 'sample-8',
    name: 'Parrilla Pampa',
    slug: 'parrilla-pampa',
    description: 'Sabores de Argentina: cortes a la parrilla, chimichurri casero y empanadas criollas.',
    logoUrl: 'https://picsum.photos/seed/parrillapampa-logo/100/100',
    coverImageUrl: 'https://picsum.photos/seed/parrilla-pampa-argentina/600/400',
    themeConfig: { primaryColor: '#e11d2e', secondaryColor: '#171717', accentColor: '#ffd500', backgroundColor: '#ffffff', textColor: '#171717', fontFamily: 'Inter' },
    isActive: true,
    ownerId: 'sample-owner-8',
    createdAt: new Date().toISOString(),
  },
  {
    id: 'sample-9',
    name: 'Ramen House',
    slug: 'ramen-house',
    description: 'Caldos de ramen cocidos durante horas y toppings 100% frescos.',
    logoUrl: 'https://picsum.photos/seed/ramenhouse-logo/100/100',
    coverImageUrl: 'https://picsum.photos/seed/ramen-house-japon/600/400',
    themeConfig: { primaryColor: '#e11d2e', secondaryColor: '#171717', accentColor: '#ffd500', backgroundColor: '#ffffff', textColor: '#171717', fontFamily: 'Inter' },
    isActive: true,
    ownerId: 'sample-owner-9',
    createdAt: new Date().toISOString(),
  },
];
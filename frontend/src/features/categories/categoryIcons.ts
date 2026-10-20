// Category icons for different vehicle types
export const categoryIcons = {
  car: '🚗',
  van: '🚐',
  truck: '🚚',
} as const

export function getCategoryIcon(key: string): string {
  return categoryIcons[key as keyof typeof categoryIcons] ?? '🚗'
}

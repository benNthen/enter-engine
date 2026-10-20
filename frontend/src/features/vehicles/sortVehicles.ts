export type SortField =
  | 'ownerName'
  | 'manufacturer'
  | 'yearOfManufacture'
  | 'weightKg'

export type SortDirection = 'asc' | 'desc'

type SortableVehicle = {
  id: number
  ownerName: string
  manufacturer: string
  yearOfManufacture: number
  weightKg: number
}

export function sortVehicles<T extends SortableVehicle>(
  vehicles: readonly T[],
  field: SortField,
  direction: SortDirection,
): T[] {
  const multiplier = direction === 'asc' ? 1 : -1

  // Copy the array so sorting does not mutate React state.
  return [...vehicles].sort((a, b) => {
    let comparison: number

    switch (field) {
      case 'ownerName':
      case 'manufacturer':
        comparison = a[field].localeCompare(b[field], 'en', {
          sensitivity: 'base',
        })
        break

      case 'yearOfManufacture':
      case 'weightKg':
        comparison = a[field] - b[field]
        break
    }

    // Equal values use ID ascending for a predictable order.
    if (comparison === 0) {
      return a.id - b.id
    }

    return comparison * multiplier
  })
}

import { describe, expect, it } from 'vitest'
import { sortVehicles } from './sortVehicles'

const vehicles = [
  {
    id: 1,
    ownerName: 'Zoe',
    manufacturer: 'Toyota',
    yearOfManufacture: 2010,
    weightKg: 3000,
    category: 'Heavy',
    categoryIconKey: 'truck',
  },
  {
    id: 2,
    ownerName: 'Amy',
    manufacturer: 'Ferrari',
    yearOfManufacture: 2020,
    weightKg: 1500,
    category: 'Medium',
    categoryIconKey: 'van',
  },
  {
    id: 3,
    ownerName: 'Ben',
    manufacturer: 'Mazda',
    yearOfManufacture: 2015,
    weightKg: 400,
    category: 'Light',
    categoryIconKey: 'car',
  },
]

describe('sortVehicles', () => {
  it.each([
    ['ownerName', 'asc', [2, 3, 1]],
    ['ownerName', 'desc', [1, 3, 2]],
    ['manufacturer', 'asc', [2, 3, 1]],
    ['manufacturer', 'desc', [1, 3, 2]],
    ['yearOfManufacture', 'asc', [1, 3, 2]],
    ['yearOfManufacture', 'desc', [2, 3, 1]],
    ['weightKg', 'asc', [3, 2, 1]],
    ['weightKg', 'desc', [1, 2, 3]],
  ] as const)('sorts %s in %s order', (field, direction, expectedIds) => {
    // Give each test its own input array.
    const input = vehicles.map((vehicle) => ({ ...vehicle }))

    const result = sortVehicles(input, field, direction)

    expect(result.map((vehicle) => vehicle.id)).toEqual(expectedIds)
  })

  it('handles an empty list', () => {
    expect(sortVehicles([], 'ownerName', 'asc')).toEqual([])
  })
})

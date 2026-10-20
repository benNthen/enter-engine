import { useEffect, useState, useMemo } from 'react'
import type { FormEvent } from 'react'
import CategoryAdministration from './CategoryAdministration'
import { getCategoryIcon } from './categoryIcons'
import { sortVehicles } from './sortVehicles'
import type { SortField, SortDirection } from './sortVehicles'

type Manufacturer = {
  id: number
  name: string
}

type Vehicle = {
  id: number
  ownerName: string
  manufacturer: string
  yearOfManufacture: number
  weightKg: number
  category: string
  categoryIconKey: string
}

type ApiProblem = {
  title?: string
  errors?: Record<string, string[]>
}

const sortLabels: Record<SortField, string> = {
  ownerName: "Owner's name",
  manufacturer: 'Manufacturer',
  yearOfManufacture: 'Year of manufacture',
  weightKg: 'Weight',
}

async function getJson<T>(url: string): Promise<T> {
  const response = await fetch(url)

  if (!response.ok) {
    throw new Error('Unable to load data. Please try again.')
  }

  return response.json() as Promise<T>
}

export default function App() {
  // For representing manufacturer and vehicles
  const [manufacturers, setManufacturers] = useState<Manufacturer[]>([])
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  // For representing vehicle properties
  const [ownerName, setOwnerName] = useState('')
  const [manufacturerId, setManufacturerId] = useState<number | null>(null)
  const [year, setYear] = useState<number | null>(null)
  const [weight, setWeight] = useState('')

  // For API functionality and error handling
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)

  // For sorting capabilities
  const [sortField, setSortField] = useState<SortField>('ownerName')
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc')

  const sortedVehicles = sortVehicles(vehicles, sortField, sortDirection)

  function handleSorts(field: SortField) {
    if (field === sortField) {
      setSortDirection((current) => (current === 'asc' ? 'desc' : 'asc'))
    } else {
      setSortField(field)
      setSortDirection('asc')
    }
  }

  useEffect(() => {
    let cancelled = false

    async function loadData() {
      try {
        const [manufacturerData, vehicleData] = await Promise.all([
          getJson<Manufacturer[]>('/api/manufacturers'),
          getJson<Vehicle[]>('/api/vehicles'),
        ])

        if (!cancelled) {
          setManufacturers(manufacturerData)
          setVehicles(vehicleData)
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Unable to load data.')
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    void loadData()

    return () => {
      cancelled = true
    }
  }, [])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    setError('')
    setMessage('')

    if (!ownerName.trim()) {
      setError("Enter the owner's name.")
      return
    }

    setSaving(true)
    let created = false

    try {
      const response = await fetch('/api/vehicles', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          ownerName: ownerName.trim(),
          manufacturerId: Number(manufacturerId),
          yearOfManufacture: Number(year),
          weightKg: Number(weight),
        }),
      })

      if (!response.ok) {
        const problem: ApiProblem = await response.json().catch(() => ({}))

        const validationMessages = Object.values(problem.errors ?? {}).flat()

        throw new Error(
          validationMessages.length > 0
            ? validationMessages.join(' ')
            : (problem.title ?? 'Unable to save vehicle.'),
        )
      }

      created = true

      setOwnerName('')
      setManufacturerId(null)
      setYear(null)
      setWeight('')
      setMessage('Vehicle saved successfully.')

      const updatedVehicles = await getJson<Vehicle[]>('/api/vehicles')
      setVehicles(updatedVehicles)
    } catch (err) {
      setError(
        created
          ? 'The vehicle was saved, but we were unable to refresh the list.'
          : err instanceof Error
            ? err.message
            : 'Unable to save vehicle.',
      )
    } finally {
      setSaving(false)
    }
  }
  // For GET /api/vehicle endpoint
  async function refreshVehicles() {
    const updatedVehicles = await getJson<Vehicle[]>('/api/vehicles')
    setVehicles(updatedVehicles)
  }

  const sortingVehicles = useMemo(() => {
    return [...vehicles].sort((a, b) => {
      let comparison: number

      switch (sortField) {
        case 'ownerName':
        case 'manufacturer':
          comparison = a[sortField].localeCompare(b[sortField], 'en', {
            sensitivity: 'base',
          })
          break

        case 'yearOfManufacture':
        case 'weightKg':
          comparison = a[sortField] - b[sortField]
          break
      }

      const directedComparison =
        sortDirection === 'asc' ? comparison : -comparison

      // Keep a predictable order when two values match.
      return directedComparison || a.id - b.id
    })
  }, [vehicles, sortField, sortDirection])

  function handleSort(field: SortField) {
    if (field === sortField) {
      setSortDirection((current) => (current === 'asc' ? 'desc' : 'asc'))
    } else {
      setSortField(field)
      setSortDirection('asc')
    }
  }

  function sortIndicator(field: SortField) {
    if (field !== sortField) {
      return ''
    }

    return sortDirection === 'asc' ? ' ↑' : ' ↓'
  }

  function sortButtonLabel(field: SortField) {
    const nextDirection =
      field === sortField && sortDirection === 'asc'
        ? 'descending'
        : 'ascending'

    return `Sort by ${sortLabels[field]}, ${nextDirection}`
  }

  return (
    <main>
      <h1>Enter Engine</h1>
      {error && <p role="alert">{error}</p>}
      {message && <p>{message}</p>}
      <h2>Create Vehicle</h2>
      <p>Time to enter your vehicle details using the form below.</p>
      <form className="create-vehicle-form" onSubmit={handleSubmit}>
        <fieldset disabled={loading || saving}>
          <div>
            <label htmlFor="ownerName">Owner Name:</label>
            <input
              id="ownerName"
              value={ownerName}
              onChange={(e) => setOwnerName(e.target.value)}
              maxLength={100}
              required
            />
          </div>

          <div>
            <label htmlFor="manufacturer">Manufacturer:</label>
            <select
              id="manufacturer"
              value={manufacturerId ?? ''}
              onChange={(e) => setManufacturerId(Number(e.target.value))}
              required
            >
              <option value="">Select</option>

              {manufacturers.map((manufacturer) => (
                <option key={manufacturer.id} value={manufacturer.id}>
                  {manufacturer.name}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label htmlFor="year">Year of Manufacture:</label>
            <input
              id="year"
              type="number"
              value={year ?? ''}
              min={1886}
              max={new Date().getFullYear()}
              onChange={(e) => setYear(Number(e.target.value))}
              required
            />
          </div>

          <div>
            <label htmlFor="weight">Weight (kg):</label>
            <input
              id="weight"
              type="number"
              min="0.01"
              max="99999999.99"
              step="0.01"
              value={weight}
              onChange={(event) => setWeight(event.currentTarget.value)}
              required
            />
          </div>

          <div className="form-actions">
            <button type="submit">{saving ? 'Saving...' : 'Submit'}</button>
            <button
              type="button"
              onClick={() => {
                setOwnerName('')
                setManufacturerId(null)
                setYear(null)
                setWeight('')
              }}
            >
              Reset
            </button>
          </div>
        </fieldset>
      </form>
      <h2>Vehicle list</h2>

      <p role="status">
        Sorted by {sortLabels[sortField]} —{' '}
        {sortDirection === 'asc' ? 'ascending' : 'descending'}.
      </p>

      {loading ? (
        <p>Loading vehicles...</p>
      ) : vehicles.length === 0 ? (
        <p>No vehicles to display.</p>
      ) : (
        <table className="vehicle-table">
          <thead>
            <tr>
              <th
                scope="col"
                aria-sort={
                  sortField === 'ownerName'
                    ? sortDirection === 'asc'
                      ? 'ascending'
                      : 'descending'
                    : undefined
                }
              >
                <button
                  type="button"
                  onClick={() => handleSort('ownerName')}
                  aria-label={sortButtonLabel('ownerName')}
                >
                  Owner's Name
                  <span aria-hidden="true">{sortIndicator('ownerName')}</span>
                </button>
              </th>

              <th
                scope="col"
                aria-sort={
                  sortField === 'manufacturer'
                    ? sortDirection === 'asc'
                      ? 'ascending'
                      : 'descending'
                    : undefined
                }
              >
                <button
                  type="button"
                  onClick={() => handleSort('manufacturer')}
                  aria-label={sortButtonLabel('manufacturer')}
                >
                  Manufacturer
                  <span aria-hidden="true">
                    {sortIndicator('manufacturer')}
                  </span>
                </button>
              </th>

              <th
                scope="col"
                aria-sort={
                  sortField === 'yearOfManufacture'
                    ? sortDirection === 'asc'
                      ? 'ascending'
                      : 'descending'
                    : undefined
                }
              >
                <button
                  type="button"
                  onClick={() => handleSort('yearOfManufacture')}
                  aria-label={sortButtonLabel('yearOfManufacture')}
                >
                  Year of manufacture
                  <span aria-hidden="true">
                    {sortIndicator('yearOfManufacture')}
                  </span>
                </button>
              </th>

              <th
                scope="col"
                aria-sort={
                  sortField === 'weightKg'
                    ? sortDirection === 'asc'
                      ? 'ascending'
                      : 'descending'
                    : undefined
                }
              >
                <button
                  type="button"
                  onClick={() => handleSort('weightKg')}
                  aria-label={sortButtonLabel('weightKg')}
                >
                  Weight (kg)
                  <span aria-hidden="true">{sortIndicator('weightKg')}</span>
                </button>
              </th>
              <th scope="col">Category</th>
            </tr>
          </thead>

          <tbody>
            {sortedVehicles.map((vehicle) => (
              <tr key={vehicle.id}>
                <td>{vehicle.ownerName}</td>
                <td>{vehicle.manufacturer}</td>
                <td>{vehicle.yearOfManufacture}</td>
                <td>{vehicle.weightKg.toFixed(2)}</td>
                <td>
                  <span aria-hidden="true">
                    {getCategoryIcon(vehicle.categoryIconKey)}
                  </span>{' '}
                  {vehicle.category}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      <CategoryAdministration onSaved={refreshVehicles} />
    </main>
  )
}

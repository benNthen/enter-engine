import { useEffect, useState } from 'react'
import { categoryIcons } from './categoryIcons'

type SavedCategory = {
  id: number
  name: string
  minWeightKg: number
  maxWeightKg: number | null
  iconKey: string
}

type DraftCategory = {
  key: string
  name: string
  minWeightKg: string
  maxWeightKg: string
  iconKey: string
}

type ApiProblem = {
  title?: string
  errors?: Record<string, string[]>
}

type Props = {
  onSaved: () => Promise<void>
}

function toDraft(category: SavedCategory): DraftCategory {
  return {
    key: crypto.randomUUID(),
    name: category.name,
    minWeightKg: String(category.minWeightKg),
    maxWeightKg:
      category.maxWeightKg === null ? '' : String(category.maxWeightKg),
    iconKey: category.iconKey,
  }
}

export default function CategoryAdministration({ onSaved }: Props) {
  const [rows, setRows] = useState<DraftCategory[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  async function fetchCategories(): Promise<SavedCategory[]> {
    const response = await fetch('/api/categories')

    if (!response.ok) {
      throw new Error('Unable to load categories')
    }

    return response.json()
  }

  useEffect(() => {
    let cancelled = false

    async function load() {
      try {
        const categories = await fetchCategories()

        if (!cancelled) {
          setRows(categories.map(toDraft))
        }
      } catch (err) {
        if (!cancelled) {
          setError(
            err instanceof Error ? err.message : 'Unable to load categories.',
          )
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    void load()

    return () => {
      cancelled = true
    }
  }, [])

  function updateRow(
    key: string,
    field: 'name' | 'minWeightKg' | 'maxWeightKg' | 'iconKey',
    value: string,
  ) {
    setRows((current) =>
      current.map((row) =>
        row.key === key ? { ...row, [field]: value } : row,
      ),
    )

    setMessage('')
  }

  function addRow() {
    setRows((current) => [
      ...current,
      {
        key: crypto.randomUUID(),
        name: '',
        minWeightKg: '',
        maxWeightKg: '',
        iconKey: 'car',
      },
    ])

    setMessage('')
  }

  function deleteRow(key: string) {
    setRows((current) => current.filter((row) => row.key !== key))
    setMessage('')
  }

  async function reload() {
    setLoading(true)
    setError('')
    setMessage('')

    try {
      const categories = await fetchCategories()
      setRows(categories.map(toDraft))
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Unable to reload categories.',
      )
    } finally {
      setLoading(false)
    }
  }

  async function save() {
    setError('')
    setMessage('')

    if (rows.some((row) => !row.name.trim() || row.minWeightKg.trim() === '')) {
      setError('Each category needs a name and a lower boundary.')
      return
    }

    const categories = rows.map((row) => ({
      name: row.name.trim(),
      minWeightKg: Number(row.minWeightKg),
      maxWeightKg:
        row.maxWeightKg.trim() === '' ? null : Number(row.maxWeightKg),
      iconKey: row.iconKey,
    }))

    if (
      categories.some(
        (category) =>
          !Number.isFinite(category.minWeightKg) ||
          (category.maxWeightKg !== null &&
            !Number.isFinite(category.maxWeightKg)),
      )
    ) {
      setError('Enter valid numeric boundaries.')
      return
    }

    setSaving(true)
    let committed = false

    try {
      const response = await fetch('/api/categories', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ categories }),
      })

      if (!response.ok) {
        const problem: ApiProblem = await response.json().catch(() => ({}))

        const messages = Object.values(problem.errors ?? {}).flat()

        throw new Error(
          messages.length > 0
            ? messages.join(' ')
            : (problem.title ?? 'Unable to save categories.'),
        )
      }

      committed = true

      // Recalculate the categories displayed for existing vehicles.
      await onSaved()

      const savedCategories = await fetchCategories()
      setRows(savedCategories.map(toDraft))
      setMessage('Categories saved. Vehicle categories updated.')
    } catch (err) {
      setError(
        committed
          ? 'Categories were saved, but the page could not refresh. Reload the page.'
          : err instanceof Error
            ? err.message
            : 'Unable to save categories.',
      )
    } finally {
      setSaving(false)
    }
  }

  return (
    <section aria-labelledby="category-heading">
      <h2 id="category-heading">Category administration</h2>

      <p>
        Lower boundaries are inclusive; upper boundaries are exclusive. Leave
        the final upper boundary blank for no limit.
      </p>

      <p>Changes remain drafts until you select Save categories.</p>

      {error && <p role="alert">{error}</p>}
      {message && <p role="status">{message}</p>}

      {loading ? (
        <p>Loading categories...</p>
      ) : (
        <fieldset disabled={saving}>
          <legend>Category configuration</legend>

          <table>
            <thead>
              <tr>
                <th scope="col">Name</th>
                <th scope="col">From kg, inclusive</th>
                <th scope="col">To kg, exclusive</th>
                <th scope="col">Icon</th>
                <th scope="col">Action</th>
              </tr>
            </thead>

            <tbody>
              {rows.map((row, index) => (
                <tr key={row.key}>
                  <td>
                    <input
                      aria-label={`Category ${index + 1} name`}
                      value={row.name}
                      maxLength={50}
                      onChange={(event) =>
                        updateRow(row.key, 'name', event.target.value)
                      }
                    />
                  </td>

                  <td>
                    <input
                      aria-label={`Category ${index + 1} lower boundary`}
                      type="number"
                      min="0"
                      max="99999999.99"
                      step="0.01"
                      value={row.minWeightKg}
                      onChange={(event) =>
                        updateRow(row.key, 'minWeightKg', event.target.value)
                      }
                    />
                  </td>

                  <td>
                    <input
                      aria-label={`Category ${index + 1} upper boundary`}
                      type="number"
                      min="0"
                      max="99999999.99"
                      step="0.01"
                      placeholder="No limit"
                      value={row.maxWeightKg}
                      onChange={(event) =>
                        updateRow(row.key, 'maxWeightKg', event.target.value)
                      }
                    />
                  </td>

                  <td>
                    <select
                      aria-label={`Category ${index + 1} icon`}
                      value={row.iconKey}
                      onChange={(event) =>
                        updateRow(row.key, 'iconKey', event.target.value)
                      }
                    >
                      {Object.entries(categoryIcons).map(([key, icon]) => (
                        <option key={key} value={key}>
                          {icon} {key}
                        </option>
                      ))}
                    </select>
                  </td>

                  <td>
                    <button
                      type="button"
                      onClick={() => deleteRow(row.key)}
                      aria-label={`Delete category ${row.name || index + 1}`}
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <button type="button" onClick={addRow}>
            Add category
          </button>

          <button type="button" onClick={() => void save()}>
            {saving ? 'Saving...' : 'Save categories'}
          </button>
        </fieldset>
      )}

      <button
        type="button"
        disabled={loading || saving}
        onClick={() => void reload()}
      >
        Reload saved categories
      </button>
    </section>
  )
}

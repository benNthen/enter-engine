import CategoryAdministration from '../features/categories/CategoryAdministration'

type Props = {
  onBack: () => void
}

export default function CategoryAdministrationPage({ onBack }: Props) {
  return (
    <main>
      <header className="page-header">
        <h1>Enter Engine</h1>
        <button type="button" onClick={onBack}>
          <span aria-hidden="true">←</span> Back to vehicles
        </button>
      </header>
      <CategoryAdministration />
    </main>
  )
}

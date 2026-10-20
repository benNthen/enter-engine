import { useEffect, useState } from 'react'
import CategoryAdministrationPage from './pages/CategoryAdministrationPage'
import VehicleAdministrationPage from './pages/VehicleAdministrationPage'

type Page = 'vehicles' | 'categories'

function getPageFromPath(): Page {
  return window.location.pathname === '/category-admin'
    ? 'categories'
    : 'vehicles'
}

export default function App() {
  const [currentPage, setCurrentPage] = useState(getPageFromPath)

  function navigateTo(page: Page, replace = false) {
    const path = page === 'categories' ? '/category-admin' : '/'
    if (replace) {
      window.history.replaceState({}, '', path)
    } else {
      window.history.pushState({}, '', path)
    }
    setCurrentPage(page)
  }

  useEffect(() => {
    function handlePopState() {
      setCurrentPage(getPageFromPath())
    }

    window.addEventListener('popstate', handlePopState)
    return () => window.removeEventListener('popstate', handlePopState)
  }, [])

  if (currentPage === 'categories') {
    return (
      <CategoryAdministrationPage onBack={() => navigateTo('vehicles', true)} />
    )
  }

  return (
    <VehicleAdministrationPage
      onNavigateCategories={() => navigateTo('categories')}
    />
  )
}

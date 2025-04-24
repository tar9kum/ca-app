import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import Header from './components/header/Header'
import PriceTracker from './components/priceTracker/PriceTracker'

function App() {
  return (
    <>
     <Header></Header>
     <PriceTracker></PriceTracker>
    </>
  )
}
export default App;

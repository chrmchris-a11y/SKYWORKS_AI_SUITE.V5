import React from 'react'
import { Link } from 'react-router-dom'
import { routes } from '../routes'

export default function Home() {
  return (
    <div style={{padding:16}}>
      <h2>Skyworks Web</h2>
      <p>Επιλέξτε λειτουργία:</p>
      <ul>
        <li><Link to={routes.phase6}>Φάση 6: Mission Planning & Templates</Link></li>
      </ul>
    </div>
  )
}

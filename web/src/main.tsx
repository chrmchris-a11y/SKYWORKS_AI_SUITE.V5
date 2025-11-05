import React from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom'
import { routes } from './ui/routes'
import Home from './ui/pages/Home'
import Phase6 from './ui/pages/Phase6'

const root = createRoot(document.getElementById('root')!)
root.render(
	<BrowserRouter>
		<div style={{height:'100%', display:'flex', flexDirection:'column'}}>
			<nav style={{padding:'8px 12px', borderBottom:'1px solid #eee', display:'flex', gap:12}}>
				<Link to={routes.home}>Αρχική</Link>
				<Link to={routes.phase6}>Φάση 6</Link>
			</nav>
			<div style={{flex:1, minHeight:0}}>
				<Routes>
					<Route path={routes.home} element={<Home />} />
					<Route path={routes.phase6} element={<Phase6 />} />
				</Routes>
			</div>
		</div>
	</BrowserRouter>
)

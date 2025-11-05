import React, { useEffect, useMemo, useRef, useState } from 'react'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { seedTemplates } from '../modules/missionTemplates/seed'
import { validateTemplate } from '../modules/missionTemplates/validation'
import { previewSail } from '../modules/missionTemplates/api'

export default function App() {
  const mapRef = useRef<HTMLDivElement>(null)
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const templates = useMemo(() => seedTemplates, [])
  const results = useMemo(() => templates.map(t => ({ t, v: validateTemplate(t) })), [templates])
  const [finalGrc, setFinalGrc] = useState<number>(6)
  const [preview, setPreview] = useState<any>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  useEffect(() => {
    if (!mapRef.current) return
    const map = L.map(mapRef.current).setView([37.9838, 23.7275], 12) // Athens default
    const tiles = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap contributors'
    })
    tiles.addTo(map)
    return () => { map.remove() }
  }, [])
  return (
    <div style={{height:'100%', display:'flex'}}>
      <aside style={{width:360, borderRight:'1px solid #ddd', padding:12, overflow:'auto'}}>
        <h3 style={{marginTop:0}}>Mission Templates</h3>
        <ul style={{listStyle:'none', padding:0, margin:0}}>
          {results.map(({ t, v }) => (
            <li key={t.template_id} style={{
              border:'1px solid #eee', borderRadius:6, padding:10, marginBottom:8,
              background: selectedId===t.template_id ? '#f7fbff' : '#fff', cursor:'pointer'
            }} onClick={() => setSelectedId(t.template_id)}>
              <div style={{display:'flex', justifyContent:'space-between', alignItems:'baseline'}}>
                <strong>{t.name}</strong>
                <span style={{fontSize:12, color:'#666'}}>SORA {t.sora_version}</span>
              </div>
              <div style={{marginTop:6, display:'flex', gap:8, fontSize:12}}>
                <span title="Blocking errors" style={{color: v.errors.length ? '#b00020' : '#2e7d32'}}>
                  Errors: {v.errors.length}
                </span>
                <span title="Policy warnings" style={{color:'#a67c00'}}>
                  Warnings: {v.warnings.length}
                </span>
              </div>
            </li>
          ))}
        </ul>
        {selectedId && (() => {
          const sel = results.find(r => r.t.template_id === selectedId)!
          return (
            <div style={{marginTop:12}}>
              <h4 style={{margin:'8px 0'}}>Selected</h4>
              <div style={{fontSize:12, color:'#555'}}>Template ID: {sel.t.template_id}</div>
              {sel.v.errors.length > 0 && (
                <div style={{marginTop:8}}>
                  <div style={{fontWeight:600, color:'#b00020'}}>Errors</div>
                  <ul style={{marginTop:4}}>
                    {sel.v.errors.map((e,i)=>(<li key={i} style={{fontSize:12}}>{e}</li>))}
                  </ul>
                </div>
              )}
              {sel.v.warnings.length > 0 && (
                <div style={{marginTop:8}}>
                  <div style={{fontWeight:600, color:'#a67c00'}}>Warnings</div>
                  <ul style={{marginTop:4}}>
                    {sel.v.warnings.map((w,i)=>(<li key={i} style={{fontSize:12}}>{w}</li>))}
                  </ul>
                </div>
              )}
              <div style={{marginTop:12, paddingTop:12, borderTop:'1px solid #eee'}}>
                <div style={{fontWeight:600, marginBottom:6}}>Preview SAIL</div>
                <div style={{display:'flex', gap:8, alignItems:'center'}}>
                  <label style={{fontSize:12}}>final_grc:</label>
                  <input type="number" value={finalGrc} onChange={e=>setFinalGrc(Number(e.target.value))} style={{width:80}} />
                  <button disabled={loading} onClick={async()=>{
                    setLoading(true); setError(null); setPreview(null)
                    try { const resp = await previewSail(sel.t, finalGrc); setPreview(resp) }
                    catch(e:any){ setError(String(e?.message ?? e)) }
                    finally { setLoading(false) }
                  }}>Υπολογισμός</button>
                </div>
                {loading && <div style={{fontSize:12, color:'#666', marginTop:6}}>Υπολογισμός...</div>}
                {error && <div style={{fontSize:12, color:'#b00020', marginTop:6}}>Σφάλμα: {error}</div>}
                {preview && (
                  <pre style={{fontSize:12, marginTop:8, background:'#fafafa', padding:8, border:'1px solid #eee', borderRadius:6}}>
                    {JSON.stringify(preview, null, 2)}
                  </pre>
                )}
              </div>
            </div>
          )
        })()}
      </aside>
      <div ref={mapRef} style={{height:'100%', flex:1}} />
    </div>
  )
}

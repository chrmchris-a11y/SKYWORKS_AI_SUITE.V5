import { MissionTemplate } from './types'

type SailResponse = {
  sail?: 'I' | 'II' | 'III' | 'IV' | 'V' | 'VI' | null
  rationale?: string
  [k: string]: any
}

export async function previewSail(template: MissionTemplate, final_grc: number): Promise<SailResponse> {
  // Decide payload by SORA version
  let body: any
  if (template.sora_version === '2.5') {
    const s25 = template.sora_input_block.sora_2_5
    if (!s25) throw new Error('Template missing sora_2_5 block')
    body = {
      sora_version: '2.5',
      final_grc: final_grc,
      residual_arc_level: s25.residual_arc_level,
    }
  } else {
    const s20 = template.sora_input_block.sora_2_0
    if (!s20) throw new Error('Template missing sora_2_0 block')
    body = {
      sora_version: '2.0',
      final_grc: final_grc,
      final_arc: s20.final_arc ?? s20.initial_arc,
    }
  }

  const r = await fetch('/py8001/api/v1/calculate/sail', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!r.ok) {
    const text = await r.text()
    throw new Error(`SAIL preview failed: ${r.status} ${text}`)
  }
  return r.json()
}

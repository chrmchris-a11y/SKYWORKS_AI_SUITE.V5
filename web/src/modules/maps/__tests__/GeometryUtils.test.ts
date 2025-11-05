import { describe, it, expect } from 'vitest'
import { GeometryUtils } from '../GeometryUtils'

describe('GeometryUtils', () => {
  it('distanceMeters ~ Athens center to Acropolis', () => {
    const athens: [number, number] = [37.9838, 23.7275]
    const acropolis: [number, number] = [37.9715, 23.7266]
    const d = GeometryUtils.distanceMeters(athens, acropolis)
    expect(d).toBeGreaterThan(1000)
    expect(d).toBeLessThan(3000)
  })

  it('areaSqMetersWebMercator returns >0 for triangle', () => {
    const tri: [number, number][] = [
      [37.98, 23.72],
      [37.99, 23.73],
      [37.98, 23.74]
    ]
    const a = GeometryUtils.areaSqMetersWebMercator(tri)
    expect(a).toBeGreaterThan(0)
  })
})

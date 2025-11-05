import { describe, it, expect } from 'vitest'
import { routes } from '../routes'

describe('Routes registry', () => {
  it('περιέχει την σελίδα Φάση 6', () => {
    expect(routes.phase6).toBe('/phase6')
  })
})

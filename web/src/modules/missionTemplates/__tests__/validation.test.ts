import { describe, it, expect } from 'vitest';
import { seedTemplates } from '../seed';
import { validateTemplate } from '../validation';

describe('Mission Templates validation (Step 52)', () => {
  it('all seed templates validate without hard errors', () => {
    for (const t of seedTemplates) {
      const { errors } = validateTemplate(t);
      expect(errors, `Template ${t.template_id} should have no blocking errors`).toEqual([]);
    }
  });
});

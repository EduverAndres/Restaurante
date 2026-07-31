import { describe, it, expect } from 'vitest';
import { parseAiResponse, AiOrderSummary } from './ai-response-validator';

describe('parseAiResponse', () => {
  it('should parse a valid order_summary', () => {
    const input = {
      type: 'order_summary',
      items: [
        { menuItemId: 'm1', name: 'Milanesa', quantity: 2, unitPrice: 12.5 },
      ],
      total: 25,
      summary: '2 Milanesas',
    };

    const result = parseAiResponse(input);

    expect(result).not.toBeNull();
    expect(result!.type).toBe('order_summary');
    expect(result!.items).toHaveLength(1);
    expect(result!.items[0].name).toBe('Milanesa');
    expect(result!.total).toBe(25);
  });

  it('should return null for malformed JSON object', () => {
    const input = { type: 'unknown' };

    const result = parseAiResponse(input);

    expect(result).toBeNull();
  });

  it('should return null when items array is empty', () => {
    const input = {
      type: 'order_summary',
      items: [],
      total: 0,
      summary: 'empty',
    };

    const result = parseAiResponse(input);

    expect(result).toBeNull();
  });

  it('should return null when negative quantity', () => {
    const input = {
      type: 'order_summary',
      items: [
        { menuItemId: 'm1', name: 'Item', quantity: -1, unitPrice: 10 },
      ],
      total: -10,
      summary: 'negative',
    };

    const result = parseAiResponse(input);

    expect(result).toBeNull();
  });

  it('should parse with optional next_question field', () => {
    const input = {
      type: 'order_summary',
      items: [
        { menuItemId: 'm1', name: 'Pizza', quantity: 1, unitPrice: 15 },
      ],
      total: 15,
      summary: '1 Pizza',
      next_question: '¿Querés algo más?',
    };

    const result = parseAiResponse(input);

    expect(result).not.toBeNull();
    expect(result!.next_question).toBe('¿Querés algo más?');
  });
});

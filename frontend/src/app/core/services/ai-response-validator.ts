import { z } from 'zod';

const AiOrderItemSchema = z.object({
  menuItemId: z.string(),
  name: z.string(),
  quantity: z.number().positive(),
  unitPrice: z.number().positive(),
});

const AiOrderSummarySchema = z.object({
  type: z.literal('order_summary'),
  items: z.array(AiOrderItemSchema).min(1),
  total: z.number().positive(),
  summary: z.string(),
  next_question: z.string().optional(),
});

export type AiOrderSummary = z.infer<typeof AiOrderSummarySchema>;

export function parseAiResponse(data: unknown): AiOrderSummary | null {
  const result = AiOrderSummarySchema.safeParse(data);
  if (!result.success) return null;
  return result.data;
}

export function parseMessageContent(content: string): AiOrderSummary | null {
  try {
    const data = JSON.parse(content);
    return parseAiResponse(data);
  } catch {
    return null;
  }
}

export interface ApiErrorEnvelope {
  success: false;
  message: string;
  data: null;
  errors?: string[] | null;
}

export function isApiErrorEnvelope(value: unknown): value is ApiErrorEnvelope {
  return typeof value === 'object' && value !== null && (value as any).success === false;
}

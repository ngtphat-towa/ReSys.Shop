import { z } from 'zod'

/**
 * Validation schema for Example entities.
 * Used for both client-side form validation (via VeeValidate) and type safety.
 */
export const ExampleSchema = z.object({
  /** The display name of the example. Required, min 3 chars. */
  name: z.string().min(3, 'Name must be at least 3 characters'),

  /** Detailed description. Optional, max 500 chars. */
  description: z.string().max(500, 'Description too long').optional(),

  /** Unit price. Must be a positive number. */
  price: z.number().positive('Price must be positive'),

  /** URL to the hosted image. Managed separately via file upload but tracked here for state. */
  image_url: z.string().nullable().optional(),
})

/**
 * TypeScript type inferred from the ExampleSchema.
 */
export type ExampleFormData = z.infer<typeof ExampleSchema>

import { z } from 'zod'

export const ExampleCategorySchema = z.object({
  name: z.string().min(1, 'Name is required').max(100, 'Name must not exceed 100 characters'),
  description: z.string().max(500, 'Description must not exceed 500 characters').optional().nullable(),
})

export type ExampleCategoryFormData = z.infer<typeof ExampleCategorySchema>

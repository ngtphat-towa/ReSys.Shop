import * as zod from 'zod';

/**
 * Zod Validation Schema
 * The final single source of truth for Example validation and types
 */
export const ExampleSchema = zod.object({
    name: zod.string()
        .min(1, 'Example name is required.')
        .max(255, 'Example name cannot exceed 255 characters.'),
    description: zod.string()
        .min(1, 'Example description is required.')
        .max(2000, 'Example description cannot exceed 2000 characters.'),
    price: zod.number()
        .min(0.01, 'Price must be at least $0.01.'),
    image_url: zod.string().optional()
});

/**
 * Derived TypeScript interface from the Zod schema
 */
export type ExampleInput = zod.infer<typeof ExampleSchema>;
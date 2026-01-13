import * as zod from 'zod';

export enum ExampleStatus {
    Draft = 0,
    Active = 1,
    Archived = 2,
}

/**
 * Zod Validation Schema
 * Mirrors the Admin setup for consistency
 */
export const ExampleSchema = zod.object({
    name: zod.string()
        .min(1, 'Example name is required.')
        .max(255, 'Example name cannot exceed 255 characters.'),
    description: zod.string()
        .max(2000, 'Example description cannot exceed 2000 characters.')
        .optional(),
    price: zod.number()
        .min(0.01, 'Price must be at least $0.01.'),
    image_url: zod.string().nullable().optional(),
    status: zod.nativeEnum(ExampleStatus).default(ExampleStatus.Draft),
    hex_color: zod.string().regex(/^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/, 'Invalid hex color').optional().nullable(),
});

/**
 * Derived TypeScript interface from the Zod schema
 */
export type ExampleInput = zod.infer<typeof ExampleSchema>;

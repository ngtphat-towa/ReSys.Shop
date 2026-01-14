/**
 * Shared formatting utilities for the Shop application.
 */
export function useFormatter() {
  /**
   * Formats a number as USD currency.
   * @param value The numeric value to format.
   */
  const formatCurrency = (value: number | undefined | null) => {
    if (value === undefined || value === null) return ''
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(value)
  }

  /**
   * Truncates a string to a specified length and adds an ellipsis.
   */
  const truncate = (text: string | null | undefined, length: number) => {
    if (!text) return ''
    if (text.length <= length) return text
    return text.substring(0, length) + '...'
  }

  return {
    formatCurrency,
    truncate,
  }
}

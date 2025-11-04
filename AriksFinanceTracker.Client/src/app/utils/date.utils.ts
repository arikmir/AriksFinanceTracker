/**
 * Date utilities for handling Australian Eastern Time (UTC+10)
 */

export class DateUtils {
  private static readonly AUSTRALIA_TIMEZONE = 'Australia/Sydney';
  
  /**
   * Convert a date to Australian Eastern Time and normalize to start of day
   */
  static toAustralianDate(date: Date): Date {
    const australianDate = new Date(date.toLocaleString('en-US', { timeZone: DateUtils.AUSTRALIA_TIMEZONE }));
    // Set to start of day in Australian timezone
    australianDate.setHours(0, 0, 0, 0);
    return australianDate;
  }
  
  /**
   * Create a date that represents the Australian date without timezone conversion
   */
  static createAustralianDate(year: number, month: number, day: number): Date {
    // Create date in Australian timezone at noon to avoid timezone issues
    return new Date(year, month, day, 12, 0, 0, 0);
  }
  
  /**
   * Get the current date in Australian timezone
   */
  static getCurrentAustralianDate(): Date {
    return DateUtils.toAustralianDate(new Date());
  }
  
  /**
   * Format a date for API submission (ensures it stays in the correct day)
   */
  static formatForAPI(date: Date): Date {
    // Ensure the date is normalized to Australian timezone and won't shift days
    const year = date.getFullYear();
    const month = date.getMonth();
    const day = date.getDate();
    
    // Create a new date at noon Australian time to prevent timezone shifting
    return new Date(year, month, day, 12, 0, 0, 0);
  }
  
  /**
   * Parse a date string from API and normalize to Australian date
   */
  static parseFromAPI(dateString: string): Date {
    const date = new Date(dateString);
    return DateUtils.toAustralianDate(date);
  }
  
  /**
   * Get month and year for filtering (Australian timezone aware)
   */
  static getAustralianMonthYear(date: Date): { month: number; year: number } {
    const australianDate = DateUtils.toAustralianDate(date);
    return {
      month: australianDate.getMonth() + 1, // JavaScript months are 0-based
      year: australianDate.getFullYear()
    };
  }
  
  /**
   * Check if two dates are in the same month/year in Australian timezone
   */
  static isSameAustralianMonth(date1: Date, date2: Date): boolean {
    const aus1 = DateUtils.getAustralianMonthYear(date1);
    const aus2 = DateUtils.getAustralianMonthYear(date2);
    
    return aus1.month === aus2.month && aus1.year === aus2.year;
  }
}
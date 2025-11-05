import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'percentageFormat',
  standalone: true
})
export class PercentageFormatPipe implements PipeTransform {
  transform(value: number | null | undefined, decimals: number = 1): string {
    if (value === null || value === undefined) {
      return '0.0%';
    }

    return `${value.toFixed(decimals)}%`;
  }
}

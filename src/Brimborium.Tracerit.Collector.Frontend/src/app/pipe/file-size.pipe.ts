import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'fileSize'
})
export class FileSizePipe implements PipeTransform {

  transform(value: number): string {
    if (value >= 1048576) {
      const mbValue = (value / 1048576).toFixed(2);
      return `${mbValue} MB`;
    }
    if (value >= 1024) {
      const kbValue = (value / 1024).toFixed(2);
      return `${kbValue} KB`;
    }
    return `${value} B`;
  }

}

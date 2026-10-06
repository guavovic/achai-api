import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { AddressSearch } from '../address-search';

export const MAX_BATCH = 20;

export function parseZipCodes(text: string): string[] {
  return text.split(/[\s,;]+/).filter(Boolean);
}

@Component({
  selector: 'app-batch-search',
  imports: [ReactiveFormsModule],
  templateUrl: './batch-search.html',
  styleUrl: '../search-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BatchSearch {
  private readonly search = inject(AddressSearch);

  readonly zipCodes = new FormControl('', { nonNullable: true });
  readonly error = signal('');

  submit(event?: Event): void {
    event?.preventDefault();
    const zipCodes = parseZipCodes(this.zipCodes.value);

    if (zipCodes.length === 0) return this.error.set('Cole pelo menos um CEP.');
    if (zipCodes.length > MAX_BATCH)
      return this.error.set(`São ${zipCodes.length} CEPs. O limite é ${MAX_BATCH} por vez.`);

    this.error.set('');
    this.search.byZipCodes(zipCodes);
  }

  searchFor(zipCodes: string[]): void {
    this.zipCodes.setValue(zipCodes.join(', '));
    this.submit();
  }
}

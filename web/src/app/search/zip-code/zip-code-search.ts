import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { AddressSearch } from '../address-search';
import { maskZipCode } from './zip-code-mask';

@Component({
  selector: 'app-zip-code-search',
  imports: [ReactiveFormsModule],
  templateUrl: './zip-code-search.html',
  styleUrl: '../search-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ZipCodeSearch {
  private readonly search = inject(AddressSearch);
  private readonly submitted = signal(false);
  private lastSearched = '';

  readonly zipCode = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.pattern(/^\d{5}-\d{3}$/)],
  });

  constructor() {
    this.zipCode.valueChanges.pipe(takeUntilDestroyed()).subscribe((value) => {
      const masked = maskZipCode(value);
      if (masked !== value) this.zipCode.setValue(masked, { emitEvent: false });
      if (masked.length === 9 && masked !== this.lastSearched) this.submit();
    });
  }

  showError(): boolean {
    return this.submitted() && this.zipCode.invalid;
  }

  submit(event?: Event): void {
    event?.preventDefault();
    this.submitted.set(true);
    if (this.zipCode.invalid) return;

    this.lastSearched = this.zipCode.value;
    this.search.byZipCode(this.zipCode.value);
  }

  searchFor(zipCode: string): void {
    this.lastSearched = '';
    this.zipCode.setValue(zipCode);
  }
}

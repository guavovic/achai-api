import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { AddressSearch } from '../address-search';

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

  readonly zipCode = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.pattern(/^\d{5}-?\d{3}$/)],
  });

  showError(): boolean {
    return this.submitted() && this.zipCode.invalid;
  }

  submit(event?: Event): void {
    event?.preventDefault();
    this.submitted.set(true);
    if (this.zipCode.invalid) return;

    this.search.byZipCode(this.zipCode.value.trim());
  }

  searchFor(zipCode: string): void {
    this.zipCode.setValue(zipCode);
    this.submit();
  }
}

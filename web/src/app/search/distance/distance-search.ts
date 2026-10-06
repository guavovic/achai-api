import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AddressSearch } from '../address-search';
import { maskZipCode } from '../zip-code/zip-code-mask';

const ZIP_CODE = Validators.pattern(/^\d{5}-\d{3}$/);

@Component({
  selector: 'app-distance-search',
  imports: [ReactiveFormsModule],
  templateUrl: './distance-search.html',
  styleUrl: '../search-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DistanceSearch {
  private readonly search = inject(AddressSearch);
  private readonly submitted = signal(false);

  readonly form = new FormGroup({
    origin: new FormControl('', { nonNullable: true, validators: [Validators.required, ZIP_CODE] }),
    destination: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, ZIP_CODE],
    }),
  });

  constructor() {
    for (const control of [this.form.controls.origin, this.form.controls.destination]) {
      control.valueChanges.pipe(takeUntilDestroyed()).subscribe((value) => {
        const masked = maskZipCode(value);
        if (masked !== value) control.setValue(masked, { emitEvent: false });
      });
    }
  }

  showError(control: 'origin' | 'destination'): boolean {
    return this.submitted() && this.form.controls[control].invalid;
  }

  submit(event?: Event): void {
    event?.preventDefault();
    this.submitted.set(true);
    if (this.form.invalid) return;

    const { origin, destination } = this.form.getRawValue();
    this.search.byDistance(origin, destination);
  }

  searchFor(origin: string, destination: string): void {
    this.form.setValue({ origin: maskZipCode(origin), destination: maskZipCode(destination) });
    this.submit();
  }
}

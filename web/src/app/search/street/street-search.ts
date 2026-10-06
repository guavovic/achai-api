import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AchaiApi, City } from '../../core/api/achai-api';
import { AddressSearch } from '../address-search';
import { STATES } from '../states';

@Component({
  selector: 'app-street-search',
  imports: [ReactiveFormsModule],
  templateUrl: './street-search.html',
  styleUrl: '../search-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StreetSearch {
  private readonly api = inject(AchaiApi);
  private readonly search = inject(AddressSearch);
  private readonly submitted = signal(false);

  readonly states = STATES;

  readonly form = new FormGroup({
    state: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    city: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(3)],
    }),
    street: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(3)],
    }),
  });

  private readonly selectedState = toSignal(this.form.controls.state.valueChanges, {
    initialValue: '',
  });

  readonly cities = httpResource<City[]>(() => {
    const state = this.selectedState();
    return state ? this.api.citiesUrl(state) : undefined;
  });

  showError(control: 'state' | 'city' | 'street'): boolean {
    return this.submitted() && this.form.controls[control].invalid;
  }

  submit(event?: Event): void {
    event?.preventDefault();
    this.submitted.set(true);
    if (this.form.invalid) return;

    const { state, city, street } = this.form.getRawValue();
    this.search.byStreet(state, city.trim(), street.trim());
  }

  searchFor(state: string, city: string, street: string): void {
    this.form.setValue({ state, city, street });
    this.submit();
  }
}

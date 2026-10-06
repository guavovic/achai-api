import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AchaiApi, City } from '../../core/api/achai-api';
import { AddressSearch } from '../address-search';
import { STATES } from '../states';
import { suggestCities } from './city-suggestions';

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

  private readonly cityText = toSignal(this.form.controls.city.valueChanges, { initialValue: '' });
  private readonly cityListOpen = signal(false);
  readonly activeCity = signal(0);
  readonly citySuggestions = computed(() =>
    suggestCities(
      (this.cities.value() ?? []).flatMap((city) => (city.nome ? [city.nome] : [])),
      this.cityText(),
    ),
  );
  readonly showCityList = computed(() => {
    const suggestions = this.citySuggestions();
    const onlyTheTypedOne = suggestions.length === 1 && suggestions[0].name === this.cityText();
    return this.cityListOpen() && suggestions.length > 0 && !onlyTheTypedOne;
  });

  openCities(): void {
    this.cityListOpen.set(true);
    this.activeCity.set(0);
  }

  closeCities(): void {
    this.cityListOpen.set(false);
  }

  chooseCity(name: string): void {
    this.form.controls.city.setValue(name);
    this.closeCities();
  }

  onCityKeydown(event: KeyboardEvent): void {
    const total = this.citySuggestions().length;

    if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
      event.preventDefault();
      if (!this.showCityList()) return this.openCities();
      const step = event.key === 'ArrowDown' ? 1 : -1;
      this.activeCity.update((index) => (index + step + total) % total);
      setTimeout(() =>
        document
          .getElementById(`cidade-${this.activeCity()}`)
          ?.scrollIntoView({ block: 'nearest' }),
      );
    } else if (event.key === 'Enter' && this.showCityList()) {
      event.preventDefault();
      this.chooseCity(this.citySuggestions()[this.activeCity()].name);
    } else if (event.key === 'Escape') {
      this.closeCities();
    }
  }

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

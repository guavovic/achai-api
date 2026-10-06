import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { AchaiApi, CoordinatesResponse } from '../../core/api/achai-api';

type MapState =
  | { status: 'closed' }
  | { status: 'loading' }
  | { status: 'ready'; coordinates: CoordinatesResponse }
  | { status: 'error' };

const MARGIN = 0.04;

@Component({
  selector: 'app-map-preview',
  templateUrl: './map-preview.html',
  styleUrl: './map-preview.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MapPreview {
  private readonly http = inject(HttpClient);
  private readonly api = inject(AchaiApi);
  private readonly sanitizer = inject(DomSanitizer);

  readonly zipCode = input.required<string>();

  protected readonly state = signal<MapState>({ status: 'closed' });
  protected readonly embedUrl = computed<SafeResourceUrl | null>(() => {
    const state = this.state();
    if (state.status !== 'ready') return null;

    const [latitude, longitude] = [
      Number(state.coordinates.latitude),
      Number(state.coordinates.longitude),
    ];
    const bbox = [
      longitude - MARGIN,
      latitude - MARGIN,
      longitude + MARGIN,
      latitude + MARGIN,
    ].join(',');
    return this.sanitizer.bypassSecurityTrustResourceUrl(
      `https://www.openstreetmap.org/export/embed.html?bbox=${bbox}&layer=mapnik&marker=${latitude},${longitude}`,
    );
  });
  protected readonly openUrl = computed(() => {
    const state = this.state();
    if (state.status !== 'ready') return '';
    const [latitude, longitude] = [
      Number(state.coordinates.latitude),
      Number(state.coordinates.longitude),
    ];
    return `https://www.openstreetmap.org/?mlat=${latitude}&mlon=${longitude}#map=13/${latitude}/${longitude}`;
  });

  protected show(): void {
    this.state.set({ status: 'loading' });
    this.http.get<CoordinatesResponse>(this.api.coordinatesUrl(this.zipCode())).subscribe({
      next: (coordinates) => this.state.set({ status: 'ready', coordinates }),
      error: () => this.state.set({ status: 'error' }),
    });
  }
}

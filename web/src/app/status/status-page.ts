import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { AchaiApi, StatusResponse } from '../core/api/achai-api';

const TIME = new Intl.DateTimeFormat('pt-BR', { hour: '2-digit', minute: '2-digit' });
const DATE_TIME = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' });

@Component({
  selector: 'app-status-page',
  templateUrl: './status-page.html',
  styleUrl: './status-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusPage {
  private readonly url = inject(AchaiApi).statusUrl;

  protected readonly status = httpResource<StatusResponse>(() => this.url);
  protected readonly report = computed(() =>
    this.status.hasValue() ? this.status.value() : undefined,
  );

  protected time(value: string | undefined): string {
    return value ? TIME.format(new Date(value)) : '';
  }

  protected dateTime(value: string | undefined): string {
    return value ? DATE_TIME.format(new Date(value)) : '';
  }
}

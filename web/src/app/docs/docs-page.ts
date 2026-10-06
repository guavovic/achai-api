import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AchaiApi } from '../core/api/achai-api';
import { RoutePath } from '../core/route-path/route-path';
import { OpenApiDocument, schemaId, toApiDocs } from './api-docs';
import { TypeLabel } from './type-label';

@Component({
  selector: 'app-docs-page',
  imports: [RouterLink, RoutePath, TypeLabel],
  templateUrl: './docs-page.html',
  styleUrl: './docs-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DocsPage {
  protected readonly openApiUrl = inject(AchaiApi).openApiUrl;
  protected readonly document = httpResource<OpenApiDocument>(() => this.openApiUrl);
  protected readonly docs = computed(() => {
    const document = this.document.hasValue() ? this.document.value() : undefined;
    return document ? toApiDocs(document) : null;
  });

  protected schemaId(name: string): string {
    return schemaId(name);
  }

  protected statusClass(status: string): string {
    return status.startsWith('2') ? 'ok' : 'fail';
  }
}

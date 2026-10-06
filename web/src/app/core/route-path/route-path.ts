import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-route-path',
  template: `@for (part of parts(); track $index) {
    <span [class.param]="part.param">{{ part.text }}</span>
  }`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoutePath {
  readonly path = input.required<string>();

  protected readonly parts = computed(() =>
    this.path()
      .split(/(\{[^}]+\})/)
      .filter(Boolean)
      .map((text) => ({ text, param: text.startsWith('{') })),
  );
}

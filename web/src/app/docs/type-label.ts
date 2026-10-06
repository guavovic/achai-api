import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

const COLORS: Record<string, string> = {
  string: 'type-string',
  integer: 'type-number',
  number: 'type-number',
  boolean: 'type-boolean',
  null: 'type-null',
};

@Component({
  selector: 'app-type-label',
  template: `@for (part of parts(); track $index) {
    <span [class]="part.kind">{{ part.text }}</span>
  }`,
  styles: `
    .type-string {
      color: var(--achai-kiwi);
    }
    .type-number {
      color: var(--achai-banana);
    }
    .type-boolean {
      color: var(--achai-mirtilo);
    }
    .type-null {
      color: var(--achai-morango);
    }
    .type-model {
      color: var(--gv-color-accent);
    }
    .type-plain {
      color: var(--gv-color-text-muted);
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TypeLabel {
  readonly type = input.required<string>();

  protected readonly parts = computed(() =>
    this.type()
      .split(/([A-Za-z_]\w*)/)
      .filter(Boolean)
      .map((text) => ({
        text,
        kind: /^[A-Za-z_]/.test(text)
          ? (COLORS[text] ?? (text === 'campo' ? 'type-plain' : 'type-model'))
          : 'type-plain',
      })),
  );
}

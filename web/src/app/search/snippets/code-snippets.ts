import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { codeTokens } from './code-tokens';
import { BatchBody, SNIPPET_LANGUAGES, SnippetLanguage, snippet } from './snippets';

@Component({
  selector: 'app-code-snippets',
  templateUrl: './code-snippets.html',
  styleUrl: './code-snippets.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CodeSnippets {
  readonly url = input.required<string>();
  readonly body = input<BatchBody>();

  readonly languages = SNIPPET_LANGUAGES;
  readonly language = signal<SnippetLanguage>('curl');
  readonly copied = signal(false);
  readonly code = computed(() => snippet(this.language(), this.url(), this.body()));
  readonly tokens = computed(() => codeTokens(this.code()));

  async copy(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.code());
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    } catch {}
  }
}

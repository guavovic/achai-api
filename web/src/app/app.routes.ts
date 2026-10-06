import { Routes } from '@angular/router';
import { PlaygroundPage } from './playground/playground-page';

export const routes: Routes = [
  { path: '', component: PlaygroundPage, title: 'achaí-API: endereços do Brasil' },
  {
    path: 'docs',
    loadComponent: () => import('./docs/docs-page').then((m) => m.DocsPage),
    title: 'Documentação · achaí-API',
  },
  { path: '**', redirectTo: '' },
];

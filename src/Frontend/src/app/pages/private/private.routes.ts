import { Routes } from '@angular/router';
import { LinksList } from './links-list/links-list';
import { Preferences } from './preferences/preferences';

export const privateRoutes: Routes = [
  { path: 'links', component: LinksList, pathMatch: 'full' },
  { path: 'preferences', component: Preferences, pathMatch: 'full' }
];

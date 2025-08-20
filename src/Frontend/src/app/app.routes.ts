import { Routes } from '@angular/router';
import { isAuthenticatedGuard } from './auth/auth.guard';
import { LandingPage } from './pages/home/landing-page/landing-page';

export const routes: Routes = [
  { path: '', component: LandingPage, pathMatch: 'full' },
  { path: 'app', canMatch: [isAuthenticatedGuard], loadChildren: () => import('./pages/private/private.routes').then(m => m.privateRoutes) }

];

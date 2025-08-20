import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { OidcSecurityService } from 'angular-auth-oidc-client';

bootstrapApplication(App, appConfig)
  .then(ref => {
    const oidc = ref.injector.get(OidcSecurityService);
    oidc.checkAuth().subscribe();
  })
  .catch((err) => console.error(err));

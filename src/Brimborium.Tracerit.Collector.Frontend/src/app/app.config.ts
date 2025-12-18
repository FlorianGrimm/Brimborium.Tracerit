import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withFetch } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
        //provideZoneChangeDetection({ eventCoalescing: true }), 
        provideZonelessChangeDetection(),
        provideRouter(routes),
        provideHttpClient(withFetch()),
    ]
};

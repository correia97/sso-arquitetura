import { BrowserModule } from '@angular/platform-browser';
import { APP_INITIALIZER, NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthModule, LogLevel, OidcConfigService } from 'angular-auth-oidc-client';
import { HomeComponent } from './home/home.component';
import { ClaimsComponent } from './claims/claims.component';
import { HttpClientModule } from '@angular/common/http';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';

export function configureAuth(oidcConfigService: OidcConfigService) {
  return () =>
      oidcConfigService.withConfig({
          stsServer: 'http://192.168.0.10:8088/auth/realms/Sample',
          redirectUrl: window.location.origin,
          postLogoutRedirectUri: window.location.origin,
          clientId: 'exemplo1',
          scope: 'openid profile email offline_access',
          responseType: 'id_token token',
          silentRenew: true,
            silentRenewUrl: `${window.location.origin}/silent-renew.html`,
          logLevel: LogLevel.Debug,
          postLoginRoute: 'claims'
      });
}

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    ClaimsComponent,
    UnauthorizedComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    AuthModule.forRoot(), HttpClientModule
  ],
  providers: [OidcConfigService,
    {
        provide: APP_INITIALIZER,
        useFactory: configureAuth,
        deps: [OidcConfigService],
        multi: true,
    }],
  bootstrap: [AppComponent]
})
export class AppModule { }

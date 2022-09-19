import { BrowserModule } from '@angular/platform-browser';
import {  NgModule } from '@angular/core';
import { environment } from './../environments/environment';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthModule, LogLevel } from 'angular-auth-oidc-client';
import { HomeComponent } from './home/home.component';
import { ClaimsComponent } from './claims/claims.component';
import { HttpClientModule } from '@angular/common/http';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
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
    AuthModule.forRoot({
      config: {
        authority: environment.authBaseUrl,
        authWellknownEndpointUrl:environment.authBaseUrl+environment.complement+'/.well-known/openid-configuration',
        redirectUrl: window.location.origin,
        postLogoutRedirectUri: window.location.origin,
        clientId: environment.clientAuth,
        scope: 'openid profile email offline_access',
        responseType: 'id_token token',
        silentRenew: true,
        silentRenewUrl: `${window.location.origin}/silent-renew.html`,
        logLevel: LogLevel.Debug,
        useRefreshToken: true,
        postLoginRoute: 'claims',
        customParamsAuthRequest: {
          audience: environment.audience
        },
        customParamsRefreshTokenRequest: {
          scope: 'openid profile email offline_access',
        },
      }
    }),
    HttpClientModule,
    NgbModule
  ],
  exports: [AuthModule],
  bootstrap: [AppComponent]
})
export class AppModule { }

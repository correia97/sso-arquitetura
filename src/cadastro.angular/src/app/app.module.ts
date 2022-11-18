import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { environment } from './../environments/environment';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthModule, LogLevel } from 'angular-auth-oidc-client';
import { HomeComponent } from './home/home.component';
import { ClaimsComponent } from './claims/claims.component';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { HomeComponent as FuncionarioHomeComponent } from './funcionario/home/home.component';
import { AddComponent as FuncionarioAddComponent } from './funcionario/add/add.component';
import { EditComponent as FuncionarioEditComponent } from './funcionario/edit/edit.component';
import { RouteService } from './service/route.service';
import { RequestInteceptorService } from './service/request-inteceptor.service';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

export const httpInterceptorProviders = [
  { provide: HTTP_INTERCEPTORS, useClass: RequestInteceptorService, multi: true },
];

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    ClaimsComponent,
    UnauthorizedComponent,
    FuncionarioHomeComponent,
    FuncionarioAddComponent,
    FuncionarioEditComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    AuthModule.forRoot({
      config: {
        authority: environment.authBaseUrl,
        authWellknownEndpointUrl: environment.authBaseUrl + environment.complement + '/.well-known/openid-configuration',
        redirectUrl: window.location.origin,
        postLogoutRedirectUri: window.location.origin,
        clientId: environment.clientAuth,
        scope: 'openid profile email offline_access',
        responseType: 'id_token token',
        silentRenew: true,
        silentRenewUrl: `${window.location.origin}/silent-renew.html`,
        logLevel: LogLevel.Debug,
        useRefreshToken: true,
        postLoginRoute: window.location.origin,
        customParamsAuthRequest: {
          audience: environment.audience
        },
        customParamsRefreshTokenRequest: {
          scope: 'openid profile email offline_access',
        },
      }
    }),
    HttpClientModule,
    NgbModule,
    FormsModule,
    ReactiveFormsModule
  ],
  exports: [AuthModule],
  bootstrap: [AppComponent],
  providers:[httpInterceptorProviders, RouteService]
})
export class AppModule { }

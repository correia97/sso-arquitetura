import { Component, OnInit } from '@angular/core';
import { EventTypes, OidcSecurityService, PublicEventsService } from 'angular-auth-oidc-client';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  constructor(public oidcSecurityService: OidcSecurityService) {
    this.oidcSecurityService.checkAuth()
      .subscribe((isAuthenticated) =>
        console.log('app authenticated', isAuthenticated));
  }
}

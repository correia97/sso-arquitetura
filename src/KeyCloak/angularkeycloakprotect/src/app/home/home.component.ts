import { Component, OnInit } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  title = 'Home';

    constructor(public oidcSecurityService: OidcSecurityService) {
      this.oidcSecurityService.checkAuth().subscribe((auth) => console.log('is authenticated', auth));
    }

    ngOnInit(): void {
    }

    login() {
        this.oidcSecurityService.authorize();
    }

    logout() {
        this.oidcSecurityService.logoff();
    }

}

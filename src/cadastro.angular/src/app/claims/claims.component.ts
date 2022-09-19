import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';

import  jwd_decode from 'jwt-decode';

@Component({
  selector: 'app-claims',
  templateUrl: './claims.component.html',
  styleUrls: ['./claims.component.css']
})
export class ClaimsComponent implements OnInit {

  payload: any;
  jwt: any;
  idToken: any;
  
  isAuthenticated: boolean = false;
  constructor(public oidcSecurityService: OidcSecurityService, public router: Router) {
    this.oidcSecurityService.checkAuth()
    .subscribe((auth) => console.log('is authenticated', auth));
  }

  ngOnInit(): void {

    this.oidcSecurityService.isAuthenticated$.subscribe({
      next: x => {
        this.isAuthenticated = x.isAuthenticated
        if (this.isAuthenticated) {
          
          console.log('-----------------------------------------this.oidcSecurityService---------------------------------------------');
          console.log(this.oidcSecurityService);
          let pay =  this.oidcSecurityService?.getPayloadFromIdToken();
 
          console.log('------------------------------payload------------------------------------------------------' );
          console.log(pay );
          this.oidcSecurityService?.getAccessToken()
                                   .subscribe((token) => {
            console.log(token);
            this.jwt = jwd_decode(token);
          });
          this.oidcSecurityService?.getIdToken()
                                   .subscribe((token) => {
            console.log(token);
            this.idToken = jwd_decode(token);

          });
          console.log('----------------------------------------jwt----------------------------------------------------');
          console.log(this.jwt );
          this.payload = pay;
        } else {
          this.router.navigate(['/unauthorized']);
        }
      },
      error: err => console.error('Observer got an error: ' + err),
      complete: () => console.log('Observer got a complete notification'),

    })

  }

}

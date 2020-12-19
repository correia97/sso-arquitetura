import { Component, OnInit } from '@angular/core';
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
  constructor(public oidcSecurityService: OidcSecurityService) {
    this.oidcSecurityService.checkAuth()
    .subscribe((auth) => console.log('is authenticated', auth));
  }

  ngOnInit(): void {
  let pay =  this.oidcSecurityService.getPayloadFromIdToken();
 
  console.log( 'payload' );
  console.log( pay );
 
  this.jwt = jwd_decode(this.oidcSecurityService.getToken());
  this.idToken = jwd_decode(this.oidcSecurityService.getIdToken());
  console.log( 'jwt');
  console.log(  this.jwt );
  this.payload = pay;
  }

}

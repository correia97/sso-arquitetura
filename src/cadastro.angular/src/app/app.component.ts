import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import {  OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent  implements OnInit {
  isAuthenticated: boolean = false;
  constructor(public oidcSecurityService: OidcSecurityService, private router: Router) {
    this.getAuthentication(); 
  }
  ngOnInit(): void {
    this.getAuthentication();
  }

  getAuthentication(){
    this.oidcSecurityService.checkAuth()
    .subscribe((auth) => {
      this.isAuthenticated = auth.isAuthenticated;
      console.log('app authenticated', auth);
    });
  }

  logout(){
    this.oidcSecurityService.logoff();
    this.router.navigate(['/home']);
  }
}

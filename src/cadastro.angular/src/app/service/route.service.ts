import { Injectable } from '@angular/core';
import { CanActivate, RouterStateSnapshot, ActivatedRouteSnapshot, Router, ActivatedRoute, UrlTree } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RouteService implements CanActivate {

  isAuthenticated: boolean = false;

  constructor(public oidcSecurityService: OidcSecurityService, private router: Router) {
    this.oidcSecurityService
      .checkAuth()
      .subscribe((auth) => {
        this.isAuthenticated = auth.isAuthenticated;
        console.log('is authenticated', auth)
      });
  }
  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | UrlTree | Observable<boolean | UrlTree> | Promise<boolean | UrlTree> {
    if (!this.isAuthenticated) {      
      this.oidcSecurityService.authorize();
     // this.router.navigate(['/unauthorized'])
    }
    return this.isAuthenticated;
  }
}

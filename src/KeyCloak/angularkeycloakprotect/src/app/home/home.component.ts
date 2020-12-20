import { Component, OnInit } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Observable } from 'rxjs';
import { Forecast } from '../models/forecast';
import { ApiclientService } from '../service/apiclient.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  title = 'Home';
  forecast: Forecast[] | undefined;
  isAuthenticated: boolean = false;

  constructor(public oidcSecurityService: OidcSecurityService, public service: ApiclientService) {
    this.oidcSecurityService.checkAuth().subscribe((auth) => console.log('is authenticated', auth));
  }

  ngOnInit(): void {
    this.oidcSecurityService.isAuthenticated$.subscribe({
      next: x => {
        this.isAuthenticated = x
        if (this.oidcSecurityService.isAuthenticated$) {
          this.service.getWeatherForecast()
            .subscribe({
              next: result => this.forecast = result,
              error: er => console.log(er)
            });
        }
      },
      error: err => console.error('Observer got an error: ' + err),
      complete: () => console.log('Observer got a complete notification'),

    })
  }

  login() {
    this.oidcSecurityService.authorize();
  }

  logout() {
    this.oidcSecurityService.logoff();
  }

}

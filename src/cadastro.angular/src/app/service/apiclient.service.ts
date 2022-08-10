import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Forecast } from '../models/forecast';

import { environment } from './../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiclientService {

  constructor(private http: HttpClient, public oidcSecurityService: OidcSecurityService) { }

  private getHeader(): any {

    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: 'bearer ' + this.oidcSecurityService.getAccessToken()
      })
    };
    return httpOptions;
  }


  getWeatherForecast(): Observable<any> {
    return this.http.get(environment.apiBaseUrl + '/api/WeatherForecast/authorization', this.getHeader())
      .pipe(
        catchError(this.handleError)
      );
  }


  private handleError(error: HttpErrorResponse) {
    if (error.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      console.error('An error occurred:', error.error.message);
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong.
      console.error(
        `Backend returned code ${error.status}, ` +
        `body was: ${error.error}`);
    }
    // Return an observable with a user-facing error message.
    return throwError(
      'Something bad happened; please try again later.');
  }
}

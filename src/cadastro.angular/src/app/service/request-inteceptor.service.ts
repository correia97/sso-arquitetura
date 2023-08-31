import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest, HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { environment } from 'src/environments/environment';
@Injectable({
  providedIn: 'root'
})
export class RequestInteceptorService implements HttpInterceptor {
  private currentToken: string = "";
  constructor(private http: HttpClient, public oidcSecurityService: OidcSecurityService) {
this.getToken();
  }

  getToken(){

    this.oidcSecurityService
      .getAccessToken()
      .subscribe((token) => {
        this.currentToken = token;
      });
  }

  private generate_guid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.url.indexOf(environment.authBaseUrl) < 0) {
      this.getToken();
      req = req.clone({
        setHeaders: {
          'Content-Type': 'application/json',
          'Authorization': `bearer ${this.currentToken}`,
          'correlationId': this.generate_guid()
        }
      });
    }
    return next.handle(req);
  }
}

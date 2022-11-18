import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { FuncionarioRequest } from 'src/app/models/request/funcionarioRequest';

import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiclientService {

  constructor(private http: HttpClient) { }

  getWeatherForecast(): Observable<any> {
    return this.http.get(environment.apiBaseUrl + '/api/WeatherForecast/authorization')
      .pipe(
        retry(2), // retry a failed request up to 3 times
        catchError(this.handleError)
      );
  }

  listarFuncionarios(paginaAtual: number, qtdItensPorPaginas: number): Observable<any> {
    return this.http.get(environment.apiBaseUrl + `/api/v1/Funcionario/funcionario/pagina/${paginaAtual}/qtdItens/${qtdItensPorPaginas}`)
      .pipe(
        retry(2), // retry a failed request up to 3 times
        catchError(this.handleError)
      );
  }

  recuperarFuncionario(id: string): Observable<any> {
    return this.http.get(environment.apiBaseUrl + `/api/v1/Funcionario/funcionario/${id}`)
      .pipe(
        retry(2), // retry a failed request up to 3 times
        catchError(this.handleError)
      );
  }

  cadastrarFuncionario(funcionario: FuncionarioRequest): Observable<any> {
    return this.http.post<FuncionarioRequest>(environment.apiBaseUrl + '/api/v1/Funcionario/funcionario', funcionario)
      .pipe(
        retry(2), // retry a failed request up to 3 times
        catchError(this.handleError)
      );
  }

  atualizarFuncionario(funcionario: FuncionarioRequest): Observable<any> {
    return this.http.patch<FuncionarioRequest>(environment.apiBaseUrl + '/api/v1/Funcionario/funcionario', funcionario)
      .pipe(
        retry(2), // retry a failed request up to 3 times
        catchError(this.handleError)
      );
  }

  removerFuncionario(id: string): Observable<any> {
    return this.http.delete(environment.apiBaseUrl + `/api/v1/Funcionario/funcionario/${id}`)
      .pipe(
        retry(2), // retry a failed request up to 3 times
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
    return throwError(()=> new Error('Something bad happened; please try again later.'));
  }
}

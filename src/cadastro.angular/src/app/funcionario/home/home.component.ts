import { Component, OnInit } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { filter, Observable } from 'rxjs';

import { Response } from 'src/app/models/response/response';

import { FuncionarioResponse } from 'src/app/models/response/funcionarioResponse';
import { ApiclientService } from 'src/app/service/apiclient.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})

export class HomeComponent implements OnInit {
  title: string = 'Home';
  funcionarios: Response<FuncionarioResponse[]> | undefined;
  page: number = 0;
  pages: Array<any> | undefined;
  qtd: number = 10;
  isAuthenticated: boolean = false;

  constructor(private router: Router, private route: ActivatedRoute, public oidcSecurityService: OidcSecurityService, public service: ApiclientService) {
    this.oidcSecurityService.checkAuth().subscribe((auth) => console.log('is authenticated', auth));
  }

  ngOnInit(): void {
    this.oidcSecurityService.isAuthenticated$.subscribe({
      next: x => this.isAuthenticated = x.isAuthenticated,
      error: err => console.error('Observer got an error: ' + err),
      complete: () => console.log('Observer got a complete notification'),
    });

    this.route.queryParams.pipe(
      filter(params => params['page']))
      .subscribe(params => {
        this.page = params['page'];
      });

    this.route.queryParams.pipe(
      filter(params => params['qtd']))
      .subscribe(params => {
        this.qtd = params['qtd'];
      });

    if (this.isAuthenticated) {
      this.listarFuncionarios();
    }
  }

  listarFuncionarios() {
    console.log('this.page');
    console.log(this.page);
    console.log('this.qtd');
    console.log(this.qtd);
    let currentPage = this.page - 1 >= 0 ? this.page - 1 : 0;
    this.service.listarFuncionarios(currentPage, this.qtd)
      .subscribe({
        next: result => {
          this.funcionarios = result;
          this.calcPagination();
        },
        error: er => console.log(er)
      });
  }

  calcPagination() {
    debugger;
    this.pages = [];
    let currentPage = Number.parseInt(this.page.toString());
    let totalPages = this.funcionarios == undefined ? 1 : this.funcionarios?.qtdTotalPaginas;
    let contador = currentPage - 5 > 0 ? currentPage - 5 : 1;
    let lastPage = currentPage + 5 < totalPages ? currentPage + 5 : totalPages;

    for (let i = contador; i <= lastPage; i++) {
      this.pages.push({ page: i, isActive: i === currentPage });
    }
  }

  goToPage(page: number, qtd: number) {
    this.page = page;
    this.qtd = qtd;
    this.router.navigate(['/funcionario'], { queryParams: { page, qtd } });
    this.listarFuncionarios();
  }

}

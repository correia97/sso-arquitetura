import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { filter, pipe } from 'rxjs';
import { FuncionarioResponse } from 'src/app/models/response/funcionarioResponse';
import { Response } from 'src/app/models/response/response';
import { ApiclientService } from 'src/app/service/apiclient.service';
import { NgbNavChangeEvent, NgbNavConfig } from '@ng-bootstrap/ng-bootstrap';
import { FuncionarioRequest } from 'src/app/models/request/funcionarioRequest';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.css'],
  providers: [NgbNavConfig]
})
export class EditComponent implements OnInit {

  funcionario: Response<FuncionarioResponse> | undefined;
  funcionarioForm!: FormGroup;
  isAuthenticated: boolean = false;
  userId: string = "";
  ativo: boolean = false;
  tabActive: number = 1;
  disabled: boolean = true;

  constructor(private router: Router, private route: ActivatedRoute,
    public oidcSecurityService: OidcSecurityService, public service: ApiclientService, config: NgbNavConfig) {
    this.oidcSecurityService.checkAuth().subscribe((auth) => console.log('is authenticated', auth));
    config.destroyOnHide = false;
    config.roles = false;
  }

  ngOnInit(): void {
    this.oidcSecurityService.isAuthenticated$.subscribe({
      next: x => this.isAuthenticated = x.isAuthenticated,
      error: err => console.error('Observer got an error: ' + err),
      complete: () => console.log('Observer got a complete notification'),
    });

    this.route.params
      .subscribe(params => {
        this.userId = params['id'];
      });
    this.loadUser();
  }

  loadUser() {
    this.service.recuperarFuncionario(this.userId)
      .subscribe({
        next: result => {
          this.funcionario = result;
          this.fillForm();
        },
        error: er => console.log(er)
      });
  }

  fillForm() {
    this.ativo = this.funcionario == undefined ? false : this.funcionario?.data.ativo;
    this.funcionarioForm = new FormGroup({
      'nome': new FormControl(this.funcionario?.data.nome, [Validators.required]),
      'sobrenome': new FormControl(this.funcionario?.data.sobreNome, [Validators.required]),
      'cargo': new FormControl(this.funcionario?.data.cargo),
      'userId': new FormControl(this.funcionario?.data.userId, [Validators.required]),
      'matricula': new FormControl(this.funcionario?.data.matricula),
      'email': new FormControl(this.funcionario?.data.email, [Validators.required]),
      'dataNascimento': new FormControl(this.funcionario?.data.dataNascimento),
      'status': new FormControl(this.funcionario?.data.ativo),
      'idResidencial': new FormControl(this.funcionario?.data.enderecoResidencial?.id),
      'cepResidencial': new FormControl(this.funcionario?.data.enderecoResidencial?.cep),
      'ruaResidencial': new FormControl(this.funcionario?.data.enderecoResidencial?.rua),
      'numeroResidencial': new FormControl(this.funcionario?.data.enderecoResidencial?.numero),
      'complementoResidencial': new FormControl(this.funcionario?.data.enderecoResidencial?.complemento),
      'bairroResidencial': new FormControl(this.funcionario?.data.enderecoResidencial?.bairro),
      'cidadeResidencial': new FormControl(this.funcionario?.data.enderecoResidencial?.cidade),
      'ufResidencial': new FormControl(this.funcionario?.data.enderecoResidencial?.uf),
      'idComercial': new FormControl(this.funcionario?.data.enderecoComercial?.id),
      'cepComercial': new FormControl(this.funcionario?.data.enderecoComercial?.cep),
      'ruaComercial': new FormControl(this.funcionario?.data.enderecoComercial?.rua),
      'numeroComercial': new FormControl(this.funcionario?.data.enderecoComercial?.numero),
      'complementoComercial': new FormControl(this.funcionario?.data.enderecoComercial?.complemento),
      'bairroComercial': new FormControl(this.funcionario?.data.enderecoComercial?.bairro),
      'cidadeComercial': new FormControl(this.funcionario?.data.enderecoComercial?.cidade),
      'ufComercial': new FormControl(this.funcionario?.data.enderecoComercial?.uf),
    });
  }

  updateFuncionario() {
    if (this.funcionario?.data == undefined)
      return;
      
    let funci: FuncionarioResponse = this.funcionario?.data;
    let request = new FuncionarioRequest(funci);
    this.service.atualizarFuncionario(request)
      .subscribe({
        next: result => {
          console.log(result)
        },
        error: er => console.log(er)
      });
  }
}

import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ClaimsComponent } from './claims/claims.component';
import { HomeComponent } from './home/home.component';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { HomeComponent as FuncionarioHomeComponent } from './funcionario/home/home.component';
import { AddComponent as FuncionarioAddComponent } from './funcionario/add/add.component';
import { EditComponent as FuncionarioEditComponent } from './funcionario/edit/edit.component';
import { RouteService } from './service/route.service';

const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'home', component: HomeComponent },
  { path: 'unauthorized', component: UnauthorizedComponent },
  { path: 'claims', canActivate: [RouteService], component: ClaimsComponent },
  { path: 'funcionario', canActivate: [RouteService], component: FuncionarioHomeComponent },
  { path: 'funcionario/home', canActivate: [RouteService], component: FuncionarioHomeComponent },
  { path: 'funcionario/edit/:id', canActivate: [RouteService], component: FuncionarioEditComponent },
  { path: 'funcionario/add', canActivate: [RouteService], component: FuncionarioAddComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

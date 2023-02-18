import { Component, NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home-component/home.component';
import { LoginComponent } from './login-component/login.component';
import { RegisterComponent } from './register-component/register.component';
import { EmailConfirmationComponent } from './email-confirmation-component/email-confirmation.component';
import { AuthenticationCompleteComponent } from './authentication-complete-component/authentication-complete.component';
import { AuthenticationGuard } from './guards/authentication.guard';
import { AuthComponent } from './auth-component/auth.component';
import { WorkspaceComponent } from './workspace-component/workspace.component';
import { RoleGuard } from './guards/role.guard';
import { CedCountryComponent } from './ced-country-component/ced-country.component';
import { CedDepartmentComponent } from './ced-department-component/ced-department.component';
import { SessionGuard } from './guards/session.guard';
import { SessionModalComponent } from './session-modal-component/session-modal.component';

const routes: Routes = [
  {path: '', redirectTo: 'home', pathMatch: 'full'},
  {path: 'session-check', component: SessionModalComponent, pathMatch: 'full'},
  {path: 'home', component: HomeComponent, pathMatch: 'full', canActivate: [SessionGuard]},
  {path: 'login', component: LoginComponent, pathMatch: 'full'},
  {path: 'register', component: RegisterComponent, pathMatch: 'full'},
  {path: 'workspace', component: WorkspaceComponent, pathMatch: 'full', canActivate: [AuthenticationGuard, SessionGuard]},
  {path: 'email-confirmation/:token&:email', component: EmailConfirmationComponent, pathMatch: 'full', canActivate: [SessionGuard]},
  {path: 'authentication-complete', component: AuthenticationCompleteComponent, pathMatch: 'full', canActivate: [AuthenticationGuard, SessionGuard]},
  {path: 'auth', pathMatch: 'full', component: AuthComponent, canActivate: [SessionGuard]},
  {path: 'cedcountry', component: CedCountryComponent, pathMatch: 'full', canActivate: [AuthenticationGuard, RoleGuard, SessionGuard], data: {role: 'FGMEmployee'}},
  {path: 'ceddepartment', component: CedDepartmentComponent, pathMatch: 'full', canActivate: [AuthenticationGuard, RoleGuard, SessionGuard], data: {role: 'FGMEmployee'}}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})

export class AppRoutingModule { }

import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RegisterComponent } from './register-component/register.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ErrorModalComponent } from './error-modal-component/error-modal.component';
import { EmailConfirmationComponent } from './email-confirmation-component/email-confirmation.component';
import { LoginComponent } from './login-component/login.component';
import { HomeComponent } from './home-component/home.component';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { HeaderComponent } from './header-component/header.component';
import { AuthenticationCompleteComponent } from './authentication-complete-component/authentication-complete.component';
import { AuthComponent } from './auth-component/auth.component';
import { WorkspaceComponent } from './workspace-component/workspace.component';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SessionModalComponent } from './session-modal-component/session-modal.component';
import { CookieModalComponent } from './cookie-modal-component/cookie-modal.component'
import { HttpInterceptorInterceptor } from './interceptor/http-interceptor.interceptor';
import { ErrorService } from './services/error.service';

@NgModule({
  declarations: [
    AppComponent,
    RegisterComponent,
    ErrorModalComponent,
    EmailConfirmationComponent,
    LoginComponent,
    HomeComponent,
    HeaderComponent,
    AuthenticationCompleteComponent,
    AuthComponent,
    WorkspaceComponent,
    SessionModalComponent,
    CookieModalComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    NgbModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    MatExpansionModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    HttpClientModule,
  ],
  providers: [{ provide : HTTP_INTERCEPTORS, useClass: HttpInterceptorInterceptor, multi: true }, ErrorService],
  entryComponents: [ErrorModalComponent],
  bootstrap: [AppComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AppModule { }

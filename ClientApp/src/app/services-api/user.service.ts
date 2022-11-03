import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ErrorModalComponent } from '../error-modal-component/error-modal.component';
import { LoginUser } from '../interfaces/LoginUser';
import { environment } from 'src/environments/environment';
import { catchError } from 'rxjs/operators';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { RegistrationUser } from '../interfaces/registrationUser';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  public REST_API_SERVER = environment._VARIABLE_HOST;
  errorMessage: any;

  constructor(
    private readonly httpClient: HttpClient,
    private readonly modalService: NgbModal
  ) {}
  
  handleError(error: HttpErrorResponse){
    console.log(error);
    return throwError(error);
  }

  public Login(loginResponse: LoginUser) {
    return this.httpClient.post(this.REST_API_SERVER+'/User/Login', loginResponse, {headers: null}).pipe(catchError(this.handleError));
  }

  public SignUp(registerUser: RegistrationUser){
    return this.httpClient.post(this.REST_API_SERVER+'/User/Signup', registerUser, {headers: null}).pipe(catchError(this.handleError));
  }

  public confirmationRegistration(token: string, emailUser: string) {
    return this.httpClient.get(this.REST_API_SERVER+'/User/ConfirmEmail?token='+token+'&email='+emailUser, {headers: null}).pipe(catchError(this.handleError));
  }

  public logoutUser(){
    return this.httpClient.get(this.REST_API_SERVER+'/User/Logout', {headers: null}).pipe(catchError(this.handleError));
  }

  public checkSessionUser(){
    return this.httpClient.get(this.REST_API_SERVER+'/User/CheckSession', {headers: null}).pipe(catchError(this.handleError));
  }
}

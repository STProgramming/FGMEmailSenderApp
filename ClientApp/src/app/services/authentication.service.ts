import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AuthenticationGuard } from '../guards/authentication.guard';
import { LoginUser } from '../interfaces/loginUser';
import { RouterService } from './router.service';
import { TokenService } from './token.service';
@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {
  _authenticated: Observable<boolean> = this.authGuard.$authentication;
  public REST_API_SERVER = environment._VARIABLE_HOST;
  public apiIdentityUserEndPoint = '/api/Identity/User/'

  constructor(
    private readonly tokenService : TokenService,
    private readonly routerService: RouterService,
    private readonly authGuard: AuthenticationGuard,
    private readonly httpClient: HttpClient
    ) { 
      this._authenticated.subscribe(
        (event:any)=>{
          if(!event){
            this.routerService.redirectToAuthentication();
          }
        }
      )
    }

  async loginAuthentication (login: LoginUser) {
    await this.httpClient.post(this.REST_API_SERVER+this.apiIdentityUserEndPoint+'Login', login).subscribe(
      (data: any) =>{
        if(data){
          let stringData = JSON.stringify(data);
          let parseData = JSON.parse(stringData);
          if(parseData.message == 'success'){
            this.tokenService.successLoginSetInfo(parseData.email, parseData.roles, parseData.nameUser);
            this.routerService.redirectToAuthentication();
          }
        }
      }
    )
  }

  async resumeAuthentication(){
    await this.httpClient.get(this.REST_API_SERVER+this.apiIdentityUserEndPoint+'CheckAuth').subscribe(
      (data:any)=>{
        if(data){
          if(data.message == 'success'){
            this.tokenService.successLoginSetInfo(data.email, data.roles, data.name);
          }
          else{
            this.tokenService.removeTokenRequest();
          }
        }
      }
    );
  }

  async logoutUser(){
    await this.httpClient.get(this.REST_API_SERVER+this.apiIdentityUserEndPoint+'Logout').subscribe(
      (data: any) => {
        if(data){
          this.tokenService.removeTokenRequest();
          this.routerService.redirectToAuthentication();
        }
        else{
          this.routerService.redirectToAuthentication();
        }
    });
  }

  getNameUser(): string{
    return this.tokenService.getNameUserToken();
  }

}

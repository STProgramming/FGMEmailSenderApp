import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthenticationGuard } from '../guards/authentication.guard';
import { LoginUser } from '../interfaces/LoginUser';
import { UserService } from '../services-api/user.service';
import { RouterService } from './router.service';
import { TokenService } from './token.service';
@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {
  _authenticated: Observable<boolean> = this.authGuard.$authentication;

  constructor(
    private readonly userServiceApi : UserService,
    private readonly tokenService : TokenService,
    private readonly routerService: RouterService,
    private readonly authGuard: AuthenticationGuard
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
    await this.userServiceApi.Login(login).subscribe(
      (data: any) =>{
        if(data){
          let stringData = JSON.stringify(data);
          let parseData = JSON.parse(stringData);
          if(parseData.message == 'success'){
            this.tokenService.successLoginSetInfo(parseData.email, parseData.roles, parseData.name);
            this.routerService.redirectToAuthentication();
          }
        }
      }
    )
  }

  async resumeAuthentication(){
    await this.userServiceApi.checkAuthenticationUser().subscribe(
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
    await this.userServiceApi.logoutUser().subscribe(
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

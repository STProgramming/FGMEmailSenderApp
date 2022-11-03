import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  nameToken: string = environment._FGMTOKEN 

  constructor(
    private readonly tokenService: TokenService
  ) { }

  private getUserRolesInSession(): any{
    let stringToken = localStorage.getItem(this.nameToken);
    stringToken = stringToken.replace(/\\/g, '');
    let parseToken = JSON.parse(stringToken);
    return this.tokenService.publicDecode(parseToken.UserCredential);
  }

  roleRequired(roleRequired: string): boolean{
    let roleUser: any = this.getUserRolesInSession();
    let flagMatch: boolean = false;
    if(roleRequired.includes(roleUser)){
      flagMatch = true;
    }  
    return flagMatch;
  }
}

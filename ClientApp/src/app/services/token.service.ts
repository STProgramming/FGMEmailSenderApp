import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { FormatToken } from '../interfaces/TokenFormat';
@Injectable({
  providedIn: 'root'
})

export class TokenService {
  private NAMETOKEN = environment._FGMTOKEN;

  constructor() { }

  successLoginSetInfo(emailUser: string ,rolesClaimed: any, nameUser: string){
    let encodedToken: FormatToken;
    let emailEnc: string = this.encodingString(emailUser);
    let rolesUser: any;   
    for(let i = 0; i < rolesClaimed.length; i++){
      rolesUser = this.encodingString(rolesClaimed[i]);
    };
    let date = new Date();
    //il piu' 1 sta a simboleggiare che la data di scadenza del token ha durata di un giorno
    encodedToken = {
      Email: emailEnc,
      UserCredential: rolesUser,
      Name: nameUser,
      DateCreation: date.getDate(),
      DateExpire: date.getDate()+1
    };
    let stringEncToken = JSON.stringify(encodedToken);
    localStorage.setItem(this.NAMETOKEN, stringEncToken);
  }

  public publicDecode(inputEncode: any): any{
    return this.decodingValor(inputEncode);
  }

  private encodingString(inputString: string){
    return btoa(inputString);
  }

  private decodingValor(encodedValues: any): any{
    return atob(encodedValues);
  }

  public removeTokenRequest(){
    this.removeToken();
  }

  private removeToken(){
    localStorage.removeItem(this.NAMETOKEN);
  }

  public getNameUserToken(): string{
    let stringToken = localStorage.getItem(this.NAMETOKEN);
    stringToken = stringToken.replace(/\\/g, '');
    let parseToken = JSON.parse(stringToken);
    return parseToken.Name;
  }
}

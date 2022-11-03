import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CookieService {
  _NAMECOOKIE: string = environment._FGMCOOKIE;
  constructor() { }

  cookiePolicyService(){
    let date = new Date();
    let valueCookieAgreement = {
      "serviceFGM": "cookiePolicy",
      "result": true,
      "url": "check-session",
      "date": date.getDate()
    };
    localStorage.setItem(this._NAMECOOKIE + 'cookiePolicy', JSON.stringify(valueCookieAgreement)); 
  }

  cookieDelete(cookieService: string){
    localStorage.removeItem(this._NAMECOOKIE + cookieService);
  }

  cookieGetValue(cookieService: string){
    let cookieValue = localStorage.getItem(this._NAMECOOKIE + cookieService);
    if(cookieValue != null){
      return cookieValue;
    }
    else{
      return null;
    }
  }
}

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { RouterService } from '../services/router.service';
import { SessionService } from '../services/session.service';

@Injectable({
  providedIn: 'root'
})
export class SessionGuard implements CanActivate {
  NAME_SESSION: string = environment._FGMSESSION;
  sessionFlag$ = new BehaviorSubject<boolean>(false);
  
  constructor(private readonly routerService: RouterService){}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    if(sessionStorage.getItem(this.NAME_SESSION) !== null){
      let flag = this.antiForgerySession();
      this.sessionFlag$.next(flag);
      return flag;      
    }
    else{
      this.routerService.redirectToSessionCheck();
      this.sessionFlag$.next(false);
      return false;
    }
  }

  antiForgerySession(){
    let parseSession = JSON.parse(sessionStorage.getItem(this.NAME_SESSION));
    if(parseSession.dateBegin < parseSession.dateEnd){      
      if(parseSession.idUser !== null && parseSession.cookieAgreementPolicy == true){
        return true;
      }
      else{
        return false;
      }
    }
    else{
      return false;
    }
  }

}

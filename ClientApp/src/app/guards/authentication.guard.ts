import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot, UrlTree } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AuthenticationService } from '../services/authentication.service';
@Injectable({
  providedIn: 'root'
})
export class AuthenticationGuard implements CanActivate {
  $authentication = new BehaviorSubject<boolean>(false);
  NAMETOKEN:string = environment._FGMTOKEN;

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
      let statusAuthGuard = this.checkAntiForgeryTokenIdentity();
      this.$authentication.next(statusAuthGuard);
      return statusAuthGuard;
    }
    
    checkAntiForgeryTokenIdentity(){
      if(localStorage.getItem(this.NAMETOKEN) != null){
        //let stringToken = JSON.stringify(localStorage.getItem(this.NAMETOKEN));
        let stringToken = localStorage.getItem(this.NAMETOKEN);
        stringToken = stringToken.replace(/\\/g, '');
        let parseToken = JSON.parse(stringToken);
        if(parseToken.DateCreation != parseToken.DateExpire || parseToken.DateCreation < parseToken.DateExpire){
          return true;
        }
        else{
          return false;
        }
      }
      return false;
    }
  }



import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { RoleService } from '../services/role.service';
import { TokenService } from '../services/token.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {
  NAMETOKEN: string = environment._FGMTOKEN;
  roles: string[] = environment._ROLES;

  constructor(private readonly roleService: RoleService){}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    if(localStorage.getItem(this.NAMETOKEN) != null){
      return this.roleService.roleRequired(route.data.role);
    }
    return false;
  }
}

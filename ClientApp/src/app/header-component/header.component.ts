import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthenticationGuard } from '../guards/authentication.guard';
import { SessionGuard } from '../guards/session.guard';
import { AuthenticationService } from '../services/authentication.service';
import { RouterService } from '../services/router.service';
import { TokenService } from '../services/token.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {
  _authentication: Observable<boolean> = this.authGuard.$authentication;
  _sessionExist: Observable<boolean> = this.sessionGuard.sessionFlag$;

  constructor(
    private readonly authGuard: AuthenticationGuard,
    private readonly authService: AuthenticationService,
    private readonly sessionGuard: SessionGuard,
    private readonly routerService: RouterService) {}

  ngOnInit(): void {}

  logout(){
    this.authService.logoutUser();
  }

  redirectLogin(){
    this.routerService.redirectLogin();
  }

  redirectRegister(){
    this.routerService.redirectRegister();
  }

  redirectWorkspace(){
    this.routerService.redirectWorkspace();
  }
}

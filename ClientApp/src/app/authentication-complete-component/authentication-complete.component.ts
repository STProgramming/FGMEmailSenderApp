import { Component, OnInit } from '@angular/core';
import { waitForAsync } from '@angular/core/testing';
import { Router } from '@angular/router';
import { delay } from 'rxjs/operators';
import { AuthenticationService } from '../services/authentication.service';
import { RouterService } from '../services/router.service';

@Component({
  selector: 'app-authentication-complete',
  templateUrl: './authentication-complete.component.html',
  styleUrls: ['./authentication-complete.component.scss']
})
export class AuthenticationCompleteComponent implements OnInit {
  nameUser: string | void;

  constructor(private readonly authService: AuthenticationService,
    private readonly routerService: RouterService) { }

  async ngOnInit(){
    this.nameUser = this.authService.getNameUser();
    setTimeout(()=>{
      this.routerService.redirectToHome();
    }, 5000);
  }
}

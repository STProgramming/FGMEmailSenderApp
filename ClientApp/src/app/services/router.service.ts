import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { SessionModalComponent } from '../session-modal-component/session-modal.component';

@Injectable({
  providedIn: 'root'
})
export class RouterService {

  constructor(
    private readonly modalService: NgbModal,
    private readonly router: Router
  ) { }

  redirectToSessionCheck(){
    this.modalService.open(SessionModalComponent);
  }

  redirectToHome(){
    this.router.navigateByUrl('home');
  }

  redirectToAuthentication(){
    this.router.navigateByUrl('auth');
  }

  redirectToAuthComplete(){
    this.router.navigateByUrl('authentication-complete');
  }

  redirectLogin(){
    this.router.navigateByUrl('login');
  }

  redirectRegister(){
    this.router.navigateByUrl('register');
  }

  redirectWorkspace(){
    this.router.navigateByUrl('workspace');
  }
}

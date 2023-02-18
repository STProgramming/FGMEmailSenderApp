import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { delay } from 'rxjs/operators';
import { AuthenticationGuard } from '../guards/authentication.guard';
import { RouterService } from '../services/router.service';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.scss']
})
export class AuthComponent implements OnInit {
  resultAuthentication : Observable<boolean> = this.authGuard.$authentication;

  constructor(private readonly routerService: RouterService,
    private readonly authGuard: AuthenticationGuard) {}

  ngOnInit(): void {
    // nel caso andasse male qualcosa, il componente intercetta il cambio di stato e ti accompagna all'home
    this.routerService.redirectToAuthComplete();
    setTimeout(()=>{
      this.resultAuthentication.subscribe(authFlag=>{
        if(!authFlag){
          this.routerService.redirectToHome();
        }
      });
    }, 3000);
  }

}

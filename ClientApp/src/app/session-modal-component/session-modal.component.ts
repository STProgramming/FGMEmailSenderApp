import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { SessionGuard } from '../guards/session.guard';
import { CookieService } from '../services/cookie.service';
import { RouterService } from '../services/router.service';
import { SessionService } from '../services/session.service';
import { AuthenticationService } from '../services/authentication.service';

@Component({
  selector: 'app-session-modal',
  templateUrl: './session-modal.component.html',
  styleUrls: ['./session-modal.component.scss']
})
export class SessionModalComponent implements OnInit {
  cookiePolicy: boolean ;
  _sessionExist: Observable<boolean> = this.sessionGuard.sessionFlag$; 

  constructor(private readonly cookieService: CookieService,
    private readonly sessionGuard: SessionGuard,
    private readonly sessionService: SessionService,
    private routerService: RouterService,
    private readonly authService: AuthenticationService) { }

  ngOnInit(): void {
    this.authService.resumeAuthentication();
    setTimeout(()=>{
      this._sessionExist.subscribe(authFlag=>{
        if(!authFlag){
          this.sessionService.createSession();
          this.routerService.redirectToAuthentication();
        }
      });
    }, 2500);
  }
}

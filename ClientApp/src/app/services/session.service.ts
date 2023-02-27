import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Guid } from 'guid-typescript';
import { BehaviorSubject, Observable } from 'rxjs';
import { SessionGuard } from '../guards/session.guard';
import { Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { isThisTypeNode } from 'typescript';
import { SessionModalComponent } from '../session-modal-component/session-modal.component';
import { RouterService } from './router.service';

@Injectable({
  providedIn: 'root'
})
export class SessionService {
  NAMESESSION: string = environment._FGMSESSION;
  public id : Guid;

  constructor(
   private readonly routerService: RouterService) {}

  createSession(){
    let date = new Date();
    let dateBegin = date.getDate();
    let dateEnd = date.getDate()+1;
    this.id = Guid.create();
    //In futuro provvederemo a memorizzare anche la lingua scelta dall'utente
    const paramsSessionMemorize = {
      "dateBegin": dateBegin,
      "dateEnd": dateEnd,
      "idUser": this.id,
      "cookieAgreementPolicy": true
    };
    let stringParamsSession = JSON.stringify(paramsSessionMemorize);
    sessionStorage.setItem(this.NAMESESSION, stringParamsSession);
  }

  removeSession(){
    sessionStorage.removeItem(this.NAMESESSION);
  }
}

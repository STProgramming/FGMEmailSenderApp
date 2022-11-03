import { Component, OnInit } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CookieModalComponent } from '../cookie-modal-component/cookie-modal.component';
import { CookieService } from '../services/cookie.service';
import { SessionModalComponent } from '../session-modal-component/session-modal.component';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  cookiePolicy: boolean;

  constructor(
    private readonly modalService: NgbModal,
    private readonly cookieService: CookieService) { }

  ngOnInit(): void {
    this.modalService.dismissAll(SessionModalComponent);
    this.cookiePolicy = this.readValueCookiePolicy();
    if(!this.cookiePolicy){
      this.modalService.open(CookieModalComponent);
    }
  }

  readValueCookiePolicy(){
    let parseValueCookie = JSON.parse(this.cookieService.cookieGetValue("cookiePolicy"));
    if(parseValueCookie != null) {
      return parseValueCookie.result;
    }
    else return null;
  }
}

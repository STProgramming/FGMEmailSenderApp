import { Component, OnInit } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CookieService } from '../services/cookie.service';

@Component({
  selector: 'app-cookie-modal',
  templateUrl: './cookie-modal.component.html',
  styleUrls: ['./cookie-modal.component.scss']
})
export class CookieModalComponent implements OnInit {

  constructor(
    private readonly cookieService: CookieService,
    private readonly modalService: NgbModal) { }

  ngOnInit(): void {
  }

  agreeCookiePolicy(){
    this.cookieService.cookiePolicyService();
    this.modalService.dismissAll();
  }

}

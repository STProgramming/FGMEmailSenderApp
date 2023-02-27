import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-error-modal',
  templateUrl: './error-modal.component.html',
  styleUrls: ['./error-modal.component.scss']
})
export class ErrorModalComponent implements OnInit {
  @Input() public errorMessage: any;
  @Input() public flagUnAuth: boolean;
  public title: string;
  
  constructor(
    private readonly router: Router,
    private readonly modalService: NgbModal
  ) { }

  ngOnInit(): void {
  }

  onOkPressUnAuth(){
    //questa funzione ha il dovere di reindirizzare l'utente nell'index dell'applicazione
    this.router.navigateByUrl('');
  }

  onSave(){
    this.modalService.dismissAll();
  }
}

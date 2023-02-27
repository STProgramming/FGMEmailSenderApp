import { Injectable } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ErrorModalComponent } from '../error-modal-component/error-modal.component';

@Injectable({
  providedIn: 'root'
})
export class ErrorService {

  constructor(private modalService: NgbModal) { }

  public openErrorModal(error: any){
    const modalError = this.modalService.open(ErrorModalComponent);
    modalError.componentInstance.errorMessage = error;
    modalError.componentInstance.flagUnAuth = false;
  }

  public openErrorModalUnauthorized(error: any){
    const modalError = this.modalService.open(ErrorModalComponent);
    modalError.componentInstance.errorMessage = error;
    modalError.componentInstance.flagUnAuth = true;
  }
}

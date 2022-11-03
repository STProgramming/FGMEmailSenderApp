import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ErrorModalComponent } from '../error-modal-component/error-modal.component';
import { UserService } from '../services-api/user.service';

@Component({
  selector: 'app-email-confirmation',
  templateUrl: './email-confirmation.component.html',
  styleUrls: ['./email-confirmation.component.scss']
})
export class EmailConfirmationComponent implements OnInit {
  emailUser: string;
  token: string;
  message: string;

  constructor(
    private readonly userService: UserService,
    private readonly activatedRouter: ActivatedRoute,
    private readonly modalService: NgbModal) { 
      this.activatedRouter.queryParams.subscribe(
        (params) => {
          if(params && params.token && params.email){
            //todo validare se il token sia un token e se l'email sia effettivamente una email
            this.token = params.token;
            this.emailUser = params.email;
          }
          else{
            const modalError = this.modalService.open(ErrorModalComponent);
            modalError.componentInstance.errorMessage = "Non sei authorizzato ad entrare in questo componente.";
          }
        }
      )
  }

  async ngOnInit() {
    await this.userService.confirmationRegistration(this.token, this.emailUser).subscribe(
      (data: any)=>{
        this.message = data;
      }
    );
  }

}

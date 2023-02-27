import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { environment } from 'src/environments/environment';
import { ErrorModalComponent } from '../error-modal-component/error-modal.component';
import { ConfirmEmailInputModel } from '../interfaces/registrationUser';
import { ErrorService } from '../services/error.service';

@Component({
  selector: 'app-email-confirmation',
  templateUrl: './email-confirmation.component.html',
  styleUrls: ['./email-confirmation.component.scss']
})
export class EmailConfirmationComponent implements OnInit {
  emailUser: string;
  token: string;
  message: string;
  public REST_API_SERVER = environment._VARIABLE_HOST;
  public apiIdentityUserEndPoint = '/api/Identity/User/'

  constructor(
    private readonly httpClient: HttpClient,
    private readonly activatedRouter: ActivatedRoute,
    private readonly errorService: ErrorService) { 
      this.activatedRouter.queryParams.subscribe(
        (params) => {
          if(params && params.token && params.email){
            //todo validare se il token sia un token e se l'email sia effettivamente una email
            this.token = params.token;
            this.emailUser = params.email;            
          }
          else{
            const modalError = this.errorService.openErrorModalUnauthorized("Non sei authorizzato ad entrare in questo componente.");            
          }
        }
      )
  }

  async ngOnInit() {
    await this.httpClient.post(this.REST_API_SERVER+this.apiIdentityUserEndPoint+'ConfirmEmail', this.bindParamsIntoInterface()).subscribe(
      (data: any)=>{
        this.message = data;
      }
    );
  }

  private bindParamsIntoInterface(): ConfirmEmailInputModel{
    let confirmModel: ConfirmEmailInputModel = {
      token: this.token,
      email: this.emailUser,
    };

    return confirmModel;
  }

}

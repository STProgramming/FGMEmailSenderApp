import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { environment } from 'src/environments/environment';
import { RegistrationUser } from '../interfaces/registrationUser';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  public registrationFormNewUser: FormGroup = null;
  succeded: boolean = false;
  newUserEmail: string;
  public REST_API_SERVER = environment._VARIABLE_HOST;
  public apiIdentityUserEndPoint = '/api/Identity/User/'

  constructor(
    private readonly httpClient: HttpClient
  ) { }

  ngOnInit(): void {
    this.registrationFormNewUser = new FormGroup({
      Name: new FormControl('',[Validators.required, Validators.pattern('^[a-zA-Z \-\']+'), Validators.maxLength(50), Validators.minLength(2)]),
      LastName: new FormControl('',[Validators.required, Validators.pattern('^[a-zA-Z \-\']+'), Validators.maxLength(50), Validators.minLength(2)]),
      Email: new FormControl('',[Validators.required, Validators.email]),
      PhoneNumber: new FormControl('',[Validators.required, Validators.pattern(/^-?(0|[1-9]\d*)?$/)]),
      Password: new FormControl('',Validators.required),
      ConfirmPassword: new FormControl('',Validators.required),
      GDPRConfirmRead: new FormControl('',Validators.required)
    });
  }

  async onSubmit(registration: FormGroup){
    let password = registration.get('Password').value;
    let confPassword = registration.get('ConfirmPassword').value; 
    if(registration.valid && password === confPassword){
      let name = registration.get('Name').value;
      let surname = registration.get('LastName').value;
      let email = registration.get('Email').value;
      let phone = registration.get('PhoneNumber').value;
      phone = phone.toString(); 
      let registrationUser: RegistrationUser = {
        "Email": email,
        "Name": name,
        "LastName": surname,
        "PhoneNumber": phone,
        "Password": password
      };

      this.httpClient.post(this.REST_API_SERVER+this.apiIdentityUserEndPoint+'Signup', registrationUser).subscribe(
        (data: any) =>{
          if(data){
            this.succeded = true;
            let stringData = JSON.stringify(data);
            let parseData = JSON.parse(stringData);
            this.newUserEmail = parseData.Email;
          }
        }
      )
    }
  }

}

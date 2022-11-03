import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { LoginUser } from '../interfaces/LoginUser';
import { AuthenticationService } from '../services/authentication.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup = null;
  authenticated: Observable<boolean>;

  constructor(
    private readonly authService: AuthenticationService
  ) { }

  ngOnInit(): void {
    this.loginForm = new FormGroup({
      Email: new FormControl('',[Validators.email, Validators.required]),
      Password: new FormControl('',[Validators.required, Validators.minLength(8)]),
      LoginStores: new FormControl()
    });
  }

  onSubmit(login: FormGroup){
    if(login.valid) {
      let email: string = login.get('Email').value;
      let password: string = login.get('Password').value;
      let loginStore: boolean = login.get('LoginStores').value;
      if(loginStore == null){
        loginStore = false;
      }
      let loginFormat: LoginUser = {
        "Email": email,
        "Password": password,
        "LoginStores": loginStore
      };
      this.authService.loginAuthentication(loginFormat);
    }
  }

}

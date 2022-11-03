import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { ErrorModalComponent } from '../error-modal-component/error-modal.component';
import { Country, CountryInput } from '../interfaces/Country';
import { Department, DepartmentInput } from '../interfaces/Department';

@Injectable({
  providedIn: 'root'
})
export class SeoUserService {
  public REST_API_SERVER = environment._VARIABLE_HOST;

  constructor(
    private readonly httpClient: HttpClient,
    private readonly modalService: NgbModal
  ) {}

  private openModalError(error: HttpErrorResponse){
    console.log(error);
    return throwError(error);
  }

  public GetAllCountries(){
    return this.httpClient.get<Country[]>(this.REST_API_SERVER+'/api/Seo/GetListCountries', {headers: null} ).pipe(catchError(this.openModalError));
  }

  public GetAllDepartments(){
    return this.httpClient.get<Department[]>(this.REST_API_SERVER+'/api/Seo/GetListDepartments', {headers: null}).pipe(catchError(this.openModalError));
  }

  public GetDepartmentDetails(idDepartment: number){
    return this.httpClient.get<Department>(this.REST_API_SERVER+'/api/Seo/GetDepartmentDetails/?idDepartment='+idDepartment, {headers: null}).pipe(catchError(this.openModalError));
  }

  public GetCountryDetails(idCountry: number){
    return this.httpClient.get<Country>(this.REST_API_SERVER+'/api/Seo/GetCountryDetails/?idCountry='+idCountry, {headers: null}).pipe(catchError(this.openModalError));
  }

  public CreateNewCountry(newCountry: CountryInput){
    return this.httpClient.post(this.REST_API_SERVER+'/api/Seo/CreateCountry', newCountry, {headers: null}).pipe(catchError(this.openModalError));
  }

  public CreateNewDepartment(newDepartment: DepartmentInput){
    return this.httpClient.post(this.REST_API_SERVER+'/api/Seo/CreateDepartment', newDepartment, {headers: null}).pipe(catchError(this.openModalError));
  }

  public EditCountry(editCountry: CountryInput, idCountry: number){
    return this.httpClient.put(this.REST_API_SERVER+'/api/Seo/EditCountry?IdCountry='+idCountry, editCountry, {headers: null}).pipe(catchError(this.openModalError));
  }

  public EditDepartment(editDepartment: DepartmentInput, idDepartment: number){
    return this.httpClient.put(this.REST_API_SERVER+'/api/Seo/EditDepartment?IdDepartment='+idDepartment, editDepartment, {headers: null}).pipe(catchError(this.openModalError));
  }

  public DeleteCountry(idCountry: number){
    return this.httpClient.delete(this.REST_API_SERVER+'/api/Seo/DeleteCountry?IdCountry='+idCountry, {headers: null}).pipe(catchError(this.openModalError));
  }

  public DeleteDepartment(idDepartment: number){
    return this.httpClient.delete(this.REST_API_SERVER+'/api/Seo/DeleteDepartment?IdDepartment='+idDepartment, {headers: null}).pipe(catchError(this.openModalError));
  }
}

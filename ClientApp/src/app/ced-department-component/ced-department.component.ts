import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { SeoUserService } from '../services-api/seo-user.service';
import { Department, DepartmentInput } from '../interfaces/Department';
import { Country } from '../interfaces/Country';
import { BehaviorSubject, Observable } from 'rxjs';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { isThisTypeNode } from 'typescript';

@Component({
  selector: 'app-ce-department',
  templateUrl: './ced-department.component.html',
  styleUrls: ['./ced-department.component.scss']
})
export class CedDepartmentComponent implements OnInit {
  //se Input actionRequired e' falso allora update altrimenti se vero create
  @Input() actionRequired: string;
  @Input() idDepartment: number | null = null;
  department: Department;
  countryDetails: Country;
  countriesList: Country[];
  formDepartment: FormGroup = null;
  downloadFlag$ = new BehaviorSubject<boolean>(false);
  _downloadFlag : Observable<boolean> = this.downloadFlag$;
  eventReloadDepartment$ = new BehaviorSubject<boolean>(false);
  
  constructor(private readonly seoServices: SeoUserService,
    private readonly modalService: NgbModal,
    private readonly router: Router) {}

  ngOnInit(): void {
    //initialization of form
    this.formDepartment = new FormGroup({
      name: new FormControl('', Validators.required),
      code: new FormControl('', Validators.required),
      city: new FormControl('', Validators.required),
      idCountry: new FormControl('', Validators.required)
    });
    this.getAllCountries(); 
    if(this.actionRequired ==='edit' && this.idDepartment != null){
      this.loadDepartmentDetails();
      this.loadCountryDetails();
      this._downloadFlag.subscribe(
        (data:any) => {
          if(data){
            this.fetchFormDepartmentDetails();
          }
        }
      );
    }
  }

  loadDepartmentDetails(){
    this.seoServices.GetDepartmentDetails(this.idDepartment).subscribe(
      (data: Department) => {
        if(data){
          this.department = data;
          this.downloadFlag$.next(true);
        }
      }
    );
  }

  loadCountryDetails(){
    this.seoServices.GetCountryDetails(this.idDepartment).subscribe(
      (data: Country) => {
        if(data){
          this.countryDetails = data;
        }
      }
    );
  }

  fetchFormDepartmentDetails(){
    this.formDepartment = new FormGroup({
      name: new FormControl(this.department.nameDepartment, Validators.required),
      code: new FormControl(this.department.codeDepartment, Validators.required),
      city: new FormControl(this.department.cityDepartment, Validators.required),
      idCountry: new FormControl('', Validators.required)
    });
  }

  getAllCountries(){
    this.seoServices.GetAllCountries().subscribe(
      (data: any) =>{
        if(data){
          this.countriesList = data;
        }
      }
    );
  }

  sendingData(formDep: FormGroup){
    if(formDep.valid){
      let name = formDep.get('name').value;
      let code = formDep.get('code').value;
      let city = formDep.get('city').value;
      let id = formDep.get('idCountry').value;
      let newDepartment: DepartmentInput = {
        DepartmentName: name,
        DepartmentCode: code,
        DepartmentCity: city,
        IdCountry: parseInt(id)
      };
      if(this.actionRequired){
        this.seoServices.CreateNewDepartment(newDepartment).subscribe();
        this.onEndRequest();
      }
      else{
        this.seoServices.EditDepartment(newDepartment, this.idDepartment).subscribe();
        this.onEndRequest();
      }
    }
  }

  deleteDepartment(){
    this.seoServices.DeleteDepartment(this.idDepartment).subscribe();
    this.onEndRequest();
  }

  refuseDelete(){
    this.modalService.dismissAll();
  }

  onEndRequest(){
    this.eventReloadDepartment$.next(true);
    this.modalService.dismissAll();
    this.eventReloadDepartment$.next(false);
  }
}
